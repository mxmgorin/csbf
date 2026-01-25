using Csbf.Core;

namespace Csbf.Cli;

public class ConsoleIo : IVmIo
{
    public byte Read()
    {
        var ch = Console.Read();

        if (ch < 0)
        {
            return 0; // EOF == 0 is conventional in BF
        }

        return (byte)ch;
    }

    public void Write(byte value)
    {
        Console.Write((char)value);
    }
}