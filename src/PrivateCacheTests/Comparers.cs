namespace Tavis.PrivateCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>
    /// Implements equality comparison for enumerable sequences using set semantics.
    /// Two input sequences are equal, if and only if, they contain the same set of values according to element equality.
    /// <seealso cref="SetComparer{T}.SetComparer(IEqualityComparer{T})"/>
    /// <remarks>Requires the input sequences to be unique according to element equality.</remarks>
    /// </summary>
    /// <typeparam name="T">The element type of the sequences to be compared.</typeparam>
    public class SetComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        private IEqualityComparer<T> ElementEquality;

        /// <summary>
        /// Initializes a new instance of <see cref="SetComparer{T}"/> using the default equality comparer for T.
        /// </summary>
        public SetComparer()
        {
            ElementEquality = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SetComparer{T}"/> using <paramref name="elementEquality"/> as the equality comparer for T.
        /// </summary>
        /// <param name="elementEquality">Custom equality comparer for the sequence elements.</param>
        public SetComparer(IEqualityComparer<T> elementEquality)
        {
            Contract.Requires<ArgumentNullException>(elementEquality != null, "elementEquality");

            ElementEquality = elementEquality;
        }

        private HashSet<T> ValidateAndCreateSet(IEnumerable<T> x)
        {
            var xCount = x.Count();
            var xSet = new HashSet<T>(x, ElementEquality);

            if (xSet.Count != xCount)
            {
                throw new InvalidOperationException("Input sequence is not a set");
            }

            return xSet;
        }

        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            var xSet = ValidateAndCreateSet(x);

            return xSet.SetEquals(y);
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            var set = ValidateAndCreateSet(obj);

            return set.GetHashCode();
        }
    }

    /// <summary>
    /// Implements equality comparison for <see cref="HttpHeaders"/>.
    /// Header collections are considered equal if and only if they define the same headers 
    /// and each corresponding header has the same set of values.
    /// </summary>
    public class HttpHeadersComparer : IEqualityComparer<HttpHeaders>
    {
        static readonly IEqualityComparer<IEnumerable<string>> ValueComparer = new SetComparer<string>();

        public bool Equals(HttpHeaders x, HttpHeaders y)
        {
            var xCount = x.Count();
            var yCount = y.Count();

            if (xCount != yCount)
                return false;

            foreach (var header in x)
            {
                IEnumerable<string> values;

                if (y.TryGetValues(header.Key, out values))
                {
                    if (!ValueComparer.Equals(header.Value, values))
                    {
                        // Header has different values in y
                        return false;
                    }
                }
                else
                {
                    // Header does not exist in y
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(HttpHeaders obj)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Implements equality comparison for <see cref="HttpContent"/>.
    /// Contents are considered equal, if their headers are equal and they are the same, when converted to a string.
    /// </summary>
    public class HttpContentComparer : IEqualityComparer<HttpContent>
    {
        static readonly IEqualityComparer<HttpHeaders> HeaderComparer = new HttpHeadersComparer();

        public bool Equals(HttpContent x, HttpContent y)
        {
            if (x == null || y == null)
            {
                return x == null && y == null;
            }

            var xString = x.ReadAsStringAsync().Result;
            var yString = y.ReadAsStringAsync().Result;

            return xString == yString
                && HeaderComparer.Equals(x.Headers, y.Headers);
        }

        public int GetHashCode(HttpContent obj)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Implements equality comparison for <see cref="HttpResponseMessage"/>.
    /// Messages are considered equal, if their status, headers, and contents match.
    /// </summary>
    public class HttpResponseMessageComparer : IEqualityComparer<HttpResponseMessage>
    {
        static readonly IEqualityComparer<HttpHeaders> HeaderComparer = new HttpHeadersComparer();
        static readonly IEqualityComparer<HttpContent> ContentComparer = new HttpContentComparer();

        public bool Equals(HttpResponseMessage x, HttpResponseMessage y)
        {
            if(x == null || y == null)
            {
                return x == null && y == null;
            }

            var status = x.StatusCode == y.StatusCode;
            var headers = HeaderComparer.Equals(x.Headers, y.Headers);
            var content = ContentComparer.Equals(x.Content, y.Content);

            return status && headers && content;
        }

        public int GetHashCode(HttpResponseMessage obj)
        {
            throw new NotImplementedException();
        }
    }
}
