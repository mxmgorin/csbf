using Xunit.Abstractions;

namespace Csbf.Core.Tests;

public class ParserTests
{
    [Fact]
    public void Parse_ReturnsEmpty_ForEmptySource()
    {
        var ops = Parser.Parse(string.Empty);

        Assert.Empty(ops);
    }

    [Fact]
    public void Parse_ParsesAllOps()
    {
        var ops = Parser.Parse("+-<>.,");

        Assert.Collection(ops,
            op => Assert.Equal(OpKind.IncByte, op.Kind),
            op => Assert.Equal(OpKind.DecByte, op.Kind),
            op => Assert.Equal(OpKind.DecPtr, op.Kind),
            op => Assert.Equal(OpKind.IncPtr, op.Kind),
            op => Assert.Equal(OpKind.Out, op.Kind),
            op => Assert.Equal(OpKind.In, op.Kind)
        );
    }

    [Fact]
    public void Parse_ParsesLoop()
    {
        var ops = Parser.Parse("[+]");

        Assert.Collection(ops,
            op => Assert.Equal(OpKind.Jz, op.Kind),
            op => Assert.Equal(OpKind.IncByte, op.Kind),
            op => Assert.Equal(OpKind.Jnz, op.Kind)
        );
    }

    [Fact]
    public void Parse_ParsesNestedLoops()
    {
        var ops = Parser.Parse("[+[-]]");

        Assert.Collection(ops,
            op => Assert.Equal(OpKind.Jz, op.Kind),
            op => Assert.Equal(OpKind.IncByte, op.Kind),
            op => Assert.Equal(OpKind.Jz, op.Kind),
            op => Assert.Equal(OpKind.DecByte, op.Kind),
            op => Assert.Equal(OpKind.Jnz, op.Kind),
            op => Assert.Equal(OpKind.Jnz, op.Kind)
        );
    }


    [Fact]
    public void Parse_ThrowsOnUnexpectedClosingBracket()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Parser.Parse("+]").ToArray());
    }
}