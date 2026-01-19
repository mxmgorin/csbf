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
        var text = App.ReadFile(path);

        if (string.IsNullOrEmpty(text)) return;

        var ir = Parser.Parse(text);
        var vmOps = Lowering.Lower(ir);
        ctx.Debugger.Vm.Load(vmOps);
        Console.WriteLine("program loaded");
    }
}