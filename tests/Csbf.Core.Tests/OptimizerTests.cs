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

    [Fact]
    public void Optimize_CollapsesRuns_AndPatchesJumpTargets()
    {
        // "+++" collapses to one op, shifting every later index; the loop's
        // jump targets must be re-patched to stay matched.
        var ops = Optimizer.Optimize(Parser.Parse("+++[>+<-]"));

        Assert.Collection(ops,
            o => { Assert.Equal(OpKind.IncByte, o.Kind); Assert.Equal(3, o.Arg); },
            o => { Assert.Equal(OpKind.Jz, o.Kind); Assert.Equal(7, o.Arg); },   // exit -> past end
            o => Assert.Equal(OpKind.IncPtr, o.Kind),
            o => Assert.Equal(OpKind.IncByte, o.Kind),
            o => Assert.Equal(OpKind.DecPtr, o.Kind),
            o => Assert.Equal(OpKind.DecByte, o.Kind),
            o => { Assert.Equal(OpKind.Jnz, o.Kind); Assert.Equal(1, o.Arg); }); // back -> the Jz
    }
}