namespace Csbf.Core;

public enum OpKind
{
    /// <summary>
    /// Add a signed delta to the data pointer (<c>&gt;</c> = +1, <c>&lt;</c> = -1).
    /// </summary>
    AddPtr,

    /// <summary>
    /// Add a signed delta to the byte at the data pointer (<c>+</c> = +1, <c>-</c> = -1).
    /// </summary>
    AddByte,

    /// <summary>
    /// Output the byte at the pointer.
    /// </summary>
    Out,

    /// <summary>
    /// Input a byte and store it in the byte at the pointer.
    /// </summary>
    In,

    /// <summary>
    /// Jump if zero
    /// </summary>
    Jz,

    /// <summary>
    /// Jump if non-zero
    /// </summary>
    Jnz,

    /// <summary>
    /// Set the byte at the pointer to a constant (<see cref="Op.Arg"/>).
    /// Lowered from clear loops <c>[-]</c> / <c>[+]</c> (which is <c>SetByte 0</c>).
    /// </summary>
    SetByte,

    /// <summary>
    /// Scan the tape from the pointer by a fixed signed stride (<see cref="Op.Arg"/>)
    /// until a zero cell (memchr-style). Lowered from scan loops <c>[&gt;]</c> / <c>[&lt;]</c>.
    /// </summary>
    ScanPtr
}

public readonly record struct Op(OpKind Kind, int Arg)
{
    public override string ToString()
        => Kind switch
        {
            OpKind.AddPtr => $"ADD_PTR {Arg}",
            OpKind.AddByte => $"ADD_BYTE {Arg}",
            OpKind.Out => "OUT",
            OpKind.In => "IN",
            OpKind.Jz => $"JZ  0x{Arg:X}",
            OpKind.Jnz => $"JNZ 0x{Arg:X}",
            OpKind.SetByte => $"SET {Arg}",
            OpKind.ScanPtr => $"SCAN {Arg}",
            _ => Kind.ToString()
        };
}
