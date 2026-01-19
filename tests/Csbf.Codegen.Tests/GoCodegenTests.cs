using Csbf.Core;

namespace Csbf.Codegen.Tests;

public class GoCodegenTests
{
    [Fact]
    public void EmitOp_EmitsIncPtr()
    {
        var codegen = new GoCodegen();
        codegen.OnOp(new Op(OpKind.IncPtr, 2));
        
        var code = codegen.Emit();
        
        Assert.Equal("package main\n\n\n  dp+=2\n", code);
    }
}