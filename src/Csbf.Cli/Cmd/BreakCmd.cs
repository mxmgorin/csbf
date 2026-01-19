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

        Console.WriteLine(ctx.Debugger.AddBreakpoint(ip)
            ? $"breakpoint set at 0x{ip:X} ({ip})"
            : $"breakpoint already exists at 0x{ip:X} ({ip})");
    }
}