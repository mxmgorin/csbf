namespace Csbf.Core.Tests;

public class OptimizerTests
{
    [Fact]
    public void Peephole_CollapsesDec()
    {
        var src = new[] { new Op(OpKind.DecByte, 1), new Op(OpKind.DecByte, 1) };
        var ops = Optimizer.Optimize(src);

        var op = Assert.Single(ops);
        Assert.Equal(OpKind.DecByte, op.Kind);
        Assert.Equal(2, op.Arg);
    }
}