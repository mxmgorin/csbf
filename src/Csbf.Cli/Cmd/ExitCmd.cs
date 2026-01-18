namespace Csbf.Cli.Cmd;

public class ExitCmd : ICmd
{
    public string Name => "Exit";
    public string[] Aliases => ["quit"];

    public void Execute(IContext ctx, string[] args)
    {
        Environment.Exit(0);
    }
}