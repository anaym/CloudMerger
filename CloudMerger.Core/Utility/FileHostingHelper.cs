using System.Threading.Tasks;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Core.Utility
{
    public static class FileHostingHelper
    {
        public static async Task<ItemType> GetItemType(this IHosting hosting, UPath path)
        {
            try
            {
                var info = await hosting.GetItemInfoAsync(path);
                return info.Type;
            }
            catch (ItemNotFound)
            {
                return ItemType.Unexist;
            }
        }

        public static async Task<bool> IsDirectoryAsync(this IHosting hosting, UPath path)
        {
            return (await hosting.GetItemInfoAsync(path)).IsDirectory;
        }

        public static async Task<bool> IsFileAsync(this IHosting hosting, UPath path)
        {
            return (await hosting.GetItemInfoAsync(path)).IsFile;
        }

        public static async Task<bool> IsDirectoryOrUnexistAsync(this IHosting hosting, UPath path)
        {
            try
            {
                return (await hosting.GetItemInfoAsync(path)).IsDirectory;
            }
            catch (ItemNotFound)
            {
                return true;
            }
        }

        public static async Task<bool> IsFileOrUnexistAsync(this IHosting hosting, UPath path)
        {
            try
            {
                return (await hosting.GetItemInfoAsync(path)).IsFile;
            }
            catch (ItemNotFound)
            {
                return true;
            }
        }
    }
}