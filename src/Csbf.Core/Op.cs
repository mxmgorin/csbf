namespace Csbf.Core;

public abstract record Op;

public record IncPtr() : Op;
public record DecPtr() : Op;
public record Inc() : Op;
public record Dec() : Op;
public record Output() : Op;
public record Input() : Op;
public record Loop(IReadOnlyCollection<Op> Body) : Op;
