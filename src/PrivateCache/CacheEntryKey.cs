namespace Tavis.PrivateCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents a key for a <see cref="CacheEntry"/>.
    /// Only depends on which headers have been designated Vary Headers.
    /// </summary>
    public class CacheEntryKey : IEquatable<CacheEntryKey>
    {
        /// <summary>
        /// Gets a unique string generated from the list of designated Vary Headers.
        /// </summary>
        public string VaryHeaders { get; protected set; }

        /// <summary>
        /// Initializes a new instance of <see cref="CacheEntryKey"/>.
        /// </summary>
        /// <param name="varyHeaders">The list of Headers that have been designated Vary Headers.</param>
        public CacheEntryKey(IEnumerable<string> varyHeaders)
        {
            Contract.Requires<ArgumentNullException>(varyHeaders != null);

            if (varyHeaders.Contains("*"))
            {
                VaryHeaders = "*";
            }
            else
            {
                VaryHeaders = string.Join(":", varyHeaders.OrderBy(x => x));
            }
        }

        #region Equals/HashCode
        /// <inheritdoc cref="Object.Equals(Object)"/>
        public override bool Equals(object obj)
        {
            if (obj is CacheEntryKey)
            {
                return this.Equals(obj as CacheEntryKey);
            }

            return false;
        }

        /// <inheritdoc cref="Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            return CacheEntryKeyComparer.KeyHash(this);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(CacheEntryKey other)
        {
            return CacheEntryKeyComparer.KeyEquals(this, other);
        }

        public static bool operator ==(CacheEntryKey x, CacheEntryKey y)
        {
            return CacheEntryKeyComparer.KeyEquals(x, y);
        }

        public static bool operator !=(CacheEntryKey x, CacheEntryKey y)
        {
            return !CacheEntryKeyComparer.KeyEquals(x, y);
        }
        #endregion
    }

    /// <summary>
    /// Implements equality for cache entry keys.
    /// Keys are considered equal, if they contain the same set of designated Vary Headers.
    /// </summary>
    public class CacheEntryKeyComparer : IEqualityComparer<CacheEntryKey>
    {
        /// <inheritdoc cref="IEqualityComparer{T}.Equals(T,T)"/>
        public bool Equals(CacheEntryKey x, CacheEntryKey y)
        {
            return KeyEquals(x, y);
        }

        /// <inheritdoc cref="IEqualityComparer{T}.GetHashCode(T)"/>
        public int GetHashCode(CacheEntryKey obj)
        {
            return KeyHash(obj);
        }

        private static Lazy<CacheEntryKeyComparer> _Instance = new Lazy<CacheEntryKeyComparer>(() => new CacheEntryKeyComparer());

        /// <summary>
        /// Gets the only existing instance, creating it if necessary.
        /// </summary>
        public static CacheEntryKeyComparer Instance
        {
            get
            {
                return _Instance.Value;
            }
        }

        /// <summary>
        /// Prevent unnecessary instances from being created.
        /// </summary>
        private CacheEntryKeyComparer()
        {
        }

        internal static bool KeyEquals(CacheEntryKey x, CacheEntryKey y)
        {
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
            {
                return Object.ReferenceEquals(x, y);
            }

            return x.VaryHeaders == y.VaryHeaders;
        }

        internal static int KeyHash(CacheEntryKey obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return 0;
            }

            return obj.VaryHeaders.GetHashCode();
        }
    }
}
