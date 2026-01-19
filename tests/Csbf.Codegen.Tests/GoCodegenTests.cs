using Csbf.Core;
using Xunit.Abstractions;

namespace Csbf.Codegen.Tests;

public class GoCodegenTests
{
    [Fact]
    public void EmitOp_EmitsIncPtr()
    {
        var codegen = new GoCodegen();
        codegen.OnBegin();
        codegen.OnOp(new Op(OpKind.IncPtr, 2));
        codegen.OnEnd(new AnalysisResult(false, false));
        
        var code = codegen.Emit();

        Assert.Equal("package main\n\n\nfunc main() {\n  mem := make([]byte, 30000)\n  dp := 0\n  dp += 2\n}\n", code);
    }
}