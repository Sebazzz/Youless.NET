namespace YoulessNet {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using JetBrains.Annotations;

    /// <summary>
    ///     Represents a class to invoke methods on the Youless API
    /// </summary>
    public class YoulessRequestInvoker : IDisposable {
        private YoulessHttpClient _httpClient;
        private bool _isDisposed;

        /// <summary>
        ///     Initializes the <see cref="YoulessRequestInvoker" /> instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="credentials"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
            Justification = "Default parameter values are more explicit")]
        public YoulessRequestInvoker([NotNull] string host, int port = 80, ICredentials credentials = null) {
            if (host == null) {
                throw new ArgumentNullException("host");
            }
            if (String.IsNullOrWhiteSpace(host)) {
                throw new ArgumentException("Host name is required", "host");
            }
            if (port < 1 || port >= short.MaxValue) {
                throw new ArgumentOutOfRangeException("port",
                    "Port is out of range. Allowed values: > 0 or <= " + short.MaxValue);
            }

            this._httpClient = YoulessHttpClient.Create(BuildUri(host, port), credentials);
        }

        private static Uri BuildUri(string host, int port) {
            return new UriBuilder("http", host, port).Uri;
        }

        /// <summary>
        /// Invokes the method specified, getting a JSON response
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> InvokeMethodAsync<T>(string method, object parameters, CancellationToken cancellationToken) where T : class, new() {
            IDictionary<string, object> props = ObjectUtils.GetProperties(parameters);
            
            // set format to json
            props["f"] = "j";

            try {
                using (HttpResponseMessage response = await this._httpClient.GetAsync(
                    HttpUriComposer.Compose("/" + method, props), cancellationToken)) {

                    // read string, decode json
                    try {
                        return SimpleJson.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }
                    catch (SerializationException ex) {
                        throw new YoulessDataFormatException("Could not decode JSON response", ex);
                    }
                }
            } catch (Exception ex) {
                if (ex is YoulessException) {
                    throw;
                }

                throw new YoulessException("Unknown error while reading response", ex);
            }
        }


        /// <summary>
        ///     Disposes the <see cref="YoulessRequestInvoker" /> instance
        /// </summary>
        public void Dispose() {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the instance
        /// </summary>
        ~YoulessRequestInvoker() {
            this.Dispose(false);
        }


        /// <summary>
        ///     Disposes the current <see cref="YoulessRequestInvoker" /> instance
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void Dispose(bool isDisposing) {
            if (!this._isDisposed) {
                if (isDisposing) {
                    if (this._httpClient != null) {
                        this._httpClient.Dispose();
                    }
                }

                this._httpClient = null;
                this._isDisposed = true;
            }
        }
    }
}