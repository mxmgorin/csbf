namespace Csbf.Cli.Cmd;

public class HelpCmd : ICmd
{
    public string Name => "help";
    public string[] Aliases => [];

    public void Execute(IContext _, string[] args)
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