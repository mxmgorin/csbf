using System.Text;
using Csbf.Core;

namespace Csbf.Codegen;

public sealed class GoCodegen : ICodegen
{
    private int _indent = 1;

    public string Emit(IEnumerable<Op> ops)
    {
        var sb = new StringBuilder();

        sb.AppendLine("package main");
        sb.AppendLine();
        sb.AppendLine("import (");
        sb.AppendLine("  \"bufio\"");
        sb.AppendLine("  \"fmt\"");
        sb.AppendLine("  \"os\"");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.AppendLine("func main() {");
        sb.AppendLine("  mem := make([]byte, 30000)");
        sb.AppendLine("  dp := 0");
        sb.AppendLine("  r := bufio.NewReader(os.Stdin)");

        foreach (var op in ops)
        {
            EmitOp(sb, op);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private void EmitLine(StringBuilder sb, string s)
    {
        sb.AppendLine(new string(' ', _indent * 2) + s);
    }

    private void EmitOp(StringBuilder sb, Op op)
    {
        switch (op)
        {
            case IncPtr:
                EmitLine(sb, "dp++");
                break;

            case DecPtr:
                EmitLine(sb, "dp--");
                break;

            case Inc:
                EmitLine(sb, "mem[dp]++");
                break;

            case Dec:
                EmitLine(sb, "mem[dp]--");
                break;

            case Output:
                EmitLine(sb, "fmt.Printf(\"%c\", mem[dp])");
                break;

            case Input:
                EmitLine(sb, "b, _ := r.ReadByte()");
                EmitLine(sb, "mem[dp] = b");
                break;

            case Loop l:
                EmitLine(sb, "for mem[dp] != 0 {");
                _indent++;

                foreach (var inner in l.Body)
                {
                    EmitOp(sb, inner);
                }

                _indent--;
                EmitLine(sb, "}");
                break;

            default:
                throw new NotSupportedException(op.GetType().Name);
        }
    }
}