using System.Threading.Tasks;
using CloudMerger.Core.Primitives;

namespace CloudMerger.Core
{
    public interface IService
    {
        string Name { get; }

        IFileHosting GetFileHostingFor(OAuthCredentials credentials);
        Task<OAuthCredentials> AuthorizeAsync();
    }
}