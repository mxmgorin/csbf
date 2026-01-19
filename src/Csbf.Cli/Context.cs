using Csbf.Codegen;
using Csbf.Core;

namespace Csbf.Cli;

public interface IContext
{
    Debugger.Debugger Debugger { get; }
}

public class AppContext : IContext
{
    public Debugger.Debugger Debugger { get; } = new(new Vm(App.ReadByte, App.WriteByte));
}