using System.Reflection;
using System.Runtime.CompilerServices;
using Amg.Extensions;
using Castle.DynamicProxy;

namespace Amg.OnceImpl;

internal class Hook : IProxyGenerationHook
{
    public void MethodsInspected()
    {
        foreach (var type in types)
        {
            AssertNoMutableFields(type);
            AssertNoMutableProperties(type);
        }
    }

    readonly HashSet<Type> types = new HashSet<Type>();

    static void AssertNoMutableFields(Type type)
    {
        var fields = type.GetFields(
            BindingFlags.Instance |
            BindingFlags.Public |
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            BindingFlags.NonPublic);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

        var mutableFields = fields.Where(
            f => !f.IsInitOnly &&
            !f.GetCustomAttributes<CompilerGeneratedAttribute>().Any());

        if (mutableFields.Any())
        {
            throw new OnceException($@"All fields of {type} must be readonly. 
Following fields are not readonly:
{mutableFields.Select(_ => _.Name).Join()}");
        }
    }

    static void AssertNoMutableProperties(Type type)
    {
        var properties = type.GetProperties(
            BindingFlags.Instance |
            BindingFlags.Public |
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            BindingFlags.NonPublic);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

        var unsuitableProperties = properties.Where(f => f.CanWrite && (f.GetMethod is { }  && !f.GetMethod.IsVirtual));

        if (unsuitableProperties.Any())
        {
            throw new OnceException($@"All properties of {type} must be readonly OR virtual
Following properties do not fulfill the condition:
{unsuitableProperties.Select(_ => _.Name).Join()}");
        }
    }

    public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
    {
        throw new OnceException($"{memberInfo} must be virtual.");
    }

    public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
    {
        var t = methodInfo.DeclaringType;
        if (t is { })
        {
            types.Add(t);
        }
        return true;
    }
}
