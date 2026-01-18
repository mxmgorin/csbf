using Csbf.Debugger;

namespace Csbf.Cli.Cmd;

public class RegsCmd : ICmd
{
    public string Name => "regs";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (!ctx.Dbg.Vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        App.PrintInt("IP", ctx.Dbg.Vm.Ip);
        App.PrintInt("DP", ctx.Dbg.Vm.Dp);
        App.PrintInt("CELL", ctx.Dbg.Vm.Current);
    }
}