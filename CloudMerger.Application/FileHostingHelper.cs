using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Application
{
    public static class FileHostingHelper
    {
        public static async Task<ItemType> GetItemType(this IFileHosting hosting, UPath path)
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

        public static async Task<bool> IsDirectoryAsync(this IFileHosting hosting, UPath path)
        {
            return (await hosting.GetItemInfoAsync(path)).IsDirectory;
        }

        public static async Task<bool> IsFileAsync(this IFileHosting hosting, UPath path)
        {
            return (await hosting.GetItemInfoAsync(path)).IsFile;
        }

        public static async Task<bool> IsDirectoryOrUnexistAsync(this IFileHosting hosting, UPath path)
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

        public static async Task<bool> IsFileOrUnexistAsync(this IFileHosting hosting, UPath path)
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