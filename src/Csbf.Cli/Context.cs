using Csbf.Core;

namespace Csbf.Cli;

public interface IContext
{
    Debugger.Debugger Debugger { get; }
}

public class AppContext(Vm vm) : IContext
{
    public Debugger.Debugger Debugger { get; } = new(vm);
}