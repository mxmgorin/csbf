namespace Csbf.Core;

public static class Lowering
{
    public static VmOp[] Lower(IEnumerable<Op> ops)
    {
        var outOps = new List<VmOp>();
        Emit(ops, outOps);
        return outOps.ToArray();
    }

    private static void Emit(IEnumerable<Op> ops, List<VmOp> outOps)
    {
        foreach (var op in ops)
        {
            switch (op)
            {
                case IncPtr:
                    outOps.Add(new VmOp(VmOpKind.IncPtr, 0));
                    break;

                case DecPtr:
                    outOps.Add(new VmOp(VmOpKind.DecPtr, 0));
                    break;

                case Inc:
                    outOps.Add(new VmOp(VmOpKind.IncByte, 0));
                    break;

                case Dec:
                    outOps.Add(new VmOp(VmOpKind.DecByte, 0));
                    break;

                case Output:
                    outOps.Add(new VmOp(VmOpKind.Out, 0));
                    break;

                case Input:
                    outOps.Add(new VmOp(VmOpKind.In, 0));
                    break;

                case Loop l:
                {
                    // Jz -> jump to end of loop
                    var jzPos = outOps.Count;
                    outOps.Add(new VmOp(VmOpKind.Jz, 0)); // patched later

                    Emit(l.Body, outOps);

                    // Jnz -> jump back to start
                    var jnzPos = outOps.Count;
                    outOps.Add(new VmOp(VmOpKind.Jnz, jzPos));

                    // patch Jz to point after Jnz
                    outOps[jzPos] = new VmOp(VmOpKind.Jz, jnzPos);
                    break;
                }
            }
        }
    }
}