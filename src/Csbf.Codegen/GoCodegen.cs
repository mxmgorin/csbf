using System.Text;
using Csbf.Core;

namespace Csbf.Codegen;

public sealed class GoCodegen : ICodegen
{
    private readonly StringBuilder _body = new();
    private int _indent = 1;
    private AnalysisResult _analysis = new(false, false);


    public void OnBegin()
    {
        _body.AppendLine("func main() {");
        _body.AppendLine("  mem := make([]byte, 30000)");
        _body.AppendLine("  dp := 0");
    }

    public void OnEnd(AnalysisResult analysis)
    {
        _analysis = analysis;
        _body.AppendLine("}");
    }

    public void OnOp(Op op)
    {
        EmitOp(op);
    }

    private void EmitLine(string op, string arg = "")
    {
        _body.AppendLine(new string(' ', _indent * 2) + op + arg);
    }

    private void EmitOp(Op op)
    {
        switch (op.Kind)
        {
            case OpKind.IncPtr:
                EmitLine("dp += ", op.Arg.ToString());
                break;

            case OpKind.DecPtr:
                EmitLine("dp -= ", op.Arg.ToString());
                break;

            case OpKind.IncByte:
                EmitLine("mem[dp] += ", op.Arg.ToString());
                break;

            case OpKind.DecByte:
                EmitLine("mem[dp] -= ", op.Arg.ToString());
                break;

            case OpKind.Out:
                EmitLine("fmt.Printf(\"%c\", mem[dp])");
                break;

            case OpKind.In:
                EmitLine("b, _ := r.ReadByte()");
                EmitLine("mem[dp] = b");
                break;

            case OpKind.Jz:
                EmitLine("for mem[dp] != 0 {");
                _indent++;
                break;

            case OpKind.Jnz:
                _indent--;
                EmitLine("}");
                break;

            default:
                throw new NotSupportedException(op.GetType().Name);
        }
    }

    public string Emit()
    {
        var sb = new StringBuilder();

        EmitHeader(sb);
        sb.AppendLine();

        if (_analysis.UsesInput)
        {
            sb.AppendLine("var r = bufio.NewReader(os.Stdin)");
        }

        sb.Append(_body);
        return sb.ToString();
    }

    private void EmitHeader(StringBuilder sb)
    {
        sb.AppendLine("package main");
        sb.AppendLine();

        if (_analysis.UsesOutput || _analysis.UsesInput)
        {
            sb.AppendLine("import (");

            if (_analysis.UsesInput)
            {
                sb.AppendLine("  \"bufio\"");
                sb.AppendLine("  \"os\"");
            }

            if (_analysis.UsesOutput)
            {
                sb.AppendLine("  \"fmt\"");
            }

            sb.AppendLine(")");
        }
    }
}