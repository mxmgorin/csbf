namespace Csbf.Core;

public enum VmOpKind
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

public readonly record struct VmOp(VmOpKind Kind, int Arg)
{
    public override string ToString()
        => Kind switch
        {
            VmOpKind.IncPtr => "INC_PTR",
            VmOpKind.DecPtr => "DEC_PTR",
            VmOpKind.IncByte => "INC_BYTE",
            VmOpKind.DecByte => "DEC_BYTE",
            VmOpKind.Out => "OUT",
            VmOpKind.In => "IN",
            VmOpKind.Jz => $"JZ  0x{Arg:X}",
            VmOpKind.Jnz => $"JNZ 0x{Arg:X}",
            _ => Kind.ToString()
        };
}