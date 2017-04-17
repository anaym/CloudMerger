using System.Collections.Generic;
using System.IO;

namespace CloudMerger.Core.Utility
{
    public static class StreamHelper
    {
        public static IEnumerable<string> ReadLines(this StreamReader reader)
        {
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}