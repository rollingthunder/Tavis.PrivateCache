namespace Tavis.PrivateCache
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;


    public class HttpCache
    {
        private readonly IContentStore _contentStore;
        private readonly IClock Clock;

        public Func<HttpResponseMessage, bool> StoreBasedOnHeuristics = (r) => false;

        public Dictionary<HttpMethod, object> CacheableMethods = new Dictionary<HttpMethod, object>
        {
            {HttpMethod.Get, null},
            {HttpMethod.Head, null},
            {HttpMethod.Post,null}
        };

        public HttpCache(IContentStore contentStore, IClock clock = null)
        {
            _contentStore = contentStore;
            Clock = clock ?? new RealClock();
        }

        public async Task<CacheQueryResult> QueryCacheAsync(HttpRequestMessage request)
        {
            var primaryKey = new PrimaryCacheKey(request.RequestUri, request.Method);

            // Do we have anything stored for this method and URI?
            var cacheEntries = await _contentStore.GetEntriesAsync(primaryKey);
            if (!cacheEntries.Any())
            {
                return CannotUseCache();
            }

            // Do we have a matching variant representation?
            var selectedResponses = await GetSelectedResponsesAsync(cacheEntries, request);
            if (!selectedResponses.Any())
            {
                return CannotUseCache();
            }

            var selected = GetPreferredResponse(selectedResponses);
            var selectedContent = selected.Item1;
            var selectedResponse = selected.Item2;

            // Do caching directives require that we re-validate it regardless of freshness?
            var requestCacheControl = request.Headers.CacheControl ?? new CacheControlHeaderValue();
            var responseCacheControl = selectedContent.CacheControl();
            if ((requestCacheControl.NoCache || responseCacheControl.NoCache))
            {
                return Revalidate(selectedContent);
            }

            // Is it fresh?
            if (IsFresh(selectedContent))
            {
                if (requestCacheControl.MinFresh != null)
                {
                    if (CalculateAge(selectedResponse) <= requestCacheControl.MinFresh)
                    {
                        return ReturnStored(selectedContent, selectedResponse);
                    }
                }
                else
                {
                    return ReturnStored(selectedContent, selectedResponse);
                }
            }

            // Did the client say we can serve it stale?
            if (requestCacheControl.MaxStale)
            {
                if (requestCacheControl.MaxStaleLimit != null)
                {
                    if ((Clock.UtcNow - selectedContent.Expires) <= requestCacheControl.MaxStaleLimit)
                    {
                        return ReturnStored(selectedContent, selectedResponse);
                    }
                }
                else
                {
                    return ReturnStored(selectedContent, selectedResponse);
                }
            }

            // Do we have a selector to allow us to do a conditional request to re-validate it?
            if (selectedContent.HasValidator)
            {
                return Revalidate(selectedContent);
            }

            // Can't do anything to help
            return CannotUseCache();
        }

        private bool IsFresh(ICacheContent selectedContent)
        {
            return selectedContent.Expires > Clock.UtcNow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedResponses"></param>
        /// <returns></returns>
        private Tuple<ICacheContent, HttpResponseMessage> GetPreferredResponse(IEnumerable<ICacheContent> selectedResponses)
        {
            return (from content in selectedResponses
                    let response = content.CreateResponse()
                    orderby response.Headers.Date ?? DateTimeOffset.MinValue
                    select Tuple.Create(content, response)).First();
        }

        private async Task<IEnumerable<ICacheContent>> GetSelectedResponsesAsync(IEnumerable<CacheEntry> cacheEntries, HttpRequestMessage request)
        {
            var result = new List<ICacheContent>();

            var selectedKeys = from entry in cacheEntries
                               let requestKey = new CacheContentKey(entry.VaryHeaders, request)
                               from contentKey in entry.ResponseKeys
                               where requestKey.Equals(contentKey)
                               select new { Primary = entry.PrimaryKey, Content = contentKey };
            foreach (var keyPair in selectedKeys)
            {
                result.Add(await _contentStore.GetContentAsync(keyPair.Primary, keyPair.Content));
            }

            return result;
        }

        public bool CanStore(HttpResponseMessage response)
        {
            // Only cache responses from methods that allow their responses to be cached
            if (!CacheableMethods.ContainsKey(response.RequestMessage.Method)) return false;


            var cacheControlHeaderValue = response.Headers.CacheControl;

            // Ensure that storing is not explicitly prohibited
            if (cacheControlHeaderValue != null && cacheControlHeaderValue.NoStore) return false;

            if (response.RequestMessage.Headers.CacheControl != null && response.RequestMessage.Headers.CacheControl.NoStore) return false;

            // Ensure we have some freshness directives as this cache doesn't do heuristic based caching

            if (response.Content != null && response.Content.Headers.Expires != null) return true;
            if (cacheControlHeaderValue != null)
            {
                if (cacheControlHeaderValue.MaxAge != null) return true;
                if (cacheControlHeaderValue.SharedMaxAge != null) return true;
            }

            var sc = (int)response.StatusCode;
            if (sc == 200 || sc == 203 || sc == 204 ||
                 sc == 206 || sc == 300 || sc == 301 ||
                 sc == 404 || sc == 405 || sc == 410 ||
                 sc == 414 || sc == 501)
            {
                return StoreBasedOnHeuristics(response);
            }

            return false;
        }

        public async Task UpdateContentAsync(HttpResponseMessage notModifiedResponse, ICacheContent cacheContent)
        {
            var newExpires = GetExpireDate(notModifiedResponse);

            var newContent = new CacheContent(cacheContent);

            if (newExpires > newContent.Expires)
            {
                newContent.Expires = newExpires;
            }
            //TODO Copy headers from notModifiedResponse to cacheContent

            await _contentStore.UpdateEntryAsync(newContent);
        }

        public async Task StoreResponseAsync(HttpResponseMessage response)
        {
            var primaryCacheKey = new PrimaryCacheKey(response.RequestMessage.RequestUri, response.RequestMessage.Method);
            var contentKey = new CacheContentKey(response.Headers.Vary, response.RequestMessage);

            var content = new CacheContent()
            {
                PrimaryKey = primaryCacheKey,
                ContentKey = contentKey,
                Expires = GetExpireDate(response),
                HasValidator = GetHasValidator(response),
                Response = response
            };

            await _contentStore.UpdateEntryAsync(content);

        }

        public DateTimeOffset GetExpireDate(HttpResponseMessage response)
        {
            if (response.Headers.CacheControl != null && response.Headers.CacheControl.MaxAge != null)
            {
                return Clock.UtcNow + response.Headers.CacheControl.MaxAge.Value;
            }
            else
            {
                if (response.Content != null && response.Content.Headers.Expires != null)
                {
                    return response.Content.Headers.Expires.Value;
                }
            }
            return Clock.UtcNow;  // Store but assume stale
        }

        public static bool GetHasValidator(HttpResponseMessage response)
        {
            return response.Headers.ETag != null || (response.Content != null && response.Content.Headers.LastModified != null);
        }

        public void UpdateAgeHeader(HttpResponseMessage response)
        {
            if (response.Headers.Date.HasValue)
            {
                response.Headers.Age = CalculateAge(response);
            }
        }

        public TimeSpan CalculateAge(HttpResponseMessage response)
        {
            var age = Clock.UtcNow - response.Headers.Date.Value;
            if (age.TotalMilliseconds < 0) age = new TimeSpan(0);

            return new TimeSpan(0, 0, (int)Math.Round(age.TotalSeconds)); ;
        }


        public CacheQueryResult CannotUseCache()
        {
            return new CacheQueryResult(this)
            {
                Status = CacheStatus.CannotUseCache
            };
        }

        public CacheQueryResult Revalidate(ICacheContent cacheContent)
        {
            return new CacheQueryResult(this)
            {
                Status = CacheStatus.Revalidate,
                SelectedVariant = cacheContent
            };
        }

        public CacheQueryResult ReturnStored(ICacheContent cacheContent, HttpResponseMessage response = null)
        {
            return new CacheQueryResult(this, response)
            {
                Status = CacheStatus.ReturnStored,
                SelectedVariant = cacheContent
            };
        }
    }
}