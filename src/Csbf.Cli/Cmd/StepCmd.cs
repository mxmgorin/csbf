namespace Csbf.Cli.Cmd;

public class StepCmd : ICmd
{
    public string Name => "step";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (!ctx.Debugger.Vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        var op = ctx.Debugger.Vm.Peek();

        if (op is null)
        {
            Console.WriteLine("program finished");
            return;
        }

        var ip = ctx.Debugger.Vm.Ip;
        ctx.Debugger.Vm.Step();
        Console.WriteLine($"0x{ip:X}: {op}");
    }
}