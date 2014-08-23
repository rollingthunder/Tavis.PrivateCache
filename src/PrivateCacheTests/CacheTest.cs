namespace Tavis.PrivateCache.Test
{
    using System;
    using System.Net.Http;
    using Xunit;

    public class CacheTest
    {
        protected readonly Uri BaseAddress;
        protected readonly VirtualClock Clock;
        protected readonly HttpClient Client;

        public CacheTest()
        {
            BaseAddress = new Uri(string.Format("http://{0}:1001", Environment.MachineName));
            Clock = new VirtualClock();
            Client = CreateCachingEnabledClient();
        }

        private HttpClient CreateCachingEnabledClient()
        {
            var httpClientHandler = TestServer.CreateServer(Clock);

            var clientHandler = new PrivateCacheHandler(httpClientHandler, new HttpCache(new InMemoryContentStore(), Clock));
            var client = new HttpClient(clientHandler) { BaseAddress = BaseAddress };
            return client;
        }
    }

    public static class HttpAssert
    {
        public static void FromCache(HttpResponseMessage response)
        {
            Assert.NotNull(response.Headers.Age);
        }

        public static void FromServer(HttpResponseMessage response)
        {
            Assert.Null(response.Headers.Age);
        }
    }
}
