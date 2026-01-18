namespace Csbf.Core;

public static class Parser
{
    private sealed class ParseState
    {
        public int Index;
        public int Depth;
    }

    public static IEnumerable<Op> Parse(string src)
    {
        var state = new ParseState();
        return ParseBlock(src, state);
    }

    private static IEnumerable<Op> ParseBlock(string src, ParseState state)
    {
        while (state.Index < src.Length)
        {
            switch (src[state.Index])
            {
                case '>':
                    state.Index++;
                    yield return new IncPtr();
                    break;

                case '<':
                    state.Index++;
                    yield return new DecPtr();
                    break;

                case '+':
                    state.Index++;
                    yield return new Inc();
                    break;

                case '-':
                    state.Index++;
                    yield return new Dec();
                    break;

                case '.':
                    state.Index++;
                    yield return new Output();
                    break;

                case ',':
                    state.Index++;
                    yield return new Input();
                    break;

                case '[':
                    state.Index++; // consume '['
                    state.Depth++;
                    var body = ParseBlock(src, state).ToArray();
                    yield return new Loop(body);
                    break;

                case ']':
                    state.Index++; // consume ']'
                    if (state.Depth == 0)
                        throw new InvalidOperationException("Unexpected closing bracket");

                    state.Depth--;
                    yield break;

                default:
                    state.Index++;
                    break;
            }
        }

        if (state.Depth > 0)
            throw new InvalidOperationException("Unclosed '['");
    }
}