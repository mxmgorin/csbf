namespace Csbf.Core.Tests;

public class OptimizerTests
{
    [Fact]
    public void Peephole_CollapsesDec()
    {
        var src = new[] { new Op(OpKind.AddByte, -1), new Op(OpKind.AddByte, -1) };
        var ops = Optimizer.Optimize(src);

        var op = Assert.Single(ops);
        Assert.Equal(OpKind.AddByte, op.Kind);
        Assert.Equal(-2, op.Arg);
    }

    [Fact]
    public void Peephole_CancelsOpposingRuns()
    {
        // Signed ops let opposing moves cancel: "><" and "+-" both net to zero.
        var ops = Optimizer.Optimize(Parser.Parse("><+-"));

        Assert.Empty(ops);
    }

    [Theory]
    [InlineData("[-]")]
    [InlineData("[+]")]
    public void Optimize_LowersClearLoop_ToSetZero(string src)
    {
        var op = Assert.Single(Optimizer.Optimize(Parser.Parse(src)));

        Assert.Equal(OpKind.SetByte, op.Kind);
        Assert.Equal(0, op.Arg);
    }

    [Fact]
    public void Optimize_DoesNotLowerEvenDecrementLoop()
    {
        // [--] only clears even start values (it loops forever on odd ones), so
        // it must stay a real loop rather than becoming a clear.
        var ops = Optimizer.Optimize(Parser.Parse("[--]"));

        Assert.Collection(ops,
            o => Assert.Equal(OpKind.Jz, o.Kind),
            o => { Assert.Equal(OpKind.AddByte, o.Kind); Assert.Equal(-2, o.Arg); },
            o => Assert.Equal(OpKind.Jnz, o.Kind));
    }

    [Theory]
    [InlineData("[>]", 1)]
    [InlineData("[<]", -1)]
    [InlineData("[>>>]", 3)] // collapsed stride
    public void Optimize_LowersScanLoop_ToScanPtr(string src, int stride)
    {
        var op = Assert.Single(Optimizer.Optimize(Parser.Parse(src)));

        Assert.Equal(OpKind.ScanPtr, op.Kind);
        Assert.Equal(stride, op.Arg);
    }

    [Fact]
    public void Optimize_LeavesMultiOpLoopBody_Intact()
    {
        // A move loop has a multi-op body, so it is not a clear or scan.
        var ops = Optimizer.Optimize(Parser.Parse("[>+<-]"));

        Assert.Contains(ops, o => o.Kind == OpKind.Jz);
        Assert.DoesNotContain(ops, o => o.Kind is OpKind.SetByte or OpKind.ScanPtr);
    }

    [Fact]
    public void Optimize_LowersClearLoop_AndPatchesSurroundingJumps()
    {
        // The clear folds 3 ops into 1; an enclosing loop's jumps must re-map.
        var ops = Optimizer.Optimize(Parser.Parse("[[-]]"));

        Assert.Collection(ops,
            o => { Assert.Equal(OpKind.Jz, o.Kind); Assert.Equal(3, o.Arg); },   // exit past Jnz
            o => Assert.Equal(OpKind.SetByte, o.Kind),
            o => { Assert.Equal(OpKind.Jnz, o.Kind); Assert.Equal(0, o.Arg); }); // back to Jz
    }

    [Fact]
    public void Optimize_None_RunsOpsVerbatim()
    {
        // With every pass off, even a clear loop stays a raw three-op loop.
        var ops = Optimizer.Optimize(Parser.Parse("[-]"), OptPasses.None);

        Assert.Collection(ops,
            o => Assert.Equal(OpKind.Jz, o.Kind),
            o => Assert.Equal(OpKind.AddByte, o.Kind),
            o => Assert.Equal(OpKind.Jnz, o.Kind));
    }

    [Fact]
    public void Optimize_ClearPass_IsIndependentOfScanPass()
    {
        // Only ClearLoop enabled: clears lower, scans do not.
        Assert.Equal(OpKind.SetByte, Assert.Single(Optimizer.Optimize(Parser.Parse("[-]"), OptPasses.ClearLoop)).Kind);

        var scanKept = Optimizer.Optimize(Parser.Parse("[>]"), OptPasses.ClearLoop);
        Assert.DoesNotContain(scanKept, o => o.Kind == OpKind.ScanPtr);
    }

    [Fact]
    public void Optimize_ScanStride_NeedsCollapse()
    {
        // [>>] only becomes one scan once the moves are collapsed; without the
        // Collapse pass the two-op body isn't a single-op loop.
        Assert.DoesNotContain(
            Optimizer.Optimize(Parser.Parse("[>>]"), OptPasses.ScanLoop),
            o => o.Kind == OpKind.ScanPtr);

        Assert.Contains(
            Optimizer.Optimize(Parser.Parse("[>>]"), OptPasses.Collapse | OptPasses.ScanLoop),
            o => o.Kind == OpKind.ScanPtr && o.Arg == 2);
    }

    [Fact]
    public void Optimize_CollapsesRuns_AndPatchesJumpTargets()
    {
        // "+++" collapses to one op, shifting every later index; the loop's
        // jump targets must be re-patched to stay matched.
        var ops = Optimizer.Optimize(Parser.Parse("+++[>+<-]"));

        Assert.Collection(ops,
            o => { Assert.Equal(OpKind.AddByte, o.Kind); Assert.Equal(3, o.Arg); },
            o => { Assert.Equal(OpKind.Jz, o.Kind); Assert.Equal(7, o.Arg); },   // exit -> past end
            o => { Assert.Equal(OpKind.AddPtr, o.Kind); Assert.Equal(1, o.Arg); },
            o => { Assert.Equal(OpKind.AddByte, o.Kind); Assert.Equal(1, o.Arg); },
            o => { Assert.Equal(OpKind.AddPtr, o.Kind); Assert.Equal(-1, o.Arg); },
            o => { Assert.Equal(OpKind.AddByte, o.Kind); Assert.Equal(-1, o.Arg); },
            o => { Assert.Equal(OpKind.Jnz, o.Kind); Assert.Equal(1, o.Arg); }); // back -> the Jz
    }
}