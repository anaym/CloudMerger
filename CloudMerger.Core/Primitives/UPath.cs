using System;
using System.Text.RegularExpressions;

namespace CloudMerger.Core.Primitives
{
    public struct UPath
    {
        public const char Separator = '/';
        public const char ReservedSeparator = '\\';
        public static readonly string NamePattern = @"[^/\\:*?""<>|]+?";
        public static readonly string PathPattern = $"^({Separator}{NamePattern})*$";
        public static readonly Regex NameRegex = new Regex(NamePattern);
        public static readonly Regex PathRegex = new Regex(PathPattern);

        public UPath(string path)
        {
            path = path.TrimEnd(Separator, ReservedSeparator);
            if (path == "")
            {
                this.path = Separator.ToString();
                Name = "";
                return;
            }
            this.path = Separator + path.Replace(ReservedSeparator, Separator).TrimStart(Separator);
            if (!PathRegex.Match(this.path).Success)
                throw new InvalidOperationException($"Incorrect path: {path}");
            var nbegin = this.path.LastIndexOf(Separator) + 1;
            Name = this.path.Substring(nbegin, this.path.Length - nbegin);
        }

        public string Name { get; }
        public UPath Parent => new UPath(path.Substring(0, path.LastIndexOf(Separator)));

        public UPath SubPath(string name)
        {
            return new UPath($"{UnixFormat()}/{name}");
        }

        public string UnixFormat() => Format('/');
        public string WindowsFormat(string drive) => Format($"{drive}:\\", '\\');
        public string Format(string begin, char separator)
        {
            if (begin == Separator.ToString())
                return Format(separator);
            return begin + Format(separator).Substring(1);
        }
        public string Format(char separator)
        {
            if (separator == Separator)
                return path;
            return path.Replace(Separator, separator);
        }

        public string AsString => path;

        public static implicit operator UPath(string path)
        {
            return new UPath(path);
        }
        public static implicit operator string(UPath path)
        {
            return path.path;
        }

        public override string ToString() => path;

        private readonly string path;
    }
}