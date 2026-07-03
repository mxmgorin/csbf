namespace Csbf.Core;

public class Vm(IVmIo? io = null, int memorySize = 30_000)
{
    /// <summary>
    /// Instruction pointer
    /// </summary>
    public int Ip { get; private set; }

    /// <summary>
    /// Data pointer
    /// </summary>
    public int Dp { get; private set; }

    public byte Current => _memory[Dp];

    public Op[] Ops { get; private set; } = [];

    private readonly byte[] _memory = new byte[memorySize];

    // --- Time-travel (step-back) support -------------------------------------
    // The VM mutates only Ip, Dp, and at most one cell per op, so every step is
    // exactly reversible. When recording is enabled each executed op pushes a
    // delta into a bounded ring buffer; StepBack pops and undoes them.
    private StepDelta[]? _history;
    private int _historyStart;    // ring index of the oldest recorded delta
    private int _historyCount;    // number of deltas currently held
    private bool _historyDropped; // true once the ring has overwritten old deltas

    /// <summary>Whether per-step recording is enabled (required for <see cref="StepBack"/>).</summary>
    public bool RecordingEnabled => _history is not null;

    /// <summary>Number of steps that can currently be reversed.</summary>
    public int HistoryDepth => _historyCount;

    /// <summary>True if the bounded history has dropped older states to stay within capacity.</summary>
    public bool HistoryDropped => _historyDropped;

    public void Load(string src)
    {
        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        var ops = Optimizer.Optimize(Parser.Parse(src));
        Load(ops);
    }

    /// <summary>
    /// Enable time-travel recording. Each executed op pushes a reversible delta
    /// into a bounded ring buffer of the given <paramref name="capacity"/>; once
    /// full, the oldest states are dropped (see <see cref="HistoryDropped"/>).
    /// </summary>
    public void EnableRecording(int capacity = 10_000)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be positive");
        }

        _history = new StepDelta[capacity];
        ClearHistory();
    }

    /// <summary>Disable recording and discard any recorded history.</summary>
    public void DisableRecording()
    {
        _history = null;
        ClearHistory();
    }

    /// <summary>
    /// Reverse the most recently executed op, restoring <see cref="Ip"/>,
    /// <see cref="Dp"/>, and any cell it changed. Returns <c>false</c> if
    /// recording is off or no history remains. Note: reversing an input
    /// (<c>,</c>) op restores the cell but does not un-consume the input byte.
    /// </summary>
    public bool StepBack()
    {
        if (_history is null || _historyCount == 0)
        {
            return false;
        }

        var index = (_historyStart + _historyCount - 1) % _history.Length;
        var delta = _history[index];
        _historyCount--;

        Ip = delta.Ip;
        Dp = delta.Dp;

        if (delta.CellIndex >= 0)
        {
            _memory[delta.CellIndex] = delta.CellValue;
        }

        return true;
    }

    public bool Loaded()
    {
        return Ops.Length != 0;
    }

    public bool Halted()
    {
        return Ip >= Ops.Length;
    }

    public ReadOnlySpan<byte> ReadMemory(uint from, uint len)
    {
        if (from >= _memory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(from));
        }

        var end = Math.Min(from + len, _memory.Length);

        return new ReadOnlySpan<byte>(_memory, (int)from, (int)(end - from));
    }


    public Op? Peek()
    {
        if (Halted())
        {
            return null;
        }

        if (!Loaded())
        {
            return null;
        }

        return Ops[Ip];
    }

    public void Step()
    {
        if (Halted())
        {
            return;
        }

        var op = Ops[Ip];
        Execute(op);
    }

    private void Load(IReadOnlyCollection<Op> ops)
    {
        Ops = ops.ToArray();
        // Recorded deltas reference the previous program's op indices.
        ClearHistory();
    }

    private void Execute(Op op)
    {
        var ipBefore = Ip;
        var dpBefore = Dp;
        var cellIndex = -1; // -1 = this op changed no cell
        byte cellOld = 0;
        var advance = true; // false when a taken jump sets Ip directly

        switch (op.Kind)
        {
            case OpKind.AddPtr:
                // May throw on out-of-bounds; nothing is recorded in that case.
                MovePointer(op.Arg);
                break;
            case OpKind.AddByte:
                cellIndex = Dp;
                cellOld = _memory[Dp];
                // Signed delta; the cast truncates to 8 bits, giving BF's wrap-around.
                _memory[Dp] = (byte)(_memory[Dp] + op.Arg);
                break;
            case OpKind.Out:
                io?.Write(_memory[Dp]);
                break;
            case OpKind.In:
                cellIndex = Dp;
                cellOld = _memory[Dp];
                _memory[Dp] = io?.Read() ?? 0;
                break;
            case OpKind.Jz:
                if (_memory[Dp] == 0)
                {
                    Ip = op.Arg;
                    advance = false;
                }

                break;
            case OpKind.Jnz:
                if (_memory[Dp] != 0)
                {
                    Ip = op.Arg;
                    advance = false;
                }

                break;
            default:
                throw new InvalidOperationException("Unsupported OpKind");
        }

        if (advance)
        {
            Ip++;
        }

        Record(ipBefore, dpBefore, cellIndex, cellOld);
    }

    private void Record(int ip, int dp, int cellIndex, byte cellValue)
    {
        if (_history is null)
        {
            return;
        }

        var slot = (_historyStart + _historyCount) % _history.Length;

        if (_historyCount == _history.Length)
        {
            // Ring is full: overwrite the oldest delta and advance the start.
            _historyStart = (_historyStart + 1) % _history.Length;
            _historyDropped = true;
        }
        else
        {
            _historyCount++;
        }

        _history[slot] = new StepDelta(ip, dp, cellIndex, cellValue);
    }

    private void ClearHistory()
    {
        _historyStart = 0;
        _historyCount = 0;
        _historyDropped = false;
    }

    /// <summary>
    /// A reversible record of one executed op: the Ip/Dp to restore, plus the
    /// single cell it changed (<see cref="CellIndex"/> = -1 when none changed).
    /// </summary>
    private readonly record struct StepDelta(int Ip, int Dp, int CellIndex, byte CellValue);

    private void MovePointer(int delta)
    {
        var target = Dp + delta;

        if (target < 0 || target >= _memory.Length)
        {
            throw new BrainfuckRuntimeException(
                $"Data pointer moved out of bounds to {target} (tape size {_memory.Length}) at instruction 0x{Ip:X}.");
        }

        Dp = target;
    }
}