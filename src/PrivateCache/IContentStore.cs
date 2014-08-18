﻿namespace Tavis.PrivateCache
{
    using System.Threading.Tasks;
    using Tavis.PrivateCache;

    public interface IContentStore
    {

        // Fast lookup to determine if any representations exist for the method and URI
        // Every HTTP request pays this cost regardless of hit or not
        Task<CacheEntry> GetEntryAsync(PrimaryCacheKey cacheKey);

        // Retreive actual content based on variant selection key
        Task<CacheContent> GetContentAsync(CacheEntry entry, string secondaryKey);

        Task UpdateEntryAsync(CacheContent content);
    }
}