namespace YoulessNet.Internal {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Helper class for composing <see cref="Uri"/> instances (HTTP only)
    /// </summary>
    internal static class HttpUriComposer {
        /// <summary>
        /// Composes an <see cref="Uri"/> from the specified parameters
        /// </summary>
        /// <param name="path"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Uri Compose(string path, string query) {
            return new Uri(path + "?" + query, UriKind.Relative);
        }

        /// <summary>
        /// Composes an <see cref="Uri"/> from the specified parameters
        /// </summary>
        /// <param name="path"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Uri Compose(string path, IDictionary<string, object> query) {
            return Compose(path, ComposeQuery(query));
        }


        private static string ComposeQuery(IDictionary<string, object> queryKeyValues) {
            StringBuilder queryBuilder = new StringBuilder();

            if (queryKeyValues != null) {
                foreach (KeyValuePair<string, object> keyValuePair in queryKeyValues) {
                    string key = keyValuePair.Key;
                    string value = ObjectUtils.GetCultureNeutralString(keyValuePair.Value);

                    string encodedValue = Uri.EscapeDataString(value);

                    queryBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", key, encodedValue);
                }

                if (queryKeyValues.Count > 0) {
                    queryBuilder.Remove(queryBuilder.Length - 1, 1);
                }
            }

            return queryBuilder.ToString();
        }

    }
}