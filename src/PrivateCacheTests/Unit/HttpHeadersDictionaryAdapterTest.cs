namespace PrivateCacheTests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Tavis.PrivateCache;
    using Xunit;

    public class HttpHeadersDictionaryAdapterTest
    {
        [Fact]
        public void Can_return_header_values_using_the_dictionary_interface()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("de"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));
            var adapter = new HttpHeadersDictionaryAdapter(request.Headers) as IDictionary<string, IEnumerable<string>>;
            var acceptKey = "Accept-Language";

            // Act
            IEnumerable<string> acceptValues, acceptValues2;
            var success = adapter.TryGetValue(acceptKey, out acceptValues);
            acceptValues2 = adapter[acceptKey];
            


            // Assert
            Assert.True(success);
            Assert.True(adapter.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => adapter[acceptKey] = Enumerable.Empty<string>());
            Assert.Throws<NotSupportedException>(() => adapter.Add("anyheader", Enumerable.Empty<string>()));
            Assert.Equal(2, acceptValues.Count());
            Assert.Contains("de", acceptValues);
            Assert.Contains("fr", acceptValues);
            Assert.Equal(acceptValues, acceptValues2);
        }

        [Fact]
        public void Is_read_only()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("de"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));
            var adapter = new HttpHeadersDictionaryAdapter(request.Headers) as IDictionary<string, IEnumerable<string>>;
            var acceptKey = "Accept-Language";

            // Act

            // Assert
            Assert.True(adapter.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => adapter[acceptKey] = Enumerable.Empty<string>());
            Assert.Throws<NotSupportedException>(() => adapter.Add("anyheader", Enumerable.Empty<string>()));
        }
    }
}
