using Amg.FileSystem;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amg.OnceImpl;

internal class CachedInvocationInfo : IInvocation
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

    public CachedInvocationInfo(Interceptor interceptor, InvocationId id, Castle.DynamicProxy.IInvocation invocation)
    {
        Id = id;

        var fileName = interceptor.CacheDir.Combine(id.Uid + ".json");

        if (fileName.IsFile())
        {
            Logger.Information("{task} uses cached result from {fileName}", this, fileName);
            next = null;
            begin = DateTime.UtcNow;
            try
            {
                using (var r = File.OpenRead(fileName))
                {
                    var taskResultType = InvocationInfo.TryGetTaskResultType(invocation.Method.ReturnType);
                    if (taskResultType is { })
                    {
                        var taskResult = JsonSerializer.Deserialize(r, taskResultType);
                        returnValue = TaskExtensions.FromResult(taskResultType, taskResult);
                    }
                    else
                    {
                        returnValue = JsonSerializer.Deserialize(r, invocation.Method.ReturnType);
                    }
                }
                end = DateTime.UtcNow;
                return;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Cached return value in {fileName} could not be read. Cache will be reset.", fileName);
                fileName.EnsureFileNotExists();
            }
        }

        next = new InvocationInfo(interceptor, id, invocation);
        if (next.ReturnValue is { })
        {
            if (next.ReturnValue is Task task)
            {
                task.ContinueWith(_ =>
                {
                    if (_.TryGetResult(out var resultType, out var result))
                    {
                        using var writer = File.OpenWrite(fileName.EnsureParentDirectoryExists());
                        System.Text.Json.JsonSerializer.Serialize(writer, result!);
                        Logger.Information("{task} stored cached result at {fileName}", this, fileName);
                    }
                });
            }
            else
            {
                using (var writer = File.OpenWrite(fileName.EnsureParentDirectoryExists()))
                {
                    System.Text.Json.JsonSerializer.Serialize(writer, next.ReturnValue);
                }
            }
        }
    }

    readonly object? returnValue = null;
    readonly DateTime? begin = null;
    readonly DateTime? end = null;
    readonly IInvocation? next;

    public InvocationId Id { get; }

    public InvocationState State { get; private set; }

    public DateTime? Begin => next is { } ? next.Begin : begin;
    public DateTime? End => next is { } ? next.End : end;

    public object? ReturnValue => next is { } ? next.ReturnValue : returnValue;

    public Exception? Exception => next?.Exception;

    public override string? ToString() => Id.ToString();
}
