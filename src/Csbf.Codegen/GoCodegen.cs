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

    private void EmitLine(string s)
    {
        _body.AppendLine(new string(' ', _indent * 2) + s);
    }

    private void EmitOp(Op op)
    {
        switch (op)
        {
            case IncPtr:
                EmitLine("dp++");
                break;

            case DecPtr:
                EmitLine("dp--");
                break;

            case IncByte:
                EmitLine("mem[dp]++");
                break;

            case DecByte:
                EmitLine("mem[dp]--");
                break;

            case Output:
                EmitLine("fmt.Printf(\"%c\", mem[dp])");
                break;

            case Input:
                EmitLine("b, _ := r.ReadByte()");
                EmitLine("mem[dp] = b");
                break;

            case Loop l:
                EmitLine("for mem[dp] != 0 {");
                _indent++;

                foreach (var inner in l.Body)
                    OnOp(inner);

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