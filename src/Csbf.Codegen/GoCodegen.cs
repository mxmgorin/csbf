using System.Text;
using Csbf.Core;

namespace Csbf.Codegen;

public class GoCodegen : ICodegen
{
    public string Emit(IReadOnlyList<VmOp> ops)
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

        const int indent = 1;

        foreach (var op in ops)
        {
            switch (op.Kind)
            {
                case VmOpKind.IncPtr:
                    Line($"dp += {op.Arg}");
                    break;

                case VmOpKind.DecPtr:
                    Line($"dp -= {op.Arg}");
                    break;

                case VmOpKind.IncByte:
                    Line($"mem[dp] += {op.Arg}");
                    break;

                case VmOpKind.DecByte:
                    Line($"mem[dp] -= {op.Arg}");
                    break;

                case VmOpKind.Out:
                    Line("fmt.Printf(\"%c\", mem[dp])");
                    break;

                case VmOpKind.In:
                    Line("b, _ := r.ReadByte()");
                    Line("mem[dp] = b");
                    break;

                case VmOpKind.Jz:
                    Line($"if mem[dp] == 0 {{ ip = {op.Arg}; continue }}");
                    break;

                case VmOpKind.Jnz:
                    Line($"if mem[dp] != 0 {{ ip = {op.Arg}; continue }}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        sb.AppendLine("}");
        return sb.ToString();

        void Line(string s)
            => sb.AppendLine(new string(' ', indent * 2) + s);
    }
}