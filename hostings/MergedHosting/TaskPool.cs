using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudMerger.Core.Utility;

namespace MergedHosting
{
    public class TaskPool<T>
    {
        public TaskPool(params Task<T>[] tasks)
        {
            worked = tasks.ToList();

            completed = new List<Task<T>>();
            faulted = new List<Task<T>>();
            finished = new List<Task<T>>();
        }

        public void Add(Task<T> task)
        {
            worked.Add(task);
        }

        public bool AllFinished => worked.Count == 0;

        public IReadOnlyList<Task<T>> Completed => completed;
        public IReadOnlyList<Task<T>> Faulted => faulted;
        public IReadOnlyList<Task<T>> Finished => finished;

        public async Task<Task<T>> WhenAny()
        {
            if (worked.Count == 0)
                throw new InvalidOperationException("All task is done");
            var task = await Task.WhenAny(worked);
            finished.Add(task);
            (task.IsFaulted ? faulted : completed).Add(task);
            worked.Remove(task);
            return task;
        }

        public async Task WhenAll()
        {
            while (!AllFinished)
                await WhenAny();
        }

        private readonly List<Task<T>> completed;
        private readonly List<Task<T>> faulted;
        private readonly List<Task<T>> finished;
        private readonly List<Task<T>> worked;
    }

    public static class TaskPool
    {
        public static TaskPool<T> ToTaskWithSource<T, TS>(this IEnumerable<TS> source, Func<TS, Task<T>> taskCreator, TimeSpan timeout, out Dictionary<Task<T>, TS> sources)
        {
            sources = new Dictionary<Task<T>, TS>();
            var pool = new TaskPool<T>();
            foreach (var s in source)
            {
                var task = taskCreator(s).WithTimeOut(timeout);
                sources.Add(task, s);
                pool.Add(task);
            }
            return pool;
        }

        public static TaskPool<T> ToTask<T, TS>(this IEnumerable<TS> source, Func<TS, Task<T>> taskCreator, TimeSpan? timeout = null)
        {
            if (timeout != null)
                return new TaskPool<T>(source.Select(taskCreator).Select(t => t.WithTimeOut(timeout.Value)).ToArray());
            return new TaskPool<T>(source.Select(taskCreator).ToArray());
        }

        public static TaskPool<None> ToTask<TS>(this IEnumerable<TS> source, Func<TS, Task> taskCreator, TimeSpan? timeout = null)
        {
            if (timeout != null)
                return new TaskPool<None>(source.Select(taskCreator).Select(t => t.WithTimeOut(timeout.Value).ReplaceResult<None>()).ToArray());
            return new TaskPool<None>(source.Select(taskCreator).Select(t => t.ReplaceResult<None>()).ToArray());
        }

        public class None
        { }
    }
}