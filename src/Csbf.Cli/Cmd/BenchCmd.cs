using System.Diagnostics;
using Csbf.Core;

namespace Csbf.Cli.Cmd;

public class BenchCmd : ICmd
{
    public string Name => "bench";
    public string[] Aliases => [];

    // Safety valve: a program that never halts (e.g. one that spins waiting on
    // input, now fed EOF) would otherwise hang the REPL.
    private const long StepCap = 2_000_000_000;

    // Op count, executed steps, and time isolate each pass's contribution.
    private static readonly (string Label, OptPasses Passes)[] Levels =
    [
        ("none", OptPasses.None),
        ("collapse", OptPasses.Collapse),
        ("+clear", OptPasses.Collapse | OptPasses.ClearLoop),
        ("+scan", OptPasses.Collapse | OptPasses.ScanLoop),
        ("all", OptPasses.All),
    ];

    // Discards output and returns EOF (0) for input, so timing reflects the VM
    // rather than console I/O and input-driven samples still run to completion.
    private sealed class NullIo : IVmIo
    {
        public byte Read() => 0;
        public void Write(byte value) { }
    }

    public void Execute(IContext ctx, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: bench <file> [runs]");
            return;
        }

        var path = args[1];
        if (!File.Exists(path))
        {
            Console.WriteLine($"File not found: {path}");
            return;
        }

        var runs = args.Length >= 3 && int.TryParse(args[2], out var r) && r > 0 ? r : 3;
        var parsed = Parser.Parse(File.ReadAllText(path));

        Console.WriteLine($"bench: {path}  (best of {runs})");
        Console.WriteLine();
        Console.WriteLine($"  {"level",-9} {"ops",8} {"steps",16} {"time(ms)",10}  speedup");

        double? baseline = null;

        foreach (var (label, passes) in Levels)
        {
            var ops = Optimizer.Optimize(parsed, passes);
            var (steps, ms, capped) = Measure(ops, runs);
            baseline ??= ms;

            var speedup = ms > 0 ? $"{baseline.Value / ms,5:0.00}x" : "  n/a";
            var note = capped ? "  (capped)" : "";
            Console.WriteLine($"  {label,-9} {ops.Count,8} {steps,16:N0} {ms,10:0.00}  {speedup}{note}");
        }
    }

    private static (long steps, double ms, bool capped) Measure(IReadOnlyList<Op> ops, int runs)
    {
        long steps = 0;
        var capped = false;
        var best = double.MaxValue;

        for (var run = 0; run < runs; run++)
        {
            var vm = new Vm(new NullIo());
            vm.Load(ops);

            long n = 0;
            var sw = Stopwatch.StartNew();

            while (!vm.Halted())
            {
                vm.Step();

                if (++n >= StepCap)
                {
                    capped = true;
                    break;
                }
            }

            sw.Stop();
            best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
            steps = n;
        }

        return (steps, best, capped);
    }
}
