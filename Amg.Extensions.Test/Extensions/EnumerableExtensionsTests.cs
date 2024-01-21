using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amg.Extensions;

[TestFixture]
public class EnumerableExtensionsTests
{
    [Test]
    public void NotNullValueType()
    {
        int?[] e = [5, 4, null, 2, 1];
        e.NotNull().Should().BeEquivalentTo([5,4,2,1]);
    }

    [Test]
    public void NotNullReferenceType()
    {
        string?[] e = ["a", null, "b"];
        e.NotNull().Should().BeEquivalentTo("a", "b");
    }

    [Test]
    public void Order()
    {
        int[] e = [5, 4, 3, 2, 1];
        int[] order = [1, 2, 3];
        e.Order(order).Should().BeEquivalentTo([1, 2, 3, 5, 4]);
    }

    [Test]
    public void FirstOf()
    {
        int[] e = [5, 4, 3, 2, 1];
        int[] candidates = [1, 2, 3];
        e.FirstOf(candidates).Should().BeEquivalentTo([1]);
    }
}
