using System;
using System.Linq;

namespace Amg.Extensions
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Writes instance properties
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static IWritable Destructure(this object x) => TextFormatExtensions.GetWritable(_ => _.Dump(x));

        /// <summary>
        /// object properties as table
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static IWritable PropertiesTable(this object x)
        {
            return x.GetType()
                .GetProperties()
                .Select(p => new { p.Name, Value = p.GetValue(x, new object[] { }) })
                .ToTable(header: false);
        }

        /// <summary>
        /// like ToString, but never throws. x can also be null.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string SafeToString(this object? x)
        {
            if (x is null)
            {
                return String.Empty;
            }

            try
            {
                return x.ToString() ?? String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }
    }
}
