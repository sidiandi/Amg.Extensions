namespace Amg.OnceImpl.Example;
#pragma warning disable CS0414
public class AClassThatHasMutableFields
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2933:Fields that are only assigned in the constructor should be \"readonly\"", Justification = "<Pending>")]
    int i = 0;

    public virtual void Hello()
    {
        Console.WriteLine("hello");
    }
}
