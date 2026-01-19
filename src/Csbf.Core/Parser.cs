namespace Csbf.Core;

public static class Parser
{

    public static IReadOnlyList<Op> Parse(string src)
    {
        var ops = new List<Op>();
        var loopStack = new Stack<int>();

        foreach (var op in src)
        {
            switch (op)
            {
                case '>': ops.Add(new Op(OpKind.IncPtr, 1)); break;
                case '<': ops.Add(new Op(OpKind.DecPtr, 1)); break;
                case '+': ops.Add(new Op(OpKind.IncByte, 1)); break;
                case '-': ops.Add(new Op(OpKind.DecByte, 1)); break;
                case '.': ops.Add(new Op(OpKind.Out, 0)); break;
                case ',': ops.Add(new Op(OpKind.In, 0)); break;

                case '[':
                {
                    // Emit Jz with dummy target
                    var jz = new Op(OpKind.Jz, -1);
                    ops.Add(jz);
                    loopStack.Push(ops.Count - 1);
                    break;
                }

                case ']':
                {
                    if (loopStack.Count == 0)
                    {
                        throw new InvalidOperationException("Unexpected closing bracket");
                    }

                    var openIndex = loopStack.Pop();

                    // Emit Jnz back to the instruction after '['
                    var jnz = new Op(OpKind.Jnz, openIndex);
                    ops.Add(jnz);

                    // Patch the earlier Jz to jump here (after Jnz)
                    ops[openIndex] = new Op(OpKind.Jz, ops.Count);
                    break;
                }
            }
        }

        return loopStack.Count != 0 ? throw new InvalidOperationException("Unclosed '['") : ops;
    }
}