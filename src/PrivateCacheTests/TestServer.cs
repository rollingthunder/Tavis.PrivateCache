namespace Tavis.PrivateCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Tavis.PrivateCache;

    public class VirtualClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }

        public void Sleep(int ms)
        {
            UtcNow = UtcNow.AddMilliseconds(ms);
        }

        public VirtualClock ()
	    {
            UtcNow = DateTimeOffset.UtcNow;
	    }
    }

    public static class TestServer 
    {
        public static HttpServer CreateServer(IClock clock)
        {
            var DateHandler = new AddDateHeader(clock);
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default", "{controller}");
            config.MessageHandlers.Add(DateHandler);
            return new HttpServer(config);
        }        
    }

    public class AddDateHeader : DelegatingHandler
    {
        private IClock Clock;

        public AddDateHeader(IClock clock)
        {
            this.Clock = clock;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Date = Clock.UtcNow;
            return response;
        }
    }
}
