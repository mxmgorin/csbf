namespace Csbf.Core;

/// <summary>
/// Optimizer passes, individually toggleable. Each flag enables one
/// independent transformation, so callers can disable any single pass
/// (e.g. to compare output or debug a miscompile).
/// </summary>
[Flags]
public enum OptPasses
{
    /// <summary>Run the parsed ops verbatim — no optimization.</summary>
    None = 0,

    /// <summary>Peephole run-length coalescing of AddPtr/AddByte.</summary>
    Collapse = 1 << 0,

    /// <summary>Lower clear loops <c>[-]</c> / <c>[+]</c> to <c>SetByte 0</c>.</summary>
    ClearLoop = 1 << 1,

    /// <summary>Lower scan loops <c>[&gt;]</c> / <c>[&lt;]</c> to <c>ScanPtr</c>.</summary>
    ScanLoop = 1 << 2,

    /// <summary>All passes enabled (the default).</summary>
    All = Collapse | ClearLoop | ScanLoop,
}

public static class Optimizer
{
    /// <summary>
    /// Optimize <paramref name="src"/>, applying only the passes selected in
    /// <paramref name="passes"/>. Passes compose: collapsing runs first lets
    /// multi-step scans (<c>[&gt;&gt;]</c>) be recognized, but each pass is
    /// independent and safe to disable on its own.
    /// </summary>
    public static IReadOnlyList<Op> Optimize(IEnumerable<Op> src, OptPasses passes = OptPasses.All)
    {
        var ops = passes.HasFlag(OptPasses.Collapse)
            ? Collapse(src)
            : src as IReadOnlyList<Op> ?? src.ToArray();

        var lowerClear = passes.HasFlag(OptPasses.ClearLoop);
        var lowerScan = passes.HasFlag(OptPasses.ScanLoop);

        return lowerClear || lowerScan ? LowerLoops(ops, lowerClear, lowerScan) : ops;
    }

    /// <summary>
    /// Peephole pass: collapse adjacent same-kind AddPtr/AddByte runs into a
    /// single op (dropping any that net to zero), then re-patch jump targets
    /// through the index map so loops stay matched.
    /// </summary>
    public static IReadOnlyList<Op> Collapse(IEnumerable<Op> src)
    {
        var ops = src as IReadOnlyList<Op> ?? src.ToArray();
        var result = new List<Op>(ops.Count);

        // map[k] = index in `result` that old index `k` now corresponds to.
        // Size is ops.Count + 1 so a jump target pointing just past the last
        // op (loop exit at end of program) also maps cleanly.
        var map = new int[ops.Count + 1];

        var i = 0;
        while (i < ops.Count)
        {
            var op = ops[i];

            if (op.Kind is OpKind.AddPtr or OpKind.AddByte)
            {
                var accum = op.Arg;
                var j = i + 1;

                while (j < ops.Count && ops[j].Kind == op.Kind)
                {
                    accum += ops[j].Arg;
                    j++;
                }

                // Whether or not the run is emitted, every old index it covers
                // maps to the current output position: the collapsed op if we
                // emit one, otherwise the next op that gets emitted.
                var newIndex = result.Count;

                if (accum != 0)
                {
                    result.Add(new Op(op.Kind, accum));
                }

                for (var k = i; k < j; k++)
                {
                    map[k] = newIndex;
                }

                i = j;
                continue;
            }

            map[i] = result.Count;
            result.Add(op);
            i++;
        }

        map[ops.Count] = result.Count;
        RepatchJumps(result, map);
        return result;
    }

    /// <summary>
    /// Lower recognized single-op loop idioms to dedicated ops:
    /// <c>[-]</c>/<c>[+]</c> → <see cref="OpKind.SetByte"/> 0 (clear),
    /// <c>[&gt;]</c>/<c>[&lt;]</c> → <see cref="OpKind.ScanPtr"/> (memchr-style skip).
    /// Any loop whose body is not exactly one recognized op is left intact, and
    /// jump targets are re-patched through the index map afterwards.
    /// </summary>
    public static IReadOnlyList<Op> LowerLoops(IReadOnlyList<Op> ops, bool lowerClear = true, bool lowerScan = true)
    {
        var result = new List<Op>(ops.Count);
        var map = new int[ops.Count + 1];

        var i = 0;
        while (i < ops.Count)
        {
            if (TryMatchLoop(ops, i, lowerClear, lowerScan, out var lowered))
            {
                var newIndex = result.Count;
                result.Add(lowered);

                // The three consumed ops (Jz, body, Jnz) all fold into the one
                // lowered op, so any jump landing on them retargets to it.
                map[i] = newIndex;
                map[i + 1] = newIndex;
                map[i + 2] = newIndex;
                i += 3;
                continue;
            }

            map[i] = result.Count;
            result.Add(ops[i]);
            i++;
        }

        map[ops.Count] = result.Count;
        RepatchJumps(result, map);
        return result;
    }

    /// <summary>
    /// Match a self-contained single-op loop starting at <paramref name="i"/>:
    /// exactly <c>Jz, body, Jnz</c> where the Jz exits just past the Jnz and the
    /// Jnz jumps back to the Jz. That shape guarantees the body runs every
    /// iteration with the cell at the pointer as the guard.
    /// </summary>
    private static bool TryMatchLoop(IReadOnlyList<Op> ops, int i, bool lowerClear, bool lowerScan, out Op lowered)
    {
        lowered = default;

        if (i + 2 >= ops.Count)
        {
            return false;
        }

        var open = ops[i];
        var body = ops[i + 1];
        var close = ops[i + 2];

        // Structural check: a three-op loop that only contains `body`.
        if (open.Kind != OpKind.Jz || close.Kind != OpKind.Jnz || open.Arg != i + 3 || close.Arg != i)
        {
            return false;
        }

        switch (body.Kind)
        {
            // [-] / [+]: only a ±1 step is a guaranteed clear — e.g. [--] loops
            // forever on odd start values, so we deliberately don't lower it.
            case OpKind.AddByte when lowerClear && (body.Arg == 1 || body.Arg == -1):
                lowered = new Op(OpKind.SetByte, 0);
                return true;

            // [>] / [<] (and collapsed strides like [>>]): scan by a fixed stride
            // until a zero cell.
            case OpKind.AddPtr when lowerScan && body.Arg != 0:
                lowered = new Op(OpKind.ScanPtr, body.Arg);
                return true;

            default:
                return false;
        }
    }

    // Rewrite jump targets through the index map so loops still point at the
    // right instructions after ops were collapsed or lowered.
    private static void RepatchJumps(List<Op> ops, int[] map)
    {
        for (var r = 0; r < ops.Count; r++)
        {
            var op = ops[r];

            if (op.Kind is OpKind.Jz or OpKind.Jnz)
            {
                ops[r] = new Op(op.Kind, map[op.Arg]);
            }
        }
    }
}
