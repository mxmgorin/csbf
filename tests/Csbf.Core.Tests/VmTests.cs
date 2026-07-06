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
    public void Run_ClearLoop_ZeroesCell()
    {
        var vm = new Vm();
        vm.Load("+++[-]"); // AddByte(3), SetByte(0)

        RunToHalt(vm);

        Assert.Equal((byte)0, vm.ReadMemory(0, 1)[0]);
    }

    [Fact]
    public void Run_ScanLoop_StopsAtFirstZeroToTheRight()
    {
        var vm = new Vm();
        // cells: 1 1 1 0, pointer back to 0, then [>] scans right to the zero.
        vm.Load("+>+>+<<[>]");

        RunToHalt(vm);

        Assert.Equal(3, vm.Dp);
        var mem = vm.ReadMemory(0, 4);
        Assert.Equal((byte)1, mem[0]);
        Assert.Equal((byte)0, mem[3]);
    }

    [Fact]
    public void Run_ScanLoop_StopsAtFirstZeroToTheLeft()
    {
        var vm = new Vm();
        // cells: c1=0, c2=1, c3=1, start at c3, then [<] scans left to c1.
        vm.Load(">>+>+[<]");

        RunToHalt(vm);

        Assert.Equal(1, vm.Dp);
    }

    [Fact]
    public void StepBack_RestoresCellClearedBySetByte()
    {
        var vm = new Vm();
        vm.EnableRecording();
        vm.Load("+++[-]"); // AddByte(3) then SetByte(0)

        RunToHalt(vm);
        Assert.Equal((byte)0, vm.ReadMemory(0, 1)[0]);

        Assert.True(vm.StepBack()); // undo the clear
        Assert.Equal((byte)3, vm.ReadMemory(0, 1)[0]);
    }

    [Fact]
    public void StepBack_ReversesScan_InOneStep()
    {
        var vm = new Vm();
        vm.EnableRecording();
        vm.Load("+>+>+<<[>]"); // last op is a scan that moves the pointer 0 -> 3

        RunToHalt(vm);
        Assert.Equal(3, vm.Dp);

        Assert.True(vm.StepBack()); // the whole scan reverses as one step
        Assert.Equal(0, vm.Dp);
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

    [Fact]
    public void StepBack_ReturnsFalse_WhenRecordingDisabled()
    {
        var vm = new Vm();
        vm.Load("+");
        vm.Step();

        Assert.False(vm.RecordingEnabled);
        Assert.False(vm.StepBack());
        Assert.Equal((byte)1, vm.ReadMemory(0, 1)[0]); // state untouched
    }

    [Fact]
    public void StepBack_RestoresCellWrittenByAddByte()
    {
        var vm = new Vm();
        vm.EnableRecording();
        vm.Load("+"); // AddByte(+1): 0 -> 1

        vm.Step();
        Assert.Equal((byte)1, vm.ReadMemory(0, 1)[0]);

        Assert.True(vm.StepBack());
        Assert.Equal((byte)0, vm.ReadMemory(0, 1)[0]); // byte restored
        Assert.Equal(0, vm.Ip);
        Assert.False(vm.StepBack()); // back at the start
    }

    [Fact]
    public void StepBack_ReversesPointerMove()
    {
        var vm = new Vm();
        vm.EnableRecording();
        // '+' between the moves keeps them as separate ops (see Step_Throws... above).
        vm.Load(">+>"); // AddPtr(+1), AddByte(+1), AddPtr(+1)

        vm.Step(); // Dp 0 -> 1
        vm.Step(); // cell[1] 0 -> 1
        vm.Step(); // Dp 1 -> 2
        Assert.Equal(2, vm.Dp);

        Assert.True(vm.StepBack()); // undo Dp 1 -> 2
        Assert.Equal(1, vm.Dp);
        Assert.True(vm.StepBack()); // undo cell[1]
        Assert.Equal((byte)0, vm.ReadMemory(1, 1)[0]);
        Assert.True(vm.StepBack()); // undo Dp 0 -> 1
        Assert.Equal(0, vm.Dp);
    }

    [Fact]
    public void StepBack_AfterNSteps_RestoresExactStartState()
    {
        var vm = new Vm();
        vm.EnableRecording();
        vm.Load("+++[>+<-]>+"); // loops, pointer moves, and byte writes

        var startIp = vm.Ip;
        var startDp = vm.Dp;
        var startMem = vm.ReadMemory(0, 4).ToArray();

        var steps = 0;

        while (!vm.Halted())
        {
            vm.Step();
            steps++;
        }

        for (var i = 0; i < steps; i++)
        {
            Assert.True(vm.StepBack());
        }

        Assert.Equal(startIp, vm.Ip);
        Assert.Equal(startDp, vm.Dp);
        Assert.Equal(startMem, vm.ReadMemory(0, 4).ToArray());
        Assert.False(vm.StepBack()); // history exhausted
    }

    [Fact]
    public void Recording_BoundsHistory_AndReportsDropped()
    {
        var vm = new Vm();
        vm.EnableRecording(capacity: 2);
        vm.Load("+>-"); // three ops -> three deltas, but the ring holds only two

        vm.Step();
        vm.Step();
        vm.Step();

        Assert.True(vm.HistoryDropped);
        Assert.Equal(2, vm.HistoryDepth);
        Assert.True(vm.StepBack());
        Assert.True(vm.StepBack());
        Assert.False(vm.StepBack()); // oldest delta was dropped
    }

    [Fact]
    public void Load_ClearsRecordedHistory()
    {
        var vm = new Vm();
        vm.EnableRecording();
        vm.Load("+");
        vm.Step();

        vm.Load("+"); // reloading resets history (op indices change)

        Assert.Equal(0, vm.HistoryDepth);
        Assert.False(vm.StepBack());
    }

    [Fact]
    public void EnableRecording_Throws_OnNonPositiveCapacity()
    {
        var vm = new Vm();
        Assert.Throws<ArgumentOutOfRangeException>(() => vm.EnableRecording(0));
    }
}
