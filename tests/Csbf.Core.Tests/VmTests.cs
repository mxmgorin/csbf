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
    public void Run_MovesValueBetweenCells_WithOptimizedLoop()
    {
        var vm = new Vm();
        vm.Load("+++[>+<-]"); // move 3 from cell 0 into cell 1

        RunToHalt(vm);

        var mem = vm.ReadMemory(0, 2);
        Assert.Equal((byte)0, mem[0]);
        Assert.Equal((byte)3, mem[1]);
    }

    [Fact]
    public void Step_Throws_WhenPointerGoesBelowZero()
    {
        var vm = new Vm(memorySize: 8);
        vm.Load("<"); // AddPtr(-1): 0 -> -1

        Assert.Throws<BrainfuckRuntimeException>(() => vm.Step());
    }

    [Fact]
    public void Step_WrapsByte_OnDecrementBelowZero()
    {
        var vm = new Vm();
        vm.Load("-"); // AddByte(-1): 0 -> 255

        vm.Step();

        Assert.Equal((byte)255, vm.ReadMemory(0, 1)[0]);
    }

    [Fact]
    public void Step_Throws_WhenPointerGoesPastEnd()
    {
        var vm = new Vm(memorySize: 2);
        // '+' between the moves keeps them as separate ops (adjacent same-kind
        // ops would otherwise be collapsed by the optimizer).
        vm.Load(">+>"); // 0 -> 1 (ok), inc cell, then 1 -> 2 (out of bounds)

        vm.Step();
        Assert.Equal(1, vm.Dp);
        vm.Step();
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
