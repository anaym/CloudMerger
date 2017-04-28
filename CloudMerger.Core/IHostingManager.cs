using System.Threading.Tasks;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Core
{
    public interface IHostingManager
    {
        string Name { get; }

        IHosting GetFileHostingFor(OAuthCredentials credentials);
        Task<OAuthCredentials> AuthorizeAsync();
    }
}