namespace Csbf.Core;

public static class Optimizer
{
    public static IReadOnlyList<Op> Optimize(IEnumerable<Op> src)
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

            if (op.Kind is OpKind.IncPtr or OpKind.DecPtr
                or OpKind.IncByte or OpKind.DecByte)
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

        // Second pass: rewrite jump targets through the index map so loops
        // still point at the right instructions after runs were collapsed.
        for (var r = 0; r < result.Count; r++)
        {
            var op = result[r];

            if (op.Kind is OpKind.Jz or OpKind.Jnz)
            {
                result[r] = new Op(op.Kind, map[op.Arg]);
            }
        }

        return result;
    }
}
