using Csbf.Core;

namespace Csbf.Cli.Cmd;

public class OptCmd : ICmd
{
    public string Name => "opt";
    public string[] Aliases => [];

    // Individually toggleable passes, addressed by short names/aliases.
    private static readonly (string Name, OptPasses Flag, string[] Aliases)[] AllPasses =
    [
        ("collapse", OptPasses.Collapse, ["peephole", "runs"]),
        ("clear", OptPasses.ClearLoop, ["clearloop"]),
        ("scan", OptPasses.ScanLoop, ["scanloop"]),
    ];

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length == 1)
        {
            Print(ctx.Optimizations);
            return;
        }

        switch (args[1].ToLowerInvariant())
        {
            case "all":
            case "on":
                ctx.Optimizations = OptPasses.All;
                Print(ctx.Optimizations);
                return;
            case "none":
            case "off":
                ctx.Optimizations = OptPasses.None;
                Print(ctx.Optimizations);
                return;
        }

        // opt <pass> [on|off] — default action toggles the pass.
        var key = args[1].ToLowerInvariant();
        var pass = AllPasses.FirstOrDefault(p => p.Name == key || p.Aliases.Contains(key));

        if (pass.Name is null)
        {
            Console.WriteLine("usage: opt [all|none | <collapse|clear|scan> [on|off]]");
            return;
        }

        var enable = args.Length < 3
            ? !ctx.Optimizations.HasFlag(pass.Flag)
            : args[2].ToLowerInvariant() is "on" or "true" or "1";

        ctx.Optimizations = enable
            ? ctx.Optimizations | pass.Flag
            : ctx.Optimizations & ~pass.Flag;

        Print(ctx.Optimizations);
    }

    private static void Print(OptPasses passes)
    {
        Console.WriteLine("optimizations:");
        foreach (var (name, flag, _) in AllPasses)
        {
            Console.WriteLine($"  {name,-9} {(passes.HasFlag(flag) ? "on" : "off")}");
        }
    }
}
