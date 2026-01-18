using Csbf.Core;
using Csbf.Debugger;

namespace Csbf.Cli;

public sealed class App
{
    private readonly Debugger.Debugger _dbg = new(new Vm(ReadByte, WriteByte));

    public void Dispatch(string[] args)
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

            case "run":
                CmdRun();
                break;

            case "load":
                CmdLoad(args);
                break;

            case "regs":
                CmdRegs();
                break;
            
            case "mem":
                CmdMem(args);
                break;
            
            case "break":
                CmdBreak(args);
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
        if (!_dbg.Vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        var op = _dbg.Vm.Peek();

        if (op is null)
        {
            Console.WriteLine("program finished");
            return;
        }

        var ip = _dbg.Vm.Ip;
        _dbg.Vm.Step();
        Console.WriteLine($"0x{ip:X}: {op}");
    }
    
    private void CmdBreak(string[] args)
    {
        if (args.Length < 2 || !int.TryParse(args[1], out var ip))
        {
            Console.WriteLine("usage: break <ip>");
            return;
        }

        Console.WriteLine(_dbg.AddBreakpoint(ip)
            ? $"breakpoint set at 0x{ip:X} ({ip})"
            : $"breakpoint already exists at 0x{ip:X} ({ip})");
    }


    private void CmdRun()
    {
        var res = _dbg.Debug();

        switch (res)
        {
            case DebugResult.HitBreakpoint:
                Console.WriteLine($"hit breakpoint at 0x{_dbg.Vm.Ip:X}");
                break;
            case DebugResult.Finished:
                Console.WriteLine("\nprogram finished");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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

        if (string.IsNullOrEmpty(text)) return;

        var ir = Parser.Parse(text);
        var vmOps = Lowering.Lower(ir);
        _dbg.Vm.Load(vmOps);
        Console.WriteLine("program loaded");
    }

    private void CmdRegs()
    {
        if (!_dbg.Vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        PrintInt("IP", _dbg.Vm.Ip);
        PrintInt("DP", _dbg.Vm.Dp);
        PrintInt("CELL", _dbg.Vm.Current);
    }

    private void CmdMem(string[] args)
    {
        if (!_dbg.Vm.HasProgram())
        {
            Console.WriteLine("no program loaded");
            return;
        }

        if (args.Length < 3 ||
            !uint.TryParse(args[1], out var from) ||
            !uint.TryParse(args[2], out var len))
        {
            Console.WriteLine("usage: mem <from> <len>");
            return;
        }

        ReadOnlySpan<byte> span;
        try
        {
            span = _dbg.Vm.Read(from, len);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("out of range");
            return;
        }

        for (var i = 0; i < span.Length; i++)
        {
            if (i % 16 == 0)
                Console.Write($"\n{from + i:X5}: ");

            Console.Write($"{span[i]:X2} ");
        }

        Console.WriteLine();
    }


    private static string ReadFile(string path)
    {
        // Try relative to current working directory
        var relative = Path.GetFullPath(path);

        if (File.Exists(relative))
        {
            return File.ReadAllText(relative);
        }

        // If the user already provided an absolute path, or the relative lookup failed,
        // try the path as-is
        if (Path.IsPathRooted(path) && File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        Console.WriteLine($"File not found: {path}");
        return string.Empty;
    }

    private static void PrintInt(string label, int value)
    {
        Console.WriteLine($"{label}: 0x{value:X} ({value})");
    }

    private static byte ReadByte()
    {
        var ch = Console.Read();
        if (ch < 0)
            return 0; // EOF == 0 is conventional in BF

        return (byte)ch;
    }

    private static void WriteByte(byte b)
    {
        Console.Write((char)b);
    }

    private static void CmdHelp()
    {
        Console.WriteLine("commands:");
        Console.WriteLine();
        Console.WriteLine("  load <file>        load Brainfuck source");
        Console.WriteLine("  step               execute one instruction");
        Console.WriteLine("  run                run until breakpoint or halt");
        Console.WriteLine("  break <ip>         set breakpoint at instruction pointer");
        Console.WriteLine("  regs               show registers (IP, DP, current cell)");
        Console.WriteLine("  mem <from> <len>   dump memory range");
        Console.WriteLine("  help               show this help");
        Console.WriteLine("  exit | quit        exit cli");
        Console.WriteLine();
    }
}