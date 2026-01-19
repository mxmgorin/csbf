using Csbf.Core;
using Csbf.Debugger;

namespace Csbf.Cli;

public sealed class App
{
    private readonly IContext _ctx = new AppContext();
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

            if (line is null) break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            _dispatcher.Dispatch(_ctx, parts);
        }
    }

    public static string ReadFile(string path)
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

    public static void PrintInt(string label, int value)
    {
        Console.WriteLine($"{label}: 0x{value:X} ({value})");
    }

    public static byte ReadByte()
    {
        var ch = Console.Read();
        if (ch < 0)
            return 0; // EOF == 0 is conventional in BF

        return (byte)ch;
    }

    public static void WriteByte(byte b)
    {
        Console.Write((char)b);
    }
}