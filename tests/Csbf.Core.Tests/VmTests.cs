namespace Csbf.Core.Tests;

public class VmTests
{
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
