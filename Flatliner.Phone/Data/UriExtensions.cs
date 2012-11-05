namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Flatliner.Portable;

    /// <summary>
    /// Provides simplified API's for working with Uri objects and query strings.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Gets the first query string value with the given name.
        /// </summary>
        /// <typeparam name="T">The type of value to get from the query string.</typeparam>
        /// <param name="uri">The url to extract the query string from.</param>
        /// <param name="name">The name of the query string.</param>
        /// <returns>If the name is present then returns it's value, otherwise returns the default value of <typeparamref name="T"/>.</returns>
        public static T GetQuery<T>(this Uri uri, string name)
        {
            var output = uri.QueryStrings()[name].FirstOrDefault();
            if (output != null)
            {
                try
                {
                    return (T)Convert.ChangeType(output, typeof(T), CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException)
                {
                }
                catch (FormatException)
                {
                }
            }

            return default(T);
        }

        /// <summary>
        /// Gets a keyed sequence of query strings from a uri.
        /// </summary>
        /// <param name="uri">The url to extract the query strings from.</param>
        /// <returns>Returns a lookup of strings grouped by the name of the query string parameter.</returns>
        public static ILookup<string, string> QueryStrings(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                uri = new Uri(new Uri("http://localhost"), uri);
            }
            var q = from queryStrings in uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped).Split('&')
                    let parts = queryStrings.Split('=')
                    where parts[0].Length != 0
                    select new
                    {
                        Name = parts.FirstOrDefault(),
                        Value = Uri.UnescapeDataString(parts.Skip(1).FirstOrDefault() ?? string.Empty)
                    };

            return q.ToLookup(pair => pair.Name, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a query string value by converting the specified value to a string.
        /// </summary>
        /// <typeparam name="T">The type of value to be added.</typeparam>
        /// <param name="uri">The uri to add the query string parameter to.</param>
        /// <param name="name">The name of the query string value to add regardless of whether 
        /// the name is already used in the query string.</param>
        /// <param name="value">The value to convert to a string and escape.</param>
        /// <returns>Returns a new instance of Uri.</returns>
        public static Uri AddQuery<T>(this Uri uri, string name, T value)
        {
            var q = (from keyValues in uri.QueryStrings()
                     from keyValue in keyValues
                     select keyValues.Key + "=" + Uri.EscapeDataString(keyValue)).Concat(new[] { name + "=" + Uri.EscapeDataString(Convert.ToString(value)) });

            return new Uri(uri.GetHostPortAndPath() + "?" + q.Join("&"), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Adds all key value pairs in a dictionary as query string parameters to the specified uri.
        /// </summary>
        /// <param name="uri">The url to add all query values to.</param>
        /// <param name="queries">The query values to add.</param>
        /// <returns>Returns a new url with the query strings added.</returns>
        /// <typeparam name="T">The type of the values to be added to the query string.</typeparam>
        public static Uri AddAllQuery<T>(this Uri uri, IDictionary<string, T> queries)
        {
            return queries.Aggregate(uri, (u, kv) => u.AddQuery(kv.Key, kv.Value));
        }

        /// <summary>
        /// Adds or replaces a query string by name
        /// </summary>
        /// <typeparam name="T">The type of the value to be set.</typeparam>
        /// <param name="uri">The uri to set the query string for.</param>
        /// <param name="name">The name of the query string to add or replace (Uses a case-insensitive match).</param>
        /// <param name="value">The value to set the query string value to.</param>
        /// <returns>Retur a new instance of Uri.</returns>
        public static Uri SetQuery<T>(this Uri uri, string name, T value)
        {
            var q = (from keyValues in uri.QueryStrings()
                     where !string.Equals(keyValues.Key, name, StringComparison.OrdinalIgnoreCase)
                     from keyValue in keyValues
                     select keyValues.Key + "=" + Uri.EscapeDataString(keyValue)).Concat(new[] { name + "=" + Uri.EscapeDataString(Convert.ToString(value)) });

            return new Uri(uri.GetHostPortAndPath() + "?" + q.Join("&"), UriKind.RelativeOrAbsolute);
        }

        public static string GetHostPortAndPath(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return uri.OriginalString.SubstringToFirst("?");
            }

            return uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.Unescaped).SubstringToFirst("?");
        }

    }
}
