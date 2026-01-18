using Csbf.Core;

namespace Csbf.Codegen;

public sealed class Pipeline(params ICodegen[] gens)
{
    private readonly ProgramAnalysis _analysis = new();

    public void Run(IEnumerable<Op> ops)
    {
        foreach (var s in gens)
        {
            s.OnBegin();
        }

        Walk(ops);

        foreach (var s in gens)
        {
            s.OnEnd(_analysis);
        }
    }

    private void Walk(IEnumerable<Op> ops)
    {
        foreach (var op in ops)
        {
            switch (op)
            {
                case Input:
                    _analysis.UsesInput = true;
                    break;
                case Output:
                    _analysis.UsesOutput = true;
                    break;
            }

            foreach (var s in gens)
            {
                s.OnOp(op);
            }
        }
    }
}