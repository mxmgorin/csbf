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
    public void EmitOp_EmitsSetByte_AsAssignment()
    {
        var codegen = new GoCodegen();
        codegen.OnBegin();
        codegen.OnOp(new Op(OpKind.SetByte, 0));
        codegen.OnEnd(new AnalysisResult(false, false));

        Assert.Contains("mem[dp] = 0", codegen.Emit());
    }

    [Fact]
    public void EmitOp_EmitsScanPtr_AsSkipLoop()
    {
        var codegen = new GoCodegen();
        codegen.OnBegin();
        codegen.OnOp(new Op(OpKind.ScanPtr, 1));
        codegen.OnEnd(new AnalysisResult(false, false));

        var code = codegen.Emit();
        Assert.Contains("for mem[dp] != 0 {", code);
        Assert.Contains("dp += 1", code);
    }

    [Fact]
    public void Pipeline_LowersClearLoop_ToReadableAssignment()
    {
        var gen = new GoCodegen();
        new Pipeline(gen).Run(Parser.Parse("+[-]"));

        var code = gen.Emit();
        Assert.Contains("mem[dp] += 1", code);
        Assert.Contains("mem[dp] = 0", code);            // clear lowered to an assignment
        Assert.DoesNotContain("for mem[dp] != 0", code); // ...not a loop
    }

    [Fact]
    public void Pipeline_WithPassesDisabled_EmitsRawClearLoop()
    {
        // Disabling the passes falls back to the 1:1 loop transliteration.
        var gen = new GoCodegen();
        new Pipeline(gen).Run(Parser.Parse("+[-]"), OptPasses.None);

        var code = gen.Emit();
        Assert.Contains("for mem[dp] != 0 {", code);
        Assert.DoesNotContain("mem[dp] = 0", code);
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