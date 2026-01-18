using Csbf.Core;

namespace Csbf.Codegen;

public interface ICodegen
{
    string Emit(IEnumerable<Op> ops);
}