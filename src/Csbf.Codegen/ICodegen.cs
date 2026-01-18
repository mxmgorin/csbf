using Csbf.Core;

namespace Csbf.Codegen;

public interface ICodegen
{
    string Emit(IReadOnlyList<VmOp> ops);
}