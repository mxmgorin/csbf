using Csbf.Codegen;
using Csbf.Core;

namespace Csbf.Cli;

public interface IContext
{
    Debugger.Debugger Dbg { get; }
    Dictionary<string, ICodegen> Codegens { get; }
}

public class AppContext : IContext
{
    public Debugger.Debugger Dbg { get; } = new(new Vm(App.ReadByte, App.WriteByte));

    public Dictionary<string, ICodegen> Codegens { get; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["go"] = new GoCodegen(),
        };
}