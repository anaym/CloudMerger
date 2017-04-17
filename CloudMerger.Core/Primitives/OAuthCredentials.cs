using System;
using System.Text.RegularExpressions;

namespace CloudMerger.Core.Primitives
{
    public class OAuthCredentials : IEquatable<OAuthCredentials>
    {
        public static readonly Regex ServicePattern = new Regex(@"^[^\s][^\n\0\r]*?$");
        public static readonly Regex LoginPattern = new Regex(@"^[^\n\0\r\s]*?$");
        public static readonly Regex TokenPattern = new Regex(@"^[a-zA-Z0-9\\=/-]*?$");
        public static readonly Regex CredentialsPattern = new Regex(@"^[\s]*(?<service>[^\n\0\r]*?) (?<login>[^\n\0\r\s]+) (?<token>[a-zA-Z0-9\\=/\-?]+)$");
        public const string NullString = "?";

        public static OAuthCredentials FromString(string s)
        {
            var matched = CredentialsPattern.Match(s);
            if (!matched.Success)
                throw new FormatException(s);
            var service = matched.Groups["service"].Value;
            var login = matched.Groups["login"].Value;
            var token = matched.Groups["token"].Value;
            return new OAuthCredentials
            {
                Service = service == NullString ? null : service,
                Login = login == NullString ? null : login,
                Token = token == NullString ? null : token
            };
        }

        public string Service
        {
            get { return service; }
            set
            {
                Validate(value, ServicePattern);
                service = value;
            }
        }
        public string Login
        {
            get { return login; }
            set
            {
                Validate(value, LoginPattern);
                login = value;
            }
        }
        public string Token
        {
            get { return token; }
            set
            {
                Validate(value, TokenPattern);
                token = value;
            }
        }


        public string SerializeToString()
        {
            return $"{Service ?? NullString} {Login ?? NullString} {Token ?? NullString}";
        }

        public override string ToString()
        {
            return SerializeToString();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OAuthCredentials);
        }
        public bool Equals(OAuthCredentials other)
        {
            if (other == null)
                return false;
            return Login == other.Login && Service == other.Service && Token == other.token;
        }
        public override int GetHashCode()
        {
            return (Login?.GetHashCode() ^ Token?.GetHashCode() ^ Service?.GetHashCode()) ?? 0;
        }

        private void Validate(string value, Regex pattern)
        {
            if (value != null && !pattern.IsMatch(value))
                throw new FormatException(value);
        }

        private string token;
        private string login;
        private string service;
    }
}