namespace Csbf.Core;

public static class Parser
{
    public sealed class ParseState
    {
        public int Index;
        public int Limit;
    }


    public static IEnumerable<Op> Parse(string src)
    {
        var state = new ParseState { Index = 0, Limit = src.Length };
        return ParseBlock(src, state);
    }

    internal static IEnumerable<Op> ParseBlock(string src, ParseState state)
    {
        while (state.Index < state.Limit)
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
                    yield return new IncByte();
                    break;
                case '-':
                    state.Index++;
                    yield return new DecByte();
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
                {
                    var bodyStart = ++state.Index;
                    var depth = 1;

                    while (state.Index < state.Limit && depth > 0)
                    {
                        switch (src[state.Index])
                        {
                            case '[':
                                depth++;
                                break;
                            case ']':
                                depth--;
                                break;
                        }

                        state.Index++;
                    }

                    if (depth != 0)
                        throw new InvalidOperationException("Unclosed '['");

                    var bodyEnd = state.Index - 1;
                    yield return new Loop(src, bodyStart, bodyEnd);
                    break;
                }

                case ']':
                    throw new InvalidOperationException("Unexpected closing bracket");

                default:
                    state.Index++;
                    break;
            }
        }
    }
}