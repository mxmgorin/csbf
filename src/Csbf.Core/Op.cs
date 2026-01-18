namespace Csbf.Core;

public abstract record Op;

public record IncPtr() : Op;
public record DecPtr() : Op;
public record IncByte() : Op;
public record DecByte() : Op;
public record Output() : Op;
public record Input() : Op;
public record Loop(IReadOnlyCollection<Op> Body) : Op;
