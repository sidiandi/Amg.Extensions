using System.Threading.Tasks;

namespace Amg.OnceImpl;

partial class InvocationInfo
{
    class TaskResultHandler<Result> : IReturnValueSource
    {
        private readonly InvocationInfo invocationInfo;
        private readonly Task<Result?> task;

#pragma warning disable S1144 // Unused private types or members should be removed
        public TaskResultHandler(InvocationInfo invocationInfo, Task<Result?> task)
#pragma warning restore S1144 // Unused private types or members should be removed
        {
            this.invocationInfo = invocationInfo;
            this.task = task;
        }

        async Task<Result?> GetReturnValue()
        {
            var result = GetReturnValue2();
            await Task.WhenAny(result, this.invocationInfo.interceptor!.waitUntilCancelled);
            return await result;
        }

        async Task<Result?> GetReturnValue2()
        {
            try
            {
                var r = await task;
                r = (Result?)invocationInfo.InterceptReturnValue(r);
                invocationInfo.Complete();
                return r;
            }
            catch (Exception ex)
            {
                throw invocationInfo.Fail(ex);
            }
        }

        public object ReturnValue => GetReturnValue();
    }
}
