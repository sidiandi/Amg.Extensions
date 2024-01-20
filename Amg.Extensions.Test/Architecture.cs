using System;
using System.Linq;
using System.Reflection;
using Amg.Extensions;

namespace Amg;

[TestFixture]
public class Architecture
{
    [Test]
    public void Api()
    {
        var assembly = typeof(Amg.GetOpt.GetOpt).Assembly;
        Assert.Pass(PublicApi(assembly).ToString());
    }

    IWritable PublicApi(Assembly a) => TextFormatExtensions.GetWritable(w =>
    {
        w.WriteLine(a.GetName().Name);
        foreach (var t in a.GetTypes()
            .Where(_ => _.IsPublic)
            .OrderBy(_ => _.FullName)
            )
        {
            w.Write(PublicApi(t));
        }
    });

    IWritable PublicApi(Type t) => TextFormatExtensions.GetWritable(w =>
    {
        if (!t.IsPublic) return;
        w.WriteLine(t.FullName);
    });
}
