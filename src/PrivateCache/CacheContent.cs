namespace Tavis.PrivateCache
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public interface ICacheContent
    {
        PrimaryCacheKey PrimaryKey { get; }
        CacheContentKey ContentKey { get; }

        DateTimeOffset Expires { get; }

        bool HasValidator { get; }

        CacheControlHeaderValue CacheControl();

        HttpResponseMessage CreateResponse();
    }

    public class CacheContent
    {
        public PrimaryCacheKey PrimaryKey { get; set; }
        public CacheContentKey ContentKey { get; set; }

        public DateTimeOffset Expires { get; set; }

        public bool HasValidator { get; set; }

        public HttpResponseMessage Response { get; set; }

        public CacheContent()
        {
        }

        public CacheContent(ICacheContent content)
        {
            this.PrimaryKey = content.PrimaryKey;
            this.ContentKey = content.ContentKey;
            this.Expires = content.Expires;
            this.HasValidator = content.HasValidator;
            this.Response = content.CreateResponse();
        }
    }
}