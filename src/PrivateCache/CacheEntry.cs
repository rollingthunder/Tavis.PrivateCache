namespace Tavis.PrivateCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public class CacheEntry : IEquatable<CacheEntry>
    {
        public PrimaryCacheKey PrimaryKey { get; protected set; }
        public CacheEntryKey EntryKey { get; protected set; }
        public IEnumerable<string> VaryHeaders { get; protected set; }
        public ISet<CacheContentKey> ResponseKeys { get; protected set; }

        /// <summary>
        /// Constructs a new CacheEntry.
        /// </summary>
        /// <param name="key">The primary key information for this entry.</param>
        /// <param name="varyHeaders">The Vary Header values for this entry.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="varyHeaders"/> is <c>null</c></exception>
        public CacheEntry(PrimaryCacheKey key, IEnumerable<string> varyHeaders)
        {
            Contract.Requires<ArgumentNullException>(key != null, "key");
            Contract.Requires<ArgumentNullException>(varyHeaders != null, "varyHeaders");

            PrimaryKey = key;
            EntryKey = new CacheEntryKey(varyHeaders);
            VaryHeaders = varyHeaders.OrderBy(x => x).ToArray();
            ResponseKeys = new HashSet<CacheContentKey>(CacheContentKeyComparer.Instance);
        }

        /// <summary>
        /// Protected default constructor allows derived classes to bypass parameter validation.
        /// </summary>
        protected CacheEntry()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is CacheEntry)
            {
                return this.Equals(obj as CacheEntry);
            }

            return false;
        }

        public bool Equals(CacheEntry other)
        {
            if (other == null)
            {
                return false;
            }

            var thisCount = this.VaryHeaders.Count();
            var otherCount = other.VaryHeaders.Count();

            var varyEqual = this.VaryHeaders
                .Zip(other.VaryHeaders, (x, y) => x == y)
                .All(x => x);

            return this.PrimaryKey.Equals(other.PrimaryKey)
                && thisCount == otherCount
                && varyEqual;
        }
    }
}
