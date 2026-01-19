namespace Csbf.Core;

public enum OpKind
{
    /// <summary>
    /// Increment the pointer.
    /// </summary>
    IncPtr,

    /// <summary>
    /// Decrement the pointer.
    /// </summary>
    DecPtr,

    /// <summary>
    /// Increment the byte at the pointer.
    /// </summary>
    IncByte,

    /// <summary>
    /// Decrement the byte at the pointer.
    /// </summary>
    DecByte,

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
    Jnz
}

public readonly record struct Op(OpKind Kind, int Arg)
{
    public override string ToString()
        => Kind switch
        {
            OpKind.IncPtr => "INC_PTR",
            OpKind.DecPtr => "DEC_PTR",
            OpKind.IncByte => "INC_BYTE",
            OpKind.DecByte => "DEC_BYTE",
            OpKind.Out => "OUT",
            OpKind.In => "IN",
            OpKind.Jz => $"JZ  0x{Arg:X}",
            OpKind.Jnz => $"JNZ 0x{Arg:X}",
            _ => Kind.ToString()
        };
}