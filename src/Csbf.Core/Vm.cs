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

    public void Load(string src)
    {
        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        var ops = Optimizer.Optimize(Parser.Parse(src));
        Load(ops);
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
    }

    private void Execute(Op op)
    {
        switch (op.Kind)
        {
            case OpKind.IncPtr:
                MovePointer(op.Arg);
                break;
            case OpKind.DecPtr:
                MovePointer(-op.Arg);
                break;
            case OpKind.IncByte:
                _memory[Dp] += (byte)op.Arg;
                break;
            case OpKind.DecByte:
                _memory[Dp] -= (byte)op.Arg;
                break;
            case OpKind.Out:
                io?.Write(_memory[Dp]);
                break;
            case OpKind.In:
                _memory[Dp] = io?.Read() ?? 0;
                break;
            case OpKind.Jz:
                if (_memory[Dp] == 0)
                {
                    Ip = op.Arg;
                    return;
                }

                break;
            case OpKind.Jnz:
                if (_memory[Dp] != 0)
                {
                    Ip = op.Arg;
                    return;
                }

                break;
            default:
                throw new InvalidOperationException("Unsupported OpKind");
        }

        Ip++;
    }

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