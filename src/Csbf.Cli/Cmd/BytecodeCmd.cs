namespace Csbf.Cli.Cmd;

public class BytecodeCmd : ICmd
{
    public string Name => "bytecode";
    public string[] Aliases => ["bc"];

    public void Execute(IContext ctx, string[] args)
    {
        for (var i = 0; i < ctx.Debugger.Vm.Ops.Length; i++)
        {
            Console.WriteLine($"0x{i + 1:X}: {ctx.Debugger.Vm.Ops[i]}");
        }
    }
}