using System;

namespace CloudMerger.Core.Primitives
{
    public class DiskSpaceInfo : IEquatable<DiskSpaceInfo>
    {
        public static readonly DiskSpaceInfo Empty = new DiskSpaceInfo(FileSize.Empty, FileSize.Empty);

        public DiskSpaceInfo(FileSize freeSpace, FileSize usedSpace)
        {
            FreeSpace = freeSpace;
            UsedSpace = usedSpace;
            TotalSpace = freeSpace + usedSpace;
        }

        public readonly FileSize FreeSpace;
        public readonly FileSize UsedSpace;
        public readonly FileSize TotalSpace;

        #region Operators
        public static DiskSpaceInfo operator +(DiskSpaceInfo l, DiskSpaceInfo r)
        {
            return new DiskSpaceInfo(l.FreeSpace + r.FreeSpace, l.UsedSpace + r.UsedSpace);
        }
        #endregion

        public override string ToString() => $"Used {UsedSpace} from {TotalSpace}";

        #region Equality members
        public bool Equals(DiskSpaceInfo other)
        {
            return FreeSpace.Equals(other.FreeSpace) && UsedSpace.Equals(other.UsedSpace) && TotalSpace.Equals(other.TotalSpace);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DiskSpaceInfo)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FreeSpace.GetHashCode();
                hashCode = (hashCode * 397) ^ UsedSpace.GetHashCode();
                hashCode = (hashCode * 397) ^ TotalSpace.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}