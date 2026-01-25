using Csbf.Core;

namespace Csbf.Cli;

public sealed class App
{
    private readonly IContext _ctx = new AppContext(new Vm(new ConsoleIo()));
    private readonly Dispatcher _dispatcher = new();

    public void ExecuteCmd(string[] args)
    {
        _dispatcher.Dispatch(_ctx, args);
    }

    public void RunRepl()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line is null)
            {
                break;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            _dispatcher.Dispatch(_ctx, parts);
        }
    }
}