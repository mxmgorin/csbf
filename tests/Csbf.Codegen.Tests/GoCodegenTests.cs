using Csbf.Core;

namespace Csbf.Codegen.Tests;

public class GoCodegenTests
{
    [Fact]
    public void EmitOp_EmitsAddPtr()
    {
        var codegen = new GoCodegen();
        codegen.OnBegin();
        codegen.OnOp(new Op(OpKind.AddPtr, 2));
        codegen.OnEnd(new AnalysisResult(false, false));

        var code = codegen.Emit();

        Assert.Equal("package main\n\n\nfunc main() {\n  mem := make([]byte, 30000)\n  dp := 0\n  dp += 2\n}\n", code);
    }

    [Fact]
    public void EmitOp_EmitsNegativeAddByte_AsSubtraction()
    {
        // A negative constant would overflow Go's byte, so it must be a subtraction.
        var codegen = new GoCodegen();
        codegen.OnBegin();
        codegen.OnOp(new Op(OpKind.AddByte, -3));
        codegen.OnEnd(new AnalysisResult(false, false));

        var code = codegen.Emit();

        Assert.Equal("package main\n\n\nfunc main() {\n  mem := make([]byte, 30000)\n  dp := 0\n  mem[dp] -= 3\n}\n", code);
    }

    [Fact]
    public void EmitOp_EmitsRepeatedInput_WithoutRedeclaringVariable()
    {
        var codegen = new GoCodegen();
        codegen.OnBegin();
        codegen.OnOp(new Op(OpKind.In, 0));
        codegen.OnOp(new Op(OpKind.In, 0));
        codegen.OnEnd(new AnalysisResult(false, true));

        var code = codegen.Emit();

        // Both reads assign (=), never redeclare (:=) — ':=' twice in one scope
        // fails to compile with "no new variables on left side of :=".
        Assert.Contains("mem[dp], _ = r.ReadByte()", code);
        Assert.DoesNotContain(":= r.ReadByte()", code);
    }
}