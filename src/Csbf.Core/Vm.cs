namespace Csbf.Core;

public class Vm(Func<byte>? input = null, Action<byte>? output = null, int memSize = 30_000)
{
    /// Instruction pointer
    public int Ip { get; private set; }

    /// data pointer
    public int Dp { get; private set; }

    private readonly byte[] _mem = new byte[memSize];
    private Op[] _program = [];

    public byte Current => _mem[Dp];

    public void Load(IReadOnlyCollection<Op> program)
    {
        _program = program.ToArray();
    }

    public bool HasProgram()
    {
        return _program.Length != 0;
    }

    public bool ProgramFinished()
    {
        return Ip >= _program.Length;
    }

    public ReadOnlySpan<byte> Read(uint from, uint len)
    {
        if (from >= _mem.Length)
            throw new ArgumentOutOfRangeException(nameof(from));

        var end = Math.Min(from + len, _mem.Length);
        return new ReadOnlySpan<byte>(_mem, (int)from, (int)(end - from));
    }

    public Op? Peek()
    {
        if (ProgramFinished())
            return null;

        if (Ip < 0 || Ip >= _program.Length)
            return null;

        return _program[Ip];
    }

    public void Step()
    {
        if (ProgramFinished())
        {
            return;
        }

        ref readonly var op = ref _program[Ip];
        Execute(op);

        Ip++;
    }

    private void Execute(Op op)
    {
        switch (op.Kind)
        {
            case OpKind.IncPtr:
                Dp++;
                break;
            case OpKind.DecPtr:
                Dp--;
                break;
            case OpKind.IncByte:
                _mem[Dp]++;
                break;
            case OpKind.DecByte:
                _mem[Dp]--;
                break;
            case OpKind.Out:
                output?.Invoke(_mem[Dp]);
                break;
            case OpKind.In:
                _mem[Dp] = input?.Invoke() ?? 0;
                break;
            case OpKind.Jz:
                if (_mem[Dp] == 0)
                {
                    Ip = op.Arg;
                }

                break;
            case OpKind.Jnz:
                if (_mem[Dp] != 0)
                {
                    Ip = op.Arg;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}