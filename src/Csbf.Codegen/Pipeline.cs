using Csbf.Core;

namespace Csbf.Codegen;

public sealed class Pipeline(params ICodegen[] gens)
{
    private readonly Analyser _analyser = new();

    public void Run(IEnumerable<Op> ops, OptPasses passes = OptPasses.All)
    {
        foreach (var s in gens)
        {
            s.OnBegin();
        }

        Walk(Optimizer.Optimize(ops, passes));

        foreach (var s in gens)
        {
            s.OnEnd(_analyser.GetResult());
        }
    }

    private void Walk(IEnumerable<Op> ops)
    {
        foreach (var op in ops)
        {
            _analyser.Process(op);

            foreach (var s in gens)
            {
                s.OnOp(op);
            }
        }
    }
}