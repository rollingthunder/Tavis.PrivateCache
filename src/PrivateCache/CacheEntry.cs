namespace Tavis.PrivateCache
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public class CacheEntry
    {
        public PrimaryCacheKey Key { get; protected set; }
        public IEnumerable<string> VaryHeaders { get; protected set; }

        /// <summary>
        /// Constructs a new CacheEntry.
        /// </summary>
        /// <param name="key">The primary key information for this entry.</param>
        /// <param name="varyHeaders">The Vary Header values for this entry.</param>
        public CacheEntry(PrimaryCacheKey key, IEnumerable<string> varyHeaders)
        {
            Key = key;
            VaryHeaders = varyHeaders;
        }       

        public string CreateSecondaryKey(HttpRequestMessage request)
        {
            var key = new StringBuilder(); 
            foreach (var h in VaryHeaders.OrderBy(v => v))  // Sort the vary headers so that ordering doesn't generate different stored variants
            {
                if (h != "*")
                {
                    key.Append(h).Append(':');
                    bool addedOne = false;
                    
                    IEnumerable<string> values;
                    if (request.Headers.TryGetValues(h, out values))
                    {
                        foreach (var val in values)
                        {
                            key.Append(val).Append(',');
                            addedOne = true;
                        }
                    }

                    if (addedOne)
                    {
                        key.Length--;  // truncate trailing comma.
                    }
                }
                else
                {
                    key.Append('*');
                }
            }
            return key.ToString().ToLowerInvariant();
        }

        public CacheContent CreateContent(HttpResponseMessage response)
        {
            return new CacheContent()
            {
                CacheEntry = this,
                Key = CreateSecondaryKey(response.RequestMessage),
                HasValidator = response.Headers.ETag != null || (response.Content != null && response.Content.Headers.LastModified != null),
                Expires = HttpCache.GetExpireDate(response),
                CacheControl = response.Headers.CacheControl ?? new CacheControlHeaderValue(),
                Response = response,
            };
        }
       
    }
}
