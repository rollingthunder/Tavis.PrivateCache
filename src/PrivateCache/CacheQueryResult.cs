namespace Tavis.PrivateCache
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net.Http;

    public enum CacheStatus
    {
        CannotUseCache,
        Revalidate,
        ReturnStored
    }

    public class CacheQueryResult
    {
        private readonly HttpCache Cache;
        public CacheStatus Status;
        public ICacheContent SelectedVariant;
        private HttpResponseMessage Response;

        public CacheQueryResult(HttpCache cache, HttpResponseMessage response = null)
        {
            Cache = cache;
            Response = response;
        }

        internal HttpResponseMessage GetHttpResponseMessage(HttpRequestMessage request)
        {
            var response = Response ?? SelectedVariant.CreateResponse();
            response.RequestMessage = request;
            Cache.UpdateAgeHeader(response);
            return response;
        }


        internal void ApplyConditionalHeaders(HttpRequestMessage request)
        {
            Contract.Assert(SelectedVariant != null);

            if (SelectedVariant == null || !SelectedVariant.HasValidator)
            {
                return;
            }

            var httpResponseMessage = SelectedVariant.CreateResponse();

            if (httpResponseMessage.Headers.ETag != null)
            {
                request.Headers.IfNoneMatch.Add(httpResponseMessage.Headers.ETag);
            }
            else
            {
                if (httpResponseMessage.Content != null && httpResponseMessage.Content.Headers.LastModified != null)
                {
                    request.Headers.IfModifiedSince = httpResponseMessage.Content.Headers.LastModified;
                }
            }
        }
    }
}