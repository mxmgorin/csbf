using Csbf.Debugger;

namespace Csbf.Cli.Cmd;

public class RegsCmd : ICmd
{
    public string Name => "regs";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (!ctx.Debugger.Vm.Loaded())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        ctx.Debugger.PrintRegs();
    }
}