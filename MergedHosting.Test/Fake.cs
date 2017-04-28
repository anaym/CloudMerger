using System;
using System.Threading.Tasks;

namespace MergedHosting.Test
{
    public static class Fake<T>
    {
        public static async Task<T> Throws<TE>(TE exception) where TE : Exception
        {
            throw exception;
        }

        public static Task<T> Throws<TE>()
            where TE : Exception, new ()
        {
            return Throws<TE>(new TE());
        }
    }
}