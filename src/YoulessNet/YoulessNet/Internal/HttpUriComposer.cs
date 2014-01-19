namespace YoulessNet.Internal {
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Helper class for composing <see cref="Uri"/> instances (HTTP only)
    /// </summary>
    internal static class HttpUriComposer {
        /// <summary>
        /// Composes an <see cref="Uri"/> from the specified parameters
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Uri Compose(string path) {
            return Compose(path, (string) null);
        }

        /// <summary>
        /// Composes an <see cref="Uri"/> from the specified parameters
        /// </summary>
        /// <param name="path"></param>
        /// <param name="query">Anonymous object descibing the properties</param>
        /// <returns></returns>
        public static Uri Compose(string path, object query) {
            IDictionary<string, object> queryKeyValues = query != null ? ObjectUtils.GetProperties(query) : null;

            return Compose(path, queryKeyValues);
        }

        /// <summary>
        /// Composes an <see cref="Uri"/> from the specified parameters
        /// </summary>
        /// <param name="path"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Uri Compose(string path, string query) {
            UriBuilder builder = new UriBuilder();
            builder.Path = path;
            builder.Query = query;

            return builder.Uri;
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

                    queryBuilder.AppendFormat("{0}={1}&", key, encodedValue);
                }

                if (queryKeyValues.Count > 0) {
                    queryBuilder.Remove(queryBuilder.Length - 2, 1);
                }
            }

            return queryBuilder.ToString();
        }

    }
}