namespace Tavis.PrivateCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Tavis.PrivateCache;

    /// <summary>
    /// Represents the backing store of a <see cref="HttpCache"/> that is used to
    /// search for existing stored responses and to add new ones for later retrieval
    /// </summary>    
    [ContractClass(typeof(IContentStoreContract))]
    public interface IContentStore
    {
        /// <summary>
        /// Gets the <see cref="CacheEntry"/> matching the given keys if one exists.
        /// </summary>
        /// <param name="primaryKey">The primary key (method, URI) to match.</param>
        /// <param name="entryKey">The secondary key (vary headers) to match.</param>
        /// <returns>The matching entry if it exists, otherwise <c>null</c></returns>
        Task<CacheEntry> GetEntryAsync(PrimaryCacheKey primaryKey, CacheEntryKey entryKey);

        /// <summary>
        /// Gets the list of <see cref="CacheEntry"/> matching the given key if one exists.
        /// </summary>
        /// <param name="primaryKey">The primary key (method, URI) to match.</param>
        /// <returns>A sequence of matching entries.</returns>
        Task<IEnumerable<CacheEntry>> GetEntriesAsync(PrimaryCacheKey primaryKey);

        /// <summary>
        /// Gets an implementation of <see cref="ICacheContent"/> matching the given keys if one exists.
        /// </summary>
        /// <param name="primaryKey">The primary key (method, URI) to match.</param>
        /// <param name="contentKey">The secondary key (vary headers and corresponding values) to match.</param>
        /// <returns>The Content if it exists, otherwise <c>null</c></returns>
        Task<ICacheContent> GetContentAsync(PrimaryCacheKey primaryKey, CacheContentKey contentKey);

        Task UpdateEntryAsync(CacheContent content);
    }

    /// <summary>
    /// Provides the Code Contracts for <see cref="IContentStore"/>
    /// </summary>
    [ContractClassFor(typeof(IContentStore))]
    public abstract class IContentStoreContract : IContentStore
    {
        /// <inheritdoc cref="IContentStore.GetEntryAsync(PrimaryCacheKey, CacheEntryKey)"/>
        public Task<CacheEntry> GetEntryAsync(PrimaryCacheKey primaryKey, CacheEntryKey entryKey)
        {
            Contract.Requires<ArgumentNullException>(primaryKey != null);
            Contract.Requires<ArgumentNullException>(entryKey != null);

            return Contract.Result<Task<CacheEntry>>();
        }

        /// <inheritdoc cref="IContentStore.GetEntriesAsync(PrimaryCacheKey)"/>
        public Task<IEnumerable<CacheEntry>> GetEntriesAsync(PrimaryCacheKey primaryKey)
        {
            Contract.Requires<ArgumentNullException>(primaryKey != null);

            Contract.Ensures(Contract.Result<Task<IEnumerable<CacheEntry>>>().Result != null);
            return Contract.Result<Task<IEnumerable<CacheEntry>>>();
        }

        /// <inheritdoc cref="IContentStore.GetContentAsync(PrimaryCacheKey, CacheContentKey)"/>
        public Task<ICacheContent> GetContentAsync(PrimaryCacheKey primaryKey, CacheContentKey contentKey)
        {
            Contract.Requires<ArgumentNullException>(primaryKey != null);
            Contract.Requires<ArgumentNullException>(contentKey != null);

            return Contract.Result<Task<ICacheContent>>();
        }

        /// <inheritdoc cref="IContentStore.UpdateEntryAsync(CacheContent)"/>
        public Task UpdateEntryAsync(CacheContent content)
        {
            Contract.Requires<ArgumentNullException>(content != null);
            Contract.Requires<ArgumentException>(content.Response != null, "The content argument does not contain a response.");
            Contract.Requires<ArgumentException>(content.Response.RequestMessage != null, "The response has no associated request.");

            return Contract.Result<Task>();
        }
    }
}