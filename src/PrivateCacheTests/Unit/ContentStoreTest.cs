namespace Tavis.PrivateCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Tavis.PrivateCache;
    using Xunit;

    public class ContentStoreTest
    {
        IContentStore Store;

        public ContentStoreTest()
        {
            Store = new InMemoryContentStore();
        }

        CacheContent CreateTestContent(IEnumerable<string> varyHeaders = null)
        {
            var content = new CacheContent()
            {
                Expires = DateTime.UtcNow.AddHours(1),
                HasValidator = true,
                Response = new HttpResponseMessage()
                {
                    RequestMessage = new HttpRequestMessage()
                    {
                        Content = new StringContent("42")
                    }
                }
            };

            foreach (var item in varyHeaders ?? Enumerable.Empty<string>())
            {
                content.Response.Headers.Vary.Add(item);
            }

            return content;
        }

        [Fact]
        public async Task Returns_empty_for_no_entries()
        {
            // Arrange
            var key = new PrimaryCacheKey("https://localhost/test", "POST");

            // Act
            var result = await Store.GetEntriesAsync(key);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Can_store_an_entry()
        {
            // Arrange
            var content = CreateTestContent();
            var key = new PrimaryCacheKey("https://localhost/test", "POST");
            var entryKey = new CacheEntryKey(new string[0]);
            var contentKey = new CacheContentKey(new string[0], content.Response.RequestMessage);
            content.PrimaryKey = key;
            content.ContentKey = contentKey;

            // Act
            await Store.UpdateEntryAsync(content);
            var entries = await Store.GetEntriesAsync(key);
            var entry1 = await Store.GetEntryAsync(key, entryKey);

            // Assert
            var entry = entries.Single();
            Assert.Equal(key, entry.PrimaryKey);
            Assert.Empty(entry.VaryHeaders);
            Assert.Equal(entry, entry1);
        }

        [Fact]
        public async Task Can_store_multiple_entries_that_differ_by_their_vary_headers()
        {
            // Arrange
            var key = new PrimaryCacheKey("https://localhost/test", "POST");

            // insert with no vary header
            var varyHeaders = new string[0];
            var content = CreateTestContent(varyHeaders);
            var contentKey = new CacheContentKey(varyHeaders, content.Response.RequestMessage);
            content.PrimaryKey = key;
            content.ContentKey = contentKey;
            await Store.UpdateEntryAsync(content);

            // insert with one vary header
            var varyHeaders2 = new[] { "Accept-Language" };
            var content2 = CreateTestContent(varyHeaders2);
            var contentKey2 = new CacheContentKey(varyHeaders2, content.Response.RequestMessage);
            content2.PrimaryKey = key;
            content2.ContentKey = contentKey2;
            await Store.UpdateEntryAsync(content2);

            // update with one vary header
            // Should replace the old entry
            content2 = CreateTestContent(varyHeaders2);
            content2.PrimaryKey = key;
            content2.ContentKey = contentKey2;
            await Store.UpdateEntryAsync(content2);

            // insert with one vary header
            var varyHeaders3 = new[] { "Accept-Language", "Accept-Encoding" };
            var content3 = CreateTestContent(varyHeaders3);
            var contentKey3 = new CacheContentKey(varyHeaders3, content.Response.RequestMessage);
            content3.PrimaryKey = key;
            content3.ContentKey = contentKey3;
            await Store.UpdateEntryAsync(content3);

            // Act            
            var entries = await Store.GetEntriesAsync(key);

            // Assert
            Assert.Equal(3, entries.Count());
            Assert.DoesNotThrow(() => entries.Single(x => x.EntryKey.Equals(new CacheEntryKey(varyHeaders))));
            Assert.DoesNotThrow(() => entries.Single(x => x.EntryKey.Equals(new CacheEntryKey(varyHeaders2))));
            Assert.DoesNotThrow(() => entries.Single(x => x.EntryKey.Equals(new CacheEntryKey(varyHeaders3))));
        }

        [Fact]
        public async Task Updates_an_entries_ResponseKeys()
        {
            // Arrange
            var content = CreateTestContent();
            var key = new PrimaryCacheKey("https://localhost/test", "POST");
            var entryKey = new CacheEntryKey(new string[0]);
            var contentKey = new CacheContentKey(new string[0], content.Response.RequestMessage);
            content.PrimaryKey = key;
            content.ContentKey = contentKey;

            // Act
            await Store.UpdateEntryAsync(content);
            var entry = await Store.GetEntryAsync(key, entryKey);

            // Assert
            Assert.Contains(contentKey, entry.ResponseKeys);
        }

        [Fact]
        public async Task Can_retrieve_a_stored_content()
        {
            // Arrange
            var content = CreateTestContent();
            var key = new PrimaryCacheKey("https://localhost/test", "POST");
            var contentKey = new CacheContentKey(new string[0], content.Response.RequestMessage);
            content.PrimaryKey = key;
            content.ContentKey = contentKey;

            // Act
            await Store.UpdateEntryAsync(content);
            var storedContent = await Store.GetContentAsync(key, contentKey);

            // Assert
            Assert.NotNull(storedContent);
            Assert.Equal(content.Response, storedContent.CreateResponse(), new HttpResponseMessageComparer());
        }
    }
}
