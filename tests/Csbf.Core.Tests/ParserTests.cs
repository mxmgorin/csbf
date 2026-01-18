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
            op => Assert.IsType<IncByte>(op),
            op => Assert.IsType<DecByte>(op),
            op => Assert.IsType<DecPtr>(op),
            op => Assert.IsType<IncPtr>(op),
            op => Assert.IsType<Output>(op),
            op => Assert.IsType<Input>(op)
        );
    }
    
    [Fact]
    public void Parse_ParsesLoop()
    {
        var ops = Parser.Parse("[+-]");

        var loop = Assert.Single(ops) as Loop;
        Assert.NotNull(loop);

        Assert.Collection(loop.Body,
            op => Assert.IsType<IncByte>(op),
            op => Assert.IsType<DecByte>(op)
        );
    }
    
    [Fact]
    public void Parse_ParsesNestedLoops()
    {
        var ops = Parser.Parse("[+[--]]");

        var outer = Assert.Single(ops) as Loop;
        Assert.NotNull(outer);

        Assert.Collection(outer.Body,
            op => Assert.IsType<IncByte>(op),
            op =>
            {
                var inner = Assert.IsType<Loop>(op);
                Assert.Collection(inner.Body,
                    x => Assert.IsType<DecByte>(x),
                    x => Assert.IsType<DecByte>(x)
                );
            }
        );
    }
    
    
    [Fact]
    public void Parse_ThrowsOnUnexpectedClosingBracket()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Parser.Parse("+]").ToArray());
    }
}