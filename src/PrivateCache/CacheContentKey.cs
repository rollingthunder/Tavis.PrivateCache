namespace Tavis.PrivateCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// Represents the discriminating key of one stored response.
    /// The key depends on the absence of values of all headers that are designated as Vary headers 
    /// in the <see cref="HttpRequestMessage"/> that led to this response.
    /// <remarks>A <see cref="CacheContentKey"/> whose vary headers list contains a "*" will compare unequal to any other key instance.</remarks>
    /// </summary>
    public class CacheContentKey : CacheEntryKey, IEquatable<CacheEntryKey>
    {
        /// <summary>
        /// A unique string value representing the key
        /// </summary>
        public string VaryValues { get; set; }

        /// <summary>
        /// Initializes a new <see cref="CacheContentKey"/> instance 
        /// from the list of designated Vary headers and two dictionaries of header values.
        /// </summary>
        /// <param name="varyHeaders">The List of designated Vary Headers.</param>
        /// <param name="requestHeaders">The first part of Header values (usually the Request Headers).</param>
        /// <param name="contentHeaders">The second part of Header values (usually the Content Headers).</param>
        public CacheContentKey(IEnumerable<string> varyHeaders, IDictionary<string, IEnumerable<string>> requestHeaders, IDictionary<string, IEnumerable<string>> contentHeaders)
            : base(varyHeaders)
        {
            Contract.Requires<ArgumentNullException>(varyHeaders != null);
            Contract.Requires<ArgumentNullException>(requestHeaders != null);
            Contract.Requires<ArgumentNullException>(contentHeaders != null);

            this.VaryValues = BuildVaryValues(varyHeaders, requestHeaders, contentHeaders);
        }

        /// <summary>
        /// Initializes a new <see cref="CacheContentKey"/> instance 
        /// from the list of designated Vary Headers and a request.
        /// </summary>
        /// <param name="varyHeaders">The List of designated Vary Headers.</param>
        /// <param name="request">The Request whose header values should be used to generate the key.</param>
        public CacheContentKey(IEnumerable<string> varyHeaders, HttpRequestMessage request)
            : base(varyHeaders)
        {
            Contract.Requires<ArgumentNullException>(varyHeaders != null);
            Contract.Requires<ArgumentNullException>(request != null);

            var requestHeaders = new HttpHeadersDictionaryAdapter(request.Headers);
            var contentHeaders =
                (request.Content != null)
                ? new HttpHeadersDictionaryAdapter(request.Content.Headers) as IDictionary<string, IEnumerable<string>>
                : new Dictionary<string, IEnumerable<string>>();

            this.VaryValues = BuildVaryValues(varyHeaders, requestHeaders, contentHeaders);
        }

        private static string BuildVaryValues(IEnumerable<string> varyHeaders, IDictionary<string, IEnumerable<string>> requestHeaders, IDictionary<string, IEnumerable<string>> contentHeaders)
        {
            Func<string, IEnumerable<string>> getHeaderValues = (header) =>
                {
                    IEnumerable<string> values;

                    if (requestHeaders.TryGetValue(header, out values) ||
                        contentHeaders.TryGetValue(header, out values))
                    {
                        return values;
                    }
                    else
                    {
                        return Enumerable.Empty<string>();
                    }
                };

            return string.Join(":", from header in varyHeaders
                                    orderby header
                                    select string.Join(",", getHeaderValues(header)));
        }

        #region Equals/HashCode
        public static bool operator !=(CacheContentKey x, CacheContentKey y)
        {
            return !CacheContentKeyComparer.KeyEquals(x, y);
        }

        /// <inheritdoc cref="Object.Equals(Object, Object)"/>
        public static bool operator ==(CacheContentKey x, CacheContentKey y)
        {
            return CacheContentKeyComparer.KeyEquals(x, y);
        }

        /// <inheritdoc cref="Object.Equals(Object)"/>
        public override bool Equals(object obj)
        {
            if (obj is CacheContentKey)
            {
                return this.Equals(obj as CacheContentKey);
            }

            return base.Equals(obj);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(CacheContentKey other)
        {
            return CacheContentKeyComparer.KeyEquals(this, other);
        }

        /// <inheritdoc cref="Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return CacheContentKeyComparer.KeyHash(this);
        }
        #endregion
    }

    /// <summary>
    /// Implements equality for cache content keys.
    /// Keys are considered equal, if they contain the same set of designated Vary Headers and the same corresponding values.
    /// </summary>
    public class CacheContentKeyComparer : IEqualityComparer<CacheContentKey>
    {
        /// <inheritdoc cref="IEqualityComparer{T}.Equals(T,T)"/>
        public bool Equals(CacheContentKey x, CacheContentKey y)
        {
            return KeyEquals(x, y);
        }

        /// <inheritdoc cref="IEqualityComparer{T}.GetHashCode(T)"/>
        public int GetHashCode(CacheContentKey obj)
        {
            return KeyHash(obj);
        }

        private static Lazy<CacheContentKeyComparer> _Instance = new Lazy<CacheContentKeyComparer>(() => new CacheContentKeyComparer());

        /// <summary>
        /// Gets the only existing instance, creating it if necessary.
        /// </summary>
        public static CacheContentKeyComparer Instance
        {
            get
            {
                return _Instance.Value;
            }
        }

        /// <summary>
        /// Prevent unnecessary instances from being created.
        /// </summary>
        private CacheContentKeyComparer()
        {
        }

        internal static int KeyHash(CacheContentKey x)
        {
            if (Object.ReferenceEquals(x, null))
            {
                return 0;
            }

            var hash = 101;
            hash = (hash * 79) + x.VaryHeaders.GetHashCode();
            hash = (hash * 79) + x.VaryValues.GetHashCode();
            return hash;
        }

        internal static bool KeyEquals(CacheContentKey x, CacheContentKey y)
        {
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
            {
                return Object.ReferenceEquals(x, y);
            }

            return x.VaryHeaders == y.VaryHeaders
                && x.VaryValues == y.VaryValues;
        }
    }
}