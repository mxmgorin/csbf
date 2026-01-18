using System.Reflection;
using Csbf.Cli.Cmd;

namespace Csbf.Cli;

public class Dispatcher
{
    private readonly Dictionary<string, ICmd> _cmds;

    public Dispatcher()
    {
        _cmds = new Dictionary<string, ICmd>(StringComparer.OrdinalIgnoreCase);
        var cmdTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(ICmd).IsAssignableFrom(t));

        foreach (var type in cmdTypes)
        {
            var cmd = (ICmd)Activator.CreateInstance(type)!;
            _cmds[cmd.Name] = cmd;

            foreach (var alias in cmd.Aliases)
            {
                _cmds[alias] = cmd;
            }
        }
    }

    public void Dispatch(IContext ctx, string[] args)
    {
        if (args.Length == 0)
            return;

        if (_cmds.TryGetValue(args[0], out var cmd))
        {
            cmd.Execute(ctx, args);
        }
        else
        {
            Console.WriteLine("unknown command");
        }
    }
}