namespace Csbf.Cli.Cmd;

public interface ICmd
{
    string Name { get; }
    string[] Aliases { get; }
    void Execute(IContext ctx, string[] args);
}