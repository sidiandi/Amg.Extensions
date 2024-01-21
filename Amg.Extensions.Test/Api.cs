using Amg.Test;
using System.Collections.Generic;
using System.Text.Json;
using R = System.Reflection;

namespace Amg
{

    namespace Api
    {

        public record PropertyInfo(
            string Name,
            TypeRef PropertyType,
            ParameterInfo[] IndexParameters);

        public record TypeRef(string FullName, string Assembly);

        public record Type(
            TypeRef Name,
            ConstructorInfo[] Constructors,
            MethodInfo[] Methods,
            PropertyInfo[] Properties
            );

        public record Assembly(Type[] Types);

        public record ConstructorInfo(ParameterInfo[] Parameters);

        public record MethodInfo(
            string Name,
            TypeRef ReturnType,
            ParameterInfo[] Parameters);

        public record ParameterInfo(string Name, TypeRef Type);

        public record ApiDiff(
            Type[] Added,
            Type[] Removed
            );

        public static class Api
        {
            public static ParameterInfo Get(R.ParameterInfo p)
                => new ParameterInfo(p.Name!, Ref(p.ParameterType));

            public static ParameterInfo[] Get(R.ParameterInfo[] parameters)
                => parameters.Select(Get).ToArray();

            public static ConstructorInfo Get(R.ConstructorInfo c) => new ConstructorInfo(Get(c.GetParameters()));

            public static MethodInfo Get(R.MethodInfo m)
                => new MethodInfo(m.Name!, Ref(m.ReturnType), Get(m.GetParameters()));

            public static PropertyInfo Get(R.PropertyInfo p)
                => new PropertyInfo(p.Name, Ref(p.PropertyType), Get(p.GetIndexParameters()));

            public static Type Get(System.Type t) => new Type(
                Ref(t),
                t.GetConstructors().Select(Get).ToArray(),
                t.GetMethods().Select(Get).ToArray(),
                t.GetProperties().Select(Get).ToArray());


            public static Assembly Get(R.Assembly a)
            {
                return new Assembly(a.ExportedTypes.Select(_ => Get(_)).ToArray());
            }

            public static TypeRef Ref(System.Type t)
                => new TypeRef(t.FullName!, t.Assembly.FullName!);

            static Type[] Added(Type a, Type b)
            {
                var d = new Type(
                    b.Name,
                    b.Constructors.Except(a.Constructors).ToArray(),
                    b.Methods.Except(a.Methods).ToArray(),
                    b.Properties.Except(a.Properties).ToArray());

                return (d.Constructors.Length == 0 && d.Methods.Length == 0 && d.Properties.Length == 0)
                    ? new Type[] { }
                    : new[] { d };
            }

            public static ApiDiff Diff(Type a, Type b)
            {
                return new ApiDiff(
                    Added(a, b),
#pragma warning disable S2234 // Parameters should be passed in the correct order
                    Added(b, a));
#pragma warning restore S2234 // Parameters should be passed in the correct order
            }

            public static ApiDiff Diff(Assembly a, Assembly b)
            {
                var added = b.Types.ExceptBy(a.Types.Select(_ => _.Name), _ => _.Name);
                var removed = a.Types.ExceptBy(b.Types.Select(_ => _.Name), _ => _.Name);
                var addedOrRemovedTypes = new ApiDiff(added.ToArray(), removed.ToArray());

                var changedTypes = b.Types.Join(a.Types, _ => _.Name, _ => _.Name,
                    (_b, _a) => Amg.Api.Api.Diff(_a, _b));

                return Merge(new[] { addedOrRemovedTypes }.Concat(changedTypes));
            }

            public static ApiDiff Merge(IEnumerable<ApiDiff> d)
            {
                return new ApiDiff(d.SelectMany(_ => _.Added).ToArray(),
                    d.SelectMany(_ => _.Removed).ToArray());
            }
        }
    }

    [TestFixture]
    public class ApiTest
    {
        [Test]
        public void SaveApi()
        {
            var assembly = R.Assembly.GetAssembly(typeof(TestHelpers))!;
            var api = Amg.Api.Api.Get(assembly);
            Assert.That(api, Is.EqualTo(api));
            var diff = Api.Api.Diff(api, api);
            Assert.That(diff.Added, Is.Empty);
            Assert.That(diff.Removed, Is.Empty);

            var t = api.Types[0];
            t = t with
            {
                Methods = t.Methods.Append(new Amg.Api.MethodInfo("Hello",
                Amg.Api.Api.Ref(typeof(string)), new Amg.Api.ParameterInfo[] { })).ToArray()
            };

            var api2 = new Amg.Api.Assembly(new[] { t }.Concat(api.Types.Skip(1)).ToArray());

            var d = Api.Api.Diff(api, api2);
            d.Added.Should().HaveCount(1);
            d.Removed.Should().HaveCount(0);
            _ = System.Text.Json.JsonSerializer.Serialize(d, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}