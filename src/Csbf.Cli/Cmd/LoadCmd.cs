using Csbf.Core;

namespace Csbf.Cli.Cmd;

public class LoadCmd : ICmd
{
    public string Name => "load";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: load <path>");
            return;
        }

        var path = args[1];
        var src = App.ReadFile(path);

        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        ctx.Debugger.Vm.Load(src);
        Console.WriteLine("program loaded");
    }
}