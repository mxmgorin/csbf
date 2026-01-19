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
        switch (op)
        {
            case Input:
                _usesInput = true;
                break;
            case Output:
                _usesOutput = true;
                break;
        }
    }

    public AnalysisResult GetResult()
    {
        return new AnalysisResult(_usesOutput, _usesInput);
    }
}