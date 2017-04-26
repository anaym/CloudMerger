using System;
using System.Text;

namespace CloudMerger.Core.Primitives
{
    public struct FileSize : IComparable<FileSize>, IEquatable<FileSize>
    {
        public static readonly FileSize Empty = new FileSize(0);

        public static FileSize FromGBytes(int gBytes) => new FileSize(gBytes*multipliers[3]);

        public FileSize(long totalBytes)
        {
            bytesCount = totalBytes;
        }

        public long TotalBytes => bytesCount;
        public double TotalKBytes => GetTotalCount(1);
        public double TotalMBytes => GetTotalCount(2);
        public double TotalGBytes => GetTotalCount(3);
        public double TotalTBytes => GetTotalCount(4);

        public short Bytes => GetCount(0);
        public short KBytes => GetCount(1);
        public short MBytes => GetCount(2);
        public short GBytes => GetCount(3);
        public short TBytes => GetCount(4);

        public static bool operator ==(FileSize left, FileSize right) => left.Equals(right);
        public static bool operator !=(FileSize left, FileSize right) => !(left == right);
        public static bool operator <(FileSize left, FileSize right) => left.CompareTo(right) < 0;
        public static bool operator >(FileSize left, FileSize right) => left.CompareTo(right) > 0;
        public static bool operator >=(FileSize left, FileSize right) => left.CompareTo(right) >= 0;
        public static bool operator <=(FileSize left, FileSize right) => left.CompareTo(right) <= 0;

        public static FileSize operator +(FileSize left, FileSize right) => new FileSize(left.TotalBytes + right.TotalBytes);

        public string ToShortString()
        {
            for (int i = multipliers.Length - 1; i >= 0; i--)
            {
                if (GetCount(i) > 0)
                    return $"{GetTotalCount(i):00.000}{suffixes[i]}";
            }
            return "0B";
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            for (int i = multipliers.Length - 1; i >= 0; i--)
                if (GetCount(i) != 0)
                    str.Append($"{GetCount(i)}{suffixes[i]} ");
            if (str.Length == 0)
                return "0 B";
            return str.ToString(0, str.Length - 1);
        }

        public int CompareTo(FileSize other) => bytesCount.CompareTo(other.bytesCount);
        public bool Equals(FileSize other) => bytesCount == other.bytesCount;

        private double GetTotalCount(int degree)
        {
            return 1.0*bytesCount/multipliers[degree];
        }

        private short GetCount(int degree)
        {
            var bottom = multipliers[degree];
            return (short) (bytesCount/bottom%1024);
        }

        private static long[] multipliers = {1, 1024, 1024*1024, 1024*1024*1024, 1024L*1024*1024*1024};
        private static string[] suffixes = {"B", "KB", "MB", "GB", "TB"};

        private readonly long bytesCount;
    }
}