using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloudMerger.Core.Utility
{
    public static class TaskHelper
    {
        public static async Task<T> WithTimeOut<T>(this Task<T> task, TimeSpan timeout, bool returnDefault = false)
        {
            var timeoutTask = Task.Delay(timeout);
            var completed = await Task.WhenAny(timeoutTask, task);
            if (completed == timeoutTask)
            {
                if (returnDefault)
                    return default(T);
                throw new TimeoutException();
            }
            return await task;
        }

        public static async Task WithTimeOut(this Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var completed = await Task.WhenAny(timeoutTask, task);
            if (completed == timeoutTask)
            {
                throw new TimeoutException();
            }
            await task;
        }

        public static bool IsFaultedWith<T>(this Task task) where T : Exception
        {
            if (task.IsFaulted)
                return false;
            return task.Exception.InnerExceptions.First().GetType() == typeof(T);
        }

        public static async Task<T> ReplaceResult<T>(this Task task, T value = default(T))
        {
            await task;
            return value;
        }
    }
}