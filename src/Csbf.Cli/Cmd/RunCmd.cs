using Csbf.Debugger;

namespace Csbf.Cli.Cmd;

public class RunCmd : ICmd
{
    public string Name => "run";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length >= 2)
        {
            var path = args[1];
            LoadCmd.Load(ctx, path);
        }

        if (!ctx.Debugger.Vm.Loaded())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        var res = ctx.Debugger.Debug();

        switch (res)
        {
            case DebugResult.HitBreakpoint:
                Console.WriteLine($"hit breakpoint at 0x{ctx.Debugger.Vm.Ip:X}");
                break;
            case DebugResult.Finished:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}