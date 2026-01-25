namespace Csbf.Cli.Cmd;

public class BreakCmd : ICmd
{
    public string Name => "break";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length < 2 || !int.TryParse(args[1], out var ip))
        {
            Console.WriteLine("usage: break <ip>");
            return;
        }

        if (ctx.Debugger.AddBreakpoint(ip))
        {
            Console.WriteLine($"breakpoint set at 0x{ip:X}");
        }
        else
        {
            ctx.Debugger.RemoveBreakpoint(ip);
            Console.WriteLine($"breakpoint removed from 0x{ip:X}");
        }
    }
}