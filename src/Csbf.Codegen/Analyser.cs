using Csbf.Core;

namespace Csbf.Codegen;

public class AnalysisResult(bool usesOutput, bool usesInput)
{
    public bool UsesInput { get; } = usesInput;
    public bool UsesOutput { get; } = usesOutput;
}

public class Analyser
{
    private bool _usesInput;
    private bool _usesOutput;

    public void Process(Op op)
    {
        switch (op.Kind)
        {
            case OpKind.In:
                _usesInput = true;
                break;
            case OpKind.Out:
                _usesOutput = true;
                break;
        }
    }

    public AnalysisResult GetResult()
    {
        return new AnalysisResult(_usesOutput, _usesInput);
    }
}