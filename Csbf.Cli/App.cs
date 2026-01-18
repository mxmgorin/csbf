using Csbf.Core;

namespace Csbf.Cli;

public sealed class App
{
    private Vm _vm = new();

    public void Run()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null) break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            Dispatch(parts);
        }
    }

    private static void Dispatch(string[] args)
    {
        var cmd = args[0];

        switch (cmd)
        {
            case "help":
                CmdHelp();
                break;

            case "exit":
            case "quit":
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("unknown command");
                break;
        }
    }

    private static void CmdHelp()
    {
        Console.WriteLine("commands:");
        Console.WriteLine();
        // Console.WriteLine("  load <file>        load Brainfuck source");
        // Console.WriteLine("  step               execute one instruction");
        // Console.WriteLine("  run                run until breakpoint or halt");
        // Console.WriteLine("  break <ip>         set breakpoint at instruction index");
        // Console.WriteLine("  regs               show registers (IP, DP, current cell)");
        // Console.WriteLine("  mem <from> <len>   dump memory range");
        Console.WriteLine("  help               show this help");
        Console.WriteLine("  exit | quit        exit cli");
        Console.WriteLine();
    }
}