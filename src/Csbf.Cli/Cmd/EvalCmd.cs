namespace Csbf.Cli.Cmd;

public class EvalCmd : ICmd
{
    public string Name => "eval";
    public string[] Aliases => [];
    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: eval <src>");
            return;
        }
        
        var src = args[1];
        ctx.Debugger.Vm.Load(src);
        ctx.Debugger.Debug();
    }
}