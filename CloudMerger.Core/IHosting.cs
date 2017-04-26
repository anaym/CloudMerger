using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Core
{
    public interface IHosting : IDisposable
    {
        string Name { get; }

        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        Task<DiskSpaceInfo> GetSpaceInfoAsync();

        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task<bool> IsExistAsync(UPath path);
        /// <exception cref="ItemNotFound">File or directory not exist by path</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task<ItemInfo> GetItemInfoAsync(UPath path);
        /// <exception cref="ItemNotFound">File or directory not exist by path</exception>
        /// <exception cref="UnexpectedItemType">Expected path to directory, but recieved path to file</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task<IEnumerable<ItemInfo>> GetDirectoryListAsync(UPath path);

        /// <exception cref="UnexpectedItemType">Expected path to directory or unexisted path, but recieved path to file</exception>
        /// <exception cref="ItemNotFound">Not founded parent directory</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task MakeDirectoryAsync(UPath path);
        /// <exception cref="UnexpectedItemType">Expected path to file or unexisted path, but recieved path to directory</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task RemoveFileAsync(UPath path);
        /// <exception cref="UnexpectedItemType">Expected path to directory or unexisted path, but recieved path to file</exception>
        /// <exception cref="InvalidOperationException">Directory isn`t empty, but recursive disabled</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task RemoveDirectoryAsync(UPath path, bool recursive);

        /// <exception cref="ItemNotFound">Not founded path to source</exception>
        /// <exception cref="InvalidOperationException">Incorrect name</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        Task RenameAsync(UPath path, string newName);

        /// <exception cref="UnexpectedItemType">Expected path to file or unexisted path, but recieved path to directory</exception>
        /// <exception cref="ItemNotFound">Not founded parent directory</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        /// <exception cref="OutOfSpaceLimit"></exception>
        Task UploadFileAsync(Stream stream, UPath path, IProgress<double> progressProvider = null);
        /// <exception cref="UnexpectedItemType">Expected path to file, but recieved path to directory</exception>
        /// <exception cref="ItemNotFound">Not founded file</exception>
        /// <exception cref="HostUnavailable">Token incorrect, or can`t resolve hostname</exception>
        /// <exception cref="InconsistentFileSystemState"></exception>
        /// <exception cref="OutOfSpaceLimit"></exception>
        Task DownloadFileAsync(Stream stream, UPath path, IProgress<double> progressProvider = null);
    }
}
