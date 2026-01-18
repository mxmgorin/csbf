namespace Csbf.Core;

public abstract record Op;

public record IncPtr() : Op;
public record DecPtr() : Op;
public record IncByte() : Op;
public record DecByte() : Op;
public record Output() : Op;
public record Input() : Op;
public record Loop : Op
{
    private readonly string _src;
    private readonly int _start;
    private readonly int _end;

    public Loop(string src, int start, int end)
    {
        _src = src;
        _start = start;
        _end = end;
    }

    public IEnumerable<Op> Body
    {
        get
        {
            var state = new Parser.ParseState { Index = _start, Limit = _end };
            return Parser.ParseBlock(_src, state);
        }
    }
}
