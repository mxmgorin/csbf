using Csbf.Core;

namespace Csbf.Codegen;

public interface ICodegen
{
    void OnOp(Op op);
    void OnBegin();
    void OnEnd(ProgramAnalysis analysis);
    string Flush();
}