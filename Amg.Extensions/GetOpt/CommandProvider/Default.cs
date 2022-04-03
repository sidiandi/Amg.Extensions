using System;

namespace Amg.GetOpt
{
    /// <summary>
    /// Marks a default command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DefaultAttribute : Attribute
    {
    }
}