using Csbf.Core;

namespace Csbf.Debugger;

public enum DebugResult
{
    HitBreakpoint,
    Finished
}

public sealed class Debugger(Vm vm)
{
    private readonly HashSet<int> _breakpoints = [];
    public Vm Vm { get; private set; } = vm;

    public bool AddBreakpoint(int ip) => _breakpoints.Add(ip);
    public bool RemoveBreakpoint(int ip) => _breakpoints.Remove(ip);

    public DebugResult Debug()
    {
        while (!Vm.ProgramFinished())
        {
            if (_breakpoints.Contains(Vm.Ip)) return DebugResult.HitBreakpoint;

            Vm.Step();
        }

        return DebugResult.Finished;
    }
}
