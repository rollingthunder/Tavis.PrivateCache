namespace Tavis.PrivateCache
{
    using System;
    using System.Net.Http;

    public class PrimaryCacheKey
    {
        public string Uri { get { return _Uri; } }
        public string Method { get { return _Method; } }

        string _Uri;
        string _Method;

        public PrimaryCacheKey(Uri uri, HttpMethod method) : this(uri.ToString(), method.Method) { }

        public PrimaryCacheKey(string uri, string method)
        {
            _Uri = uri;
            _Method = method;
        }

        public override bool Equals(object obj)
        {
            var key2 = (PrimaryCacheKey)obj;
            return key2._Uri == _Uri && key2._Method == _Method;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + _Uri.GetHashCode();
            hash = (hash * 7) + _Method.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return string.Format(
                "{0}->{1}",
                Method ?? string.Empty,
                Uri ?? string.Empty
                );
        }
    }
}