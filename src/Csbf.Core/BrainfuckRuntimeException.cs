namespace Csbf.Core;

/// <summary>
/// Thrown when a Brainfuck program performs an illegal operation at runtime,
/// such as moving the data pointer outside the bounds of the tape.
/// </summary>
public sealed class BrainfuckRuntimeException(string message) : Exception(message);
