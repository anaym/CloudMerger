using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudMerger.Core;
using CloudMerger.Core.Primitives;
using CloudMerger.GuiPrimitives;

namespace YandexFileHosting
{
    public class YandexHostingManager : IHostingManager
    {
        public string Name => "Yandex Disk";
        public async Task<OAuthCredentials> AuthorizeAsync()
        {
            var start = new Uri(string.Format(Resource.StartAuthenticationUrlFormat, Resource.ApplicationToken));
            var result = await AuthorizationBrowser.CreateNewAsync(start, (uri, content) => accessTokenRegex.IsMatch(uri.AbsoluteUri));
            if (result == null)
                return null;
            return new OAuthCredentials
            {
                Token = accessTokenRegex.Match(result.Item1.AbsoluteUri).Groups["token"].Value,
                Login = loginRegex.IsMatch(result.Item2) ? loginRegex.Match(result.Item2).Groups["login"].Value : "-",
                Service = Name
            };
        }
        public IHosting GetFileHostingFor(OAuthCredentials credentials)
        {
            if (credentials.Token == null)
                throw new HostingException("Incorrect token");
            if (credentials.Service.ToLower() != Name.ToLower())
                throw new ArgumentException($"Credentials({credentials.Service}) is not valid for this hostingManager ({Name})");

            return new YandexHosting(credentials.Token, credentials.Login ?? credentials.Token.Substring(0, 8));
        }

        public override string ToString() => Name;

        private readonly Regex accessTokenRegex= new Regex(@"access_token=(?<token>[_a-zA-Z0-9-]+?)&");
        private readonly Regex loginRegex= new Regex(@"var login ?= ?""(?<login>[^"";\s]+?)""");
    }
}
