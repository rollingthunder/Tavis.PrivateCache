namespace Tavis.PrivateCache
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IContentStore"/> implementation using main memory to store Cache information.
    /// </summary>
    public class InMemoryContentStore : IContentStore
    {
        private readonly IDictionary<PrimaryCacheKey, IDictionary<CacheEntryKey, InMemoryCacheEntry>> CacheEntries;

        /// <summary>
        /// Initialized a new <see cref="InMemoryContentStore"/> instance.
        /// </summary>
        public InMemoryContentStore()
        {
            CacheEntries = new Dictionary<PrimaryCacheKey, IDictionary<CacheEntryKey, InMemoryCacheEntry>>();
        }

        /// <inheritdoc cref="IContentStore.GetEntryAsync(PrimaryCacheKey, CacheEntryKey)"/>
        public Task<CacheEntry> GetEntryAsync(PrimaryCacheKey key, CacheEntryKey entryKey)
        {
            var entries = GetCacheEntries(key, false);

            if (entries == null)
            {
                return null;
            }

            var entry = null as InMemoryCacheEntry;

            lock (entries)
            {
                entries.TryGetValue(entryKey, out entry);
            }

            return Task.FromResult<CacheEntry>(entry);
        }

        /// <inheritdoc cref="IContentStore.GetEntriesAsync(PrimaryCacheKey)"/>
        public Task<IEnumerable<CacheEntry>> GetEntriesAsync(PrimaryCacheKey key)
        {
            var entries = GetCacheEntries(key, false);

            if (entries == null)
            {
                return Task.FromResult(Enumerable.Empty<CacheEntry>());
            }

            lock (entries)
            {
                return Task.FromResult<IEnumerable<CacheEntry>>(entries.Values.ToList());
            }
        }

        /// <inheritdoc cref="IContentStore.GetContentAsync(PrimaryCacheKey, CacheContentKey)"/>
        public async Task<ICacheContent> GetContentAsync(PrimaryCacheKey primaryKey, CacheContentKey contentKey)
        {
            var entry = await GetEntryAsync(primaryKey, contentKey) as InMemoryCacheEntry;

            if (entry == null)
            {
                return null;
            }

            return GetCacheContent(entry, contentKey);
        }

        /// <inheritdoc cref="IContentStore.UpdateEntryAsync(CacheContent)"/>
        public Task UpdateEntryAsync(CacheContent content)
        {
            var varyHeaders = content.Response.Headers.Vary ?? Enumerable.Empty<string>();

            var entries = GetCacheEntries(content.PrimaryKey, true);

            InMemoryCacheEntry varyEntry;

            lock (entries)
            {
                var headerKey = new CacheEntryKey(varyHeaders);
                if (!entries.TryGetValue(headerKey, out varyEntry))
                {
                    varyEntry = new InMemoryCacheEntry(content.PrimaryKey, varyHeaders);

                    entries.Add(headerKey, varyEntry);
                }
            }

            var request = content.Response.RequestMessage;
            var requestHeaders = request.Headers.AsDictionary();
            var contentHeaders =
                (request.Content != null)
                ? request.Content.Headers.AsDictionary()
                : new Dictionary<string, IEnumerable<string>>();
            var responseKey = new CacheContentKey(varyHeaders, requestHeaders, contentHeaders);

            lock (varyEntry)
            {
                varyEntry.ResponseKeys.Add(responseKey);
                varyEntry.Responses[responseKey] = new InMemoryCacheContent(content);
            }

            return Task.FromResult<object>(null);
        }

        private IDictionary<CacheEntryKey, InMemoryCacheEntry> GetCacheEntries(PrimaryCacheKey key, bool createIfNecessary = false)
        {
            IDictionary<CacheEntryKey, InMemoryCacheEntry> entries;
            lock (CacheEntries)
            {
                if (!CacheEntries.TryGetValue(key, out entries) && createIfNecessary)
                {
                    entries = new Dictionary<CacheEntryKey, InMemoryCacheEntry>(CacheEntryKeyComparer.Instance);

                    CacheEntries.Add(key, entries);
                }
            }
            return entries;
        }

        private ICacheContent GetCacheContent(InMemoryCacheEntry entry, CacheContentKey key)
        {
            InMemoryCacheContent content;
            lock (entry)
            {
                entry.Responses.TryGetValue(key, out content);
            }

            return content;
        }
    }

    internal class InMemoryCacheEntry : CacheEntry
    {
        public IDictionary<CacheContentKey, InMemoryCacheContent> Responses { get; private set; }

        public InMemoryCacheEntry(PrimaryCacheKey key, IEnumerable<string> varyHeaders)
            : base(key, varyHeaders)
        {
            Responses = new Dictionary<CacheContentKey, InMemoryCacheContent>(CacheContentKeyComparer.Instance);
        }
    }

    internal class InMemoryCacheContent : ICacheContent
    {
        public PrimaryCacheKey PrimaryKey
        {
            get
            {
                return Content.PrimaryKey;
            }
        }

        public CacheContentKey ContentKey
        {
            get { return Content.ContentKey; }
        }

        public System.DateTimeOffset Expires
        {
            get { return Content.Expires; }
        }

        public bool HasValidator
        {
            get { return Content.HasValidator; }
        }

        private CacheContent Content;

        public InMemoryCacheContent(CacheContent content)
        {
            Content = content;
        }

        public CacheControlHeaderValue CacheControl()
        {
            return Content.Response.Headers.CacheControl;
        }

        public HttpResponseMessage CreateResponse()
        {
            return Content.Response;
        }
    }
}