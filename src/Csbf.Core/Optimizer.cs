namespace Csbf.Core;

public static class Optimizer
{
    public static IReadOnlyList<Op> Optimize(IEnumerable<Op> src)
    {
        var ops = src as IReadOnlyList<Op> ?? src.ToArray();
        var result = new List<Op>(ops.Count);

        var i = 0;
        while (i < ops.Count)
        {
            var op = ops[i];

            if (op.Kind is OpKind.IncPtr or OpKind.DecPtr
                or OpKind.IncByte or OpKind.DecByte)
            {
                var accum = op.Arg;
                var j = i + 1;

                while (j < ops.Count && ops[j].Kind == op.Kind)
                {
                    accum += ops[j].Arg;
                    j++;
                }

                if (accum != 0)
                {
                    result.Add(new Op(op.Kind, accum));
                }

                i = j;
                continue;
            }

            result.Add(op);
            i++;
        }

        return result;
    }
}