using Csbf.Core;

namespace Csbf.Cli.Tests;

public class ConsoleIoTests
{
    [Fact]
    public void Write_EmitsRawBytes_NotReinterpretedChars()
    {
        // 'é' (U+00E9) is 0xC3 0xA9 in UTF-8. Emitting those bytes must produce
        // exactly those bytes — the old `Console.Write((char)value)` would have
        // turned 0xC3 alone into the two-byte UTF-8 encoding of 'Ã'.
        using var output = new MemoryStream();
        var io = new ConsoleIo(Stream.Null, output);

        io.Write(0xC3);
        io.Write(0xA9);

        Assert.Equal(new byte[] { 0xC3, 0xA9 }, output.ToArray());
    }

    [Fact]
    public void ReadWrite_RoundTripsHighBytes_ThroughVm()
    {
        using var input = new MemoryStream([0xC3, 0xA9]);
        using var output = new MemoryStream();
        var vm = new Vm(new ConsoleIo(input, output));
        vm.Load(",.,."); // read+write, twice

        while (!vm.Halted())
        {
            vm.Step();
        }

        Assert.Equal(new byte[] { 0xC3, 0xA9 }, output.ToArray());
    }

    [Fact]
    public void Read_ReturnsZero_AtEndOfInput()
    {
        var io = new ConsoleIo(Stream.Null, Stream.Null);

        Assert.Equal((byte)0, io.Read());
    }
}
