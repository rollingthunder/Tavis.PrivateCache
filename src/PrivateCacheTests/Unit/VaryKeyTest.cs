namespace PrivateCacheTests.Unit
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Tavis.PrivateCache;
    using Xunit;

    public class VaryKeyTest
    {
        [Fact]
        public void Compares_unequal_for_missing_vary_headers()
        {
            // Arrange            
            var varyHeaders = new[] { "Header1", "Header2" };

            var contentHeaders = new Dictionary<string, IEnumerable<string>>();
            var requestHeaders1 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4" } },
            };

            var requestHeaders2 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4" } },
            };

            var requestHeaders3 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },                
                {"Header3", new[]{ "value4" } },
            };

            // Act
            var key1 = new CacheContentKey(varyHeaders, requestHeaders1, contentHeaders);
            var key2 = new CacheContentKey(varyHeaders, requestHeaders2, contentHeaders);
            var key3 = new CacheContentKey(varyHeaders, requestHeaders3, contentHeaders);

            // Assert
            Assert.NotEqual(key1, key2);
            Assert.NotEqual(key1, key3);
            Assert.NotEqual(key3, key2);
        }

        [Fact]
        public void Always_Compares_unequal_for_star_header()
        {
            // Arrange            
            var varyHeaders = new[] { "*" };

            var contentHeaders = new Dictionary<string, IEnumerable<string>>();
            var requestHeaders = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4" } },
            };

            // Act
            var key1 = new CacheContentKey(varyHeaders, requestHeaders, contentHeaders);
            var key2 = new CacheContentKey(varyHeaders, requestHeaders, contentHeaders);
            var key3 = new CacheContentKey(varyHeaders, requestHeaders, contentHeaders);

            // Assert
            Assert.NotEqual(key1, key2);
            Assert.NotEqual(key1, key3);
            Assert.NotEqual(key3, key2);
        }

        [Fact]
        public void Compares_unequal_for_different_vary_headers()
        {
            // Arrange            
            var varyHeaders = new[] { "Header1", "Header2" };

            var contentHeaders = new Dictionary<string, IEnumerable<string>>();
            var requestHeaders1 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4" } },
            };

            var requestHeaders2 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1", "value5" } },
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4" } },
            };

            var requestHeaders3 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2" } },
                {"Header3", new[]{ "value4" } },
            };

            // Act
            var key1 = new CacheContentKey(varyHeaders, requestHeaders1, contentHeaders);
            var key2 = new CacheContentKey(varyHeaders, requestHeaders2, contentHeaders);
            var key3 = new CacheContentKey(varyHeaders, requestHeaders3, contentHeaders);

            // Assert
            Assert.NotEqual(key1, key2);
            Assert.NotEqual(key1, key3);
            Assert.NotEqual(key3, key2);
        }

        [Fact]
        public void Compares_equal_for_different_non_vary_headers()
        {
            // Arrange            
            var varyHeaders = new[] { "Header1", "Header2" };

            var contentHeaders = new Dictionary<string, IEnumerable<string>>();
            var requestHeaders1 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4" } },
            };

            var requestHeaders2 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2", "value3" } },
                {"Header3", new[]{ "value4", "value5" } },
            };

            var requestHeaders3 = new Dictionary<string, IEnumerable<string>>() 
            {
                {"Header1", new[]{ "value1" } },
                {"Header2", new[]{ "value2", "value3" } },
            };

            // Act
            var key1 = new CacheContentKey(varyHeaders, requestHeaders1, contentHeaders);
            var key2 = new CacheContentKey(varyHeaders, requestHeaders2, contentHeaders);
            var key3 = new CacheContentKey(varyHeaders, requestHeaders3, contentHeaders);

            // Assert
            Assert.Equal(key1, key2);
            Assert.Equal(key1, key3);
            Assert.Equal(key3, key2);
        }

        [Fact]
        public void Can_be_used_to_compare_Vary_Headers()
        {
            // Arrange            
            var varyHeaders = new[] { "Header1", "Header2" };
            var varyHeaders2 = new[] { "Header2", "Header1" };
            var varyHeaders3 = new[] { "Header1", "Header2", "Header3" };

            // Act
            var key1 = new CacheEntryKey(varyHeaders);
            var key2 = new CacheEntryKey(varyHeaders2);
            var key3 = new CacheEntryKey(varyHeaders3);

            // Assert
            Assert.Equal(key1, key2);
            Assert.NotEqual(key1, key3);
            Assert.NotEqual(key3, key2);
        }

        [Fact]
        public void A_CacheContentKey_compares_equal_for_a_CacheEntryKey_comparison()
        {
            // Arrange            
            var varyHeaders = new[] { "Header1", "Header2" };
            var noHeaders = new Dictionary<string, IEnumerable<string>>();

            // Act
            var headerKey = new CacheEntryKey(varyHeaders);
            var ValueKey = new CacheContentKey(varyHeaders, noHeaders, noHeaders);

            // Assert
            Assert.Equal<CacheEntryKey>(headerKey, ValueKey, CacheEntryKeyComparer.Instance);
        }
    }
}
