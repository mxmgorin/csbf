using Csbf.Core;

namespace Csbf.Cli;

public sealed class ConsoleIo : IVmIo
{
    private readonly Stream _input;
    private readonly Stream _output;

    public ConsoleIo()
        : this(Console.OpenStandardInput(), Console.OpenStandardOutput())
    {
    }

    // Injectable streams keep this testable and let the VM move raw bytes
    // instead of reinterpreting them as UTF-16 chars.
    public ConsoleIo(Stream input, Stream output)
    {
        _input = input;
        _output = output;
    }

    public byte Read()
    {
        var b = _input.ReadByte();

        // EOF == 0 is conventional in BF.
        return b < 0 ? (byte)0 : (byte)b;
    }

    public void Write(byte value)
    {
        _output.WriteByte(value);
        _output.Flush();
    }
}
