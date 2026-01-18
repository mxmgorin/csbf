namespace Csbf.Core;

public class Vm
{
    /// Instruction pointer
    private int _ip;

    /// data pointer
    private int _dp;

    private byte[] _mem;

    public Vm(int memSize = 30_000)
    {
        _mem = new byte[memSize];
        _ip = 0;
        _dp = 0;
    }

    public void Step()
    {
        _ip++;
    }
}