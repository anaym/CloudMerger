using System.IO;

namespace CloudMerger.Core.Utility
{
    public static class DirectoryHelper
    {
        public static DirectoryInfo GetSubDirectory(this DirectoryInfo directory, string name)
        {
            return new DirectoryInfo(Path.Combine(directory.FullName, name));
        }
        public static FileInfo GetSubFile(this DirectoryInfo directory, string name)
        {
            return new FileInfo(Path.Combine(directory.FullName, name));
        }
    }
}