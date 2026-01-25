namespace Csbf.Core;

public interface IVmIo
{
    byte Read();
    void Write(byte value);
}