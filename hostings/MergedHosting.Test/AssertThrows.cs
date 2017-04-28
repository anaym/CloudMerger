using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace MergedHosting.Test
{
    public static class AssertThrows
    {
        public static void On<T>(Task task) where T : Exception
        {
            try
            {
                task.Wait();
            }
            catch (Exception)
            { }
            if (task.Exception == null)
                Assert.Fail($"Expected exception {typeof(T)}");
            task.Exception.InnerExceptions.First().GetType()
                .ShouldBeEquivalentTo(typeof(T), because: task.Exception.InnerExceptions.First().Message);
        }

        public static void ShouldThrows<T>(this Task t) where T : Exception
        {
            On<T>(t);
        }
    }
}