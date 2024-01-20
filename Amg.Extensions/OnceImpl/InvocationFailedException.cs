using Amg.Extensions;
using System;
using System.Runtime.Serialization;

namespace Amg.OnceImpl;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>")]
internal class InvocationFailedException : Exception
{
    public IInvocation Invocation { get; }

    public InvocationFailedException(IInvocation invocationInfo)
       : base($"{invocationInfo} failed.", invocationInfo.Exception)
    {
        Invocation = invocationInfo;
    }

    public static IWritable ShortMessage(Exception ex) => TextFormatExtensions.GetWritable(w =>
    {
        if (ex is InvocationFailedException i)
        {
            w.Write(i.Message);
        }
        else
        {
            w.Write(ex);
        }
    });
}
