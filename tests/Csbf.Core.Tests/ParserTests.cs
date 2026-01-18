using Xunit.Abstractions;

namespace Csbf.Core.Tests;

public class ParserTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ParserTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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
            op => Assert.IsType<Inc>(op),
            op => Assert.IsType<Dec>(op),
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
            op => Assert.IsType<Inc>(op),
            op => Assert.IsType<Dec>(op)
        );
    }
    
    [Fact]
    public void Parse_ParsesNestedLoops()
    {
        var ops = Parser.Parse("[+[--]]");

        var outer = Assert.Single(ops) as Loop;
        Assert.NotNull(outer);

        Assert.Collection(outer.Body,
            op => Assert.IsType<Inc>(op),
            op =>
            {
                var inner = Assert.IsType<Loop>(op);
                Assert.Collection(inner.Body,
                    x => Assert.IsType<Dec>(x),
                    x => Assert.IsType<Dec>(x)
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