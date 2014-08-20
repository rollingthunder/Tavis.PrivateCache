namespace Tavis.PrivateCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="varyHeaders"/> is <c>null</c></exception>
        public CacheEntry(PrimaryCacheKey key, IEnumerable<string> varyHeaders)
        {
            Contract.Requires<ArgumentNullException>(key != null, "key");
            Contract.Requires<ArgumentNullException>(varyHeaders != null, "varyHeaders");

            Key = key;
            VaryHeaders = varyHeaders;
        }     
  
        /// <summary>
        /// Protected default constructor allows derived classes to bypass parameter validation.
        /// </summary>
        protected CacheEntry()
        {
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
