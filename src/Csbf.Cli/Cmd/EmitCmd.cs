using Csbf.Codegen;
using Csbf.Core;

namespace Csbf.Cli.Cmd;

public class EmitCmd : ICmd
{
    public string Name => "emit";
    public string[] Aliases => [];

    private readonly Dictionary<string, ICodegen> _codegenByLang =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["go"] = new GoCodegen(),
        };


    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("usage: emit <lang> <in> <out>");
            return;
        }

        var lang = args[1];
        if (!_codegenByLang.TryGetValue(lang, out var codegen))
        {
            Console.WriteLine($"unknown backend: {lang}");
            return;
        }

        var src = File.ReadAllText(args[2]);
        var ops = Parser.Parse(src);
        new Pipeline(codegen).Run(ops);
        var code = codegen.Emit();
        File.WriteAllText(args[3], code);

        Console.WriteLine($"emitted file: {args[3]}");
    }
}