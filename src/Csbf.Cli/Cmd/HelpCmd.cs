namespace Csbf.Cli.Cmd;

public class HelpCmd : ICmd
{
    public string Name => "help";
    public string[] Aliases => [];

    public void Execute(IContext _, string[] args)
    {
        Console.WriteLine("commands:");
        Console.WriteLine();
        Console.WriteLine("  load <file>              load Brainfuck source file");
        Console.WriteLine("  step                     execute one instruction");
        Console.WriteLine("  run [file]               run until breakpoint or end");
        Console.WriteLine("  break <ip>               set breakpoint at instruction pointer");
        Console.WriteLine("  regs                     show registers (IP, DP, current cell)");
        Console.WriteLine("  mem <from> <len>         dump memory range");
        Console.WriteLine("  eval <src>               execute inline Brainfuck source");
        Console.WriteLine("  emit <lang> <in> [out]   compile <in> to target language <lang>");
        Console.WriteLine("  help                     show this help");
        Console.WriteLine("  exit | quit              exit cli");
        Console.WriteLine();
    }
}