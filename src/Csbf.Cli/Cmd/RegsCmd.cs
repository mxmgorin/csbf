using Csbf.Debugger;

namespace Csbf.Cli.Cmd;

public class RegsCmd : ICmd
{
    public string Name => "regs";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (!ctx.Debugger.Vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        App.PrintInt("IP", ctx.Debugger.Vm.Ip);
        App.PrintInt("DP", ctx.Debugger.Vm.Dp);
        App.PrintInt("CELL", ctx.Debugger.Vm.Current);
    }
}