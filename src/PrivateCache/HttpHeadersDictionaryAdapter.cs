namespace Tavis.PrivateCache
{
    using System.Net.Http.Headers;
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// An adapter that allows treating <see cref="HttpHeaders"/> as a read only dictionary.
    /// </summary>
    public class HttpHeadersDictionaryAdapter : IDictionary<string, IEnumerable<string>>
    {
        private HttpHeaders inner;

        public HttpHeadersDictionaryAdapter(HttpHeaders inner)
        {
            Contract.Requires<ArgumentNullException>(inner != null);

            this.inner = inner;
        }

        public void Add(string key, IEnumerable<string> value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(string key)
        {
            IEnumerable<string> _;
            return inner.TryGetValues(key, out _);
        }

        public ICollection<string> Keys
        {
            get { throw new NotSupportedException(); }
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out IEnumerable<string> value)
        {
            return inner.TryGetValues(key, out value);
        }

        public ICollection<IEnumerable<string>> Values
        {
            get { throw new NotSupportedException(); }
        }

        public IEnumerable<string> this[string key]
        {
            get
            {
                return inner.GetValues(key);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void Add(KeyValuePair<string, IEnumerable<string>> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            inner.Clear();
        }

        public bool Contains(KeyValuePair<string, IEnumerable<string>> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, IEnumerable<string>>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return inner.Count(); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<string, IEnumerable<string>> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }
    }

    /// <summary>
    /// Contains Extension Methods for working with <see cref="HttpHeadersDictionaryAdapter"/>.
    /// </summary>
    public static class HttpHeadersDictionaryExtensions
    {
        /// <summary>
        /// Wraps a <see cref="HttpHeaders"/> instance in an adapter to enable use as a standard dictionary.
        /// </summary>
        /// <param name="This">The header collection to be wrapped.</param>
        /// <returns>A dictionary wrapper containing <paramref name="This"/>.</returns>
        public static IDictionary<string, IEnumerable<string>> AsDictionary(this HttpHeaders This)
        {
            return new HttpHeadersDictionaryAdapter(This);
        }
    }
}
