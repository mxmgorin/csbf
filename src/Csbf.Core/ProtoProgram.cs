namespace Csbf.Core;

public sealed class ProtoProgram(IReadOnlyList<Op> ops, bool usesInput)
{
    public IReadOnlyList<Op> Ops { get; } = ops;
    public bool UsesInput { get; } = usesInput;
}