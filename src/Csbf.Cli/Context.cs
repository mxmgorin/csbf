using Csbf.Core;

namespace Csbf.Cli;

public interface IContext
{
    Debugger.Debugger Debugger { get; }

    /// <summary>Optimizer passes applied when loading a program; each is toggleable via the <c>opt</c> command.</summary>
    OptPasses Optimizations { get; set; }
}

public class AppContext(Vm vm) : IContext
{
    public Debugger.Debugger Debugger { get; } = new(vm);

    public OptPasses Optimizations { get; set; } = OptPasses.All;
}
