namespace Csbf.Cli.Cmd;

public class BackCmd : ICmd
{
    public string Name => "back";
    public string[] Aliases => ["stepb"];

    public void Execute(IContext ctx, string[] args)
    {
        var vm = ctx.Debugger.Vm;

        if (!vm.Loaded())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        var n = 1;

        if (args.Length >= 2 && (!int.TryParse(args[1], out n) || n < 1))
        {
            Console.WriteLine("usage: back [n]   (n >= 1)");
            return;
        }

        var reversed = 0;

        while (reversed < n && vm.StepBack())
        {
            reversed++;
        }

        if (reversed == 0)
        {
            Console.WriteLine("nothing to step back to");
            return;
        }

        Console.WriteLine($"stepped back {reversed} instruction(s) -> 0x{vm.Ip:X}");

        if (reversed < n)
        {
            Console.WriteLine("reached start of recorded history");
        }
    }
}
