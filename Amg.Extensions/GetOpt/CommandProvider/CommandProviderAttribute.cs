using System;

namespace Amg.GetOpt
{
    /// <summary>
    /// Indicates that the property contains commands and options.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandProviderAttribute : Attribute
    {
    }
}
