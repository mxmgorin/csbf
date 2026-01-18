namespace Csbf.Core;

public class Vm(int memSize = 30_000)
{
    /// Instruction pointer
    private int _ip;

    /// data pointer
    private int _dp;

    private readonly byte[] _mem = new byte[memSize];
    private VmOp[] _program = [];

    public void Load(VmOp[] program)
    {
        _program = program;
    }

    public void Step(Func<byte>? input = null, Action<byte>? output = null)
    {
        if (_ip >= _program.Length)
        {
            return;
        }
        
        ref readonly var op = ref _program[_ip];
        switch (op.Kind)
        {
            case VmOpKind.IncPtr:
                _dp++;
                break;
            case VmOpKind.DecPtr:
                _dp--;
                break;
            case VmOpKind.IncByte:
                _mem[_dp]++;
                break;
            case VmOpKind.DecByte:
                _mem[_dp]--;
                break;
            case VmOpKind.Out:
                output?.Invoke(_mem[_dp]);
                break;
            case VmOpKind.In:
                _mem[_dp] = input?.Invoke() ?? 0;
                break;
            case VmOpKind.Jz:
                if (_mem[_dp] == 0)
                {
                    _ip = op.Arg;
                    return;
                }

                break;
            case VmOpKind.Jnz:
                if (_mem[_dp] != 0)
                {
                    _ip = op.Arg;
                    return;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _ip++;
    }
}