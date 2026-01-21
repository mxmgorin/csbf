namespace Csbf.Core;

public class Vm(Func<byte>? input = null, Action<byte>? output = null, int memorySize = 30_000)
{
    /// Instruction pointer
    public int Ip { get; private set; }

    /// data pointer
    public int Dp { get; private set; }

    private readonly byte[] _memory = new byte[memorySize];
    private Op[] _ops = [];

    public byte Current => _memory[Dp];
    
    public Op[] Ops => _ops;

    public void Load(IReadOnlyCollection<Op> ops)
    {
        _ops = ops.ToArray();
    }

    public void Load(string src)
    {
        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        var ops = Parser.Parse(src);
        Load(ops);
    }

    public bool Loaded()
    {
        return _ops.Length != 0;
    }

    public bool Halted()
    {
        return Ip >= _ops.Length;
    }

    public ReadOnlySpan<byte> ReadMemory(uint from, uint len)
    {
        if (from >= _memory.Length)
            throw new ArgumentOutOfRangeException(nameof(from));

        var end = Math.Min(from + len, _memory.Length);
        return new ReadOnlySpan<byte>(_memory, (int)from, (int)(end - from));
    }


    public Op? Peek()
    {
        if (Halted())
            return null;

        if (Ip < 0 || Ip >= _ops.Length)
            return null;

        return _ops[Ip];
    }

    public void Step()
    {
        if (Halted())
        {
            return;
        }

        var op = _ops[Ip];
        Execute(op);

    }

    private void Execute(Op op)
    {
        switch (op.Kind)
        {
            case OpKind.IncPtr:
                Dp += op.Arg;
                break;
            case OpKind.DecPtr:
                Dp -= op.Arg;
                break;
            case OpKind.IncByte:
                _memory[Dp] += (byte)op.Arg;
                break;
            case OpKind.DecByte:
                _memory[Dp] -= (byte)op.Arg;
                break;
            case OpKind.Out:
                output?.Invoke(_memory[Dp]);
                break;
            case OpKind.In:
                _memory[Dp] = input?.Invoke() ?? 0;
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
                throw new ArgumentOutOfRangeException();
        }
        
        Ip++;
    }
}