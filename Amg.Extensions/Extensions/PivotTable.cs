namespace Amg.Extensions;

public record PivotTable<D, A0, A1>
{
    internal PivotTable(A0[] axis0, A1[] axis1, D[,] data)
    {
        Axis0 = axis0;
        Axis1 = axis1;
        Data = data;
    }

    public A0[] Axis0 { get; private set; }
    public A1[] Axis1 { get; private set; }
    public D[,] Data { get; private set; }

    public IEnumerable<D> GetRow(int axis1)
    {
        foreach (var j in Enumerable.Range(0, Axis0.Length))
        {
            yield return Data[j, axis1];
        }
    }
}
