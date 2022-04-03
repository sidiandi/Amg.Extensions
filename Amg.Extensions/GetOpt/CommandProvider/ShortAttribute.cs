using System;
using System.Reflection;

namespace Amg.GetOpt
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ShortAttribute : Attribute
    {
        public string ShortName { get; }

        public ShortAttribute(char shortName)
        {
            this.ShortName = new string(shortName, 1);
        }

        public static string? Get(PropertyInfo p)
        {
            var a = p.GetCustomAttribute<ShortAttribute>();
            return a == null ? null : a.ShortName;
        }
    }
}