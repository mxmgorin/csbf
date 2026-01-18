using Csbf.Core;

namespace Csbf.Cli.Cmd;

public class EmitCmd : ICmd
{
    public string Name => "emit";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("usage: emit <lang> <in> <out>");
            return;
        }

        var lang = args[1];
        if (!ctx.Codegens.TryGetValue(lang, out var cg))
        {
            Console.WriteLine($"unknown backend: {lang}");
            return;
        }

        var src = File.ReadAllText(args[2]);
        var ops = Parser.Parse(src);
        var code = cg.Emit(ops);
        File.WriteAllText(args[3], code);

        Console.WriteLine($"emitted to {lang}: {args[3]}");
    }
}