using Csbf.Debugger;

namespace Csbf.Cli.Cmd;

public class RunCmd : ICmd
{
    public string Name => "run";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        var res = ctx.Dbg.Debug();

        switch (res)
        {
            case DebugResult.HitBreakpoint:
                Console.WriteLine($"hit breakpoint at 0x{ctx.Dbg.Vm.Ip:X}");
                break;
            case DebugResult.Finished:
                Console.WriteLine("\nprogram finished");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}