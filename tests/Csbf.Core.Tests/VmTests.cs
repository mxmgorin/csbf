namespace Csbf.Core.Tests;

public class VmTests
{
    private static void RunToHalt(Vm vm)
    {
        while (!vm.Halted())
        {
            vm.Step();
        }
    }

    [Fact]
    public void Run_HelloWorldSample_PrintsHelloWorld()
    {
        var io = new CapturingIo();
        var vm = new Vm(io);
        var path = Path.Combine(AppContext.BaseDirectory, "samples", "hello_world.bf");
        vm.Load(File.ReadAllText(path));

        RunToHalt(vm);

        Assert.Equal("Hello World!\n", io.OutputAsString());
    }

    [Fact]
    public void Run_EchoesInput_RoundTrip()
    {
        // ',.' reads a byte then writes it; repeated to echo two input bytes.
        var io = new CapturingIo((byte)'H', (byte)'i');
        var vm = new Vm(io);
        vm.Load(",.,.");

        RunToHalt(vm);

        Assert.Equal("Hi", io.OutputAsString());
    }

    [Fact]
    public void Step_Throws_WhenPointerGoesBelowZero()
    {
        var vm = new Vm(memorySize: 8);
        vm.Load("<"); // DecPtr: 0 -> -1

        Assert.Throws<BrainfuckRuntimeException>(() => vm.Step());
    }

    [Fact]
    public void Step_Throws_WhenPointerGoesPastEnd()
    {
        var vm = new Vm(memorySize: 2);
        vm.Load(">>"); // 0 -> 1 (ok), then 1 -> 2 (out of bounds)

        vm.Step();
        Assert.Equal(1, vm.Dp);
        Assert.Throws<BrainfuckRuntimeException>(() => vm.Step());
    }

    [Fact]
    public void Step_KeepsPointerValid_WhenMoveFails()
    {
        var vm = new Vm(memorySize: 8);
        vm.Load("<");

        Assert.Throws<BrainfuckRuntimeException>(() => vm.Step());
        Assert.Equal(0, vm.Dp); // pointer was not left in an invalid state
    }

    [Fact]
    public void Step_AllowsPointerAtLastCell()
    {
        var vm = new Vm(memorySize: 2);
        vm.Load(">"); // 0 -> 1, the last valid index

        vm.Step();
        Assert.Equal(1, vm.Dp);
    }
}
