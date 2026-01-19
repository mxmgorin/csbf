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
            var src = App.ReadFile(path);

            if (!string.IsNullOrEmpty(src))
            {
                ctx.Debugger.Vm.Load(src);
                Console.WriteLine("program loaded");
            }
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
                Console.WriteLine("\nprogram finished");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}