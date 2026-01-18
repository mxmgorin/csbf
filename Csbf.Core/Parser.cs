namespace Csbf.Core;

public static class Parser
{
    public static IReadOnlyList<Op> Parse(string src)
    {
        var i = 0;
        var ops = ParseBlock(src, ref i);

        return i < src.Length ? throw new InvalidOperationException("unexpected ']'") : ops;
    }

    private static List<Op> ParseBlock(string src, ref int i)
    {
        var ops = new List<Op>();

        while (i < src.Length)
        {
            switch (src[i])
            {
                case '>': ops.Add(new IncPtr()); break;
                case '<': ops.Add(new DecPtr()); break;
                case '+': ops.Add(new Inc()); break;
                case '-': ops.Add(new Dec()); break;
                case '.': ops.Add(new Output()); break;
                case ',': ops.Add(new Input()); break;

                case '[':
                    i++; // consume '['
                    var body = ParseBlock(src, ref i);
                    ops.Add(new Loop(body));
                    continue;

                case ']':
                    // end of this block
                    return ops;

                // everything else is a comment
            }

            i++;
        }

        return ops;
    }
}