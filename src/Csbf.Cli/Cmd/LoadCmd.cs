using Csbf.Core;

namespace Csbf.Cli.Cmd;

public class LoadCmd : ICmd
{
    public string Name => "load";
    public string[] Aliases => [];

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: load <path>");
            return;
        }

        var path = args[1];
        Load(ctx, path);
    }

    public static void Load(IContext ctx, string path)
    {
        var src = ReadFile(path);

        if (string.IsNullOrEmpty(src))
        {
            Console.WriteLine($"File not found: {path}");
            return;
        }

        ctx.Debugger.Vm.Load(src);
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

        return string.Empty;
    }
}