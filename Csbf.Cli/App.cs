using Csbf.Core;

namespace Csbf.Cli;

public sealed class App
{
    private readonly Vm _vm = new();

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

    private void CmdStep()
    {
        _vm.Step();
        Console.WriteLine("Executed 1 step");
    }

    private void CmdLoad(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: load <path>");
            return;
        }
        
        var path = args[1];
        var text = ReadFile(path);
        var ir = Parser.Parse(text);
        var vmOps = Lowering.Lower(ir);
        _vm.Load(vmOps);
    }
    
    private void CmdRegs()
    {
        if (!_vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        PrintInt("IP", _vm.Ip);
        PrintInt("DP", _vm.Dp);
        PrintInt("CELL", _vm.Current);
    }

    private void Dispatch(string[] args)
    {
        var cmd = args[0];

        switch (cmd)
        {
            case "help":
                CmdHelp();
                break;

            case "step":
                CmdStep();
                break;
            
            case "load":
                CmdLoad(args);
                break;
            
            case "regs":
                CmdRegs();
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

    private static string ReadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found");
            return string.Empty;
        }
    }

    private static void PrintInt(string label, int value)
    {
        Console.WriteLine($"{label}: 0x{value:X} ({value})");
    }

    private static void CmdHelp()
    {
        Console.WriteLine("commands:");
        Console.WriteLine();
        Console.WriteLine("  load <file>        load Brainfuck source");
        Console.WriteLine("  step               execute one instruction");
        // Console.WriteLine("  run                run until breakpoint or halt");
        // Console.WriteLine("  break <ip>         set breakpoint at instruction index");
        Console.WriteLine("  regs               show registers (IP, DP, current cell)");
        // Console.WriteLine("  mem <from> <len>   dump memory range");
        Console.WriteLine("  help               show this help");
        Console.WriteLine("  exit | quit        exit cli");
        Console.WriteLine();
    }
}