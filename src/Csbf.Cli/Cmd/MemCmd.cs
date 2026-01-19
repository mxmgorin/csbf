namespace Csbf.Cli.Cmd;

public class MemCmd : ICmd
{
    public string Name => "mem";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (!ctx.Debugger.Vm.HasProgram())
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
            span = ctx.Debugger.Vm.Read(from, len);
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
}