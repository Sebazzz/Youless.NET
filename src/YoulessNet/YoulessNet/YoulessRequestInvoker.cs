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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
            Justification = "Default parameter values are more explicit")]
        public YoulessRequestInvoker([NotNull] string host) : this(host, 0) {}

        /// <summary>
        ///     Initializes the <see cref="YoulessRequestInvoker" /> instance
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
            Justification = "Default parameter values are more explicit")]
        public YoulessRequestInvoker([NotNull] string host, int port = 80) : this(host, port, null) {}

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
        /// Invokes the method specified, getting a JSON response and performing deserialization to <typeparamref name="T"/>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters">Anonymous object specifying key/value pairs of parameters to pass, or an <see cref="IDictionary{TKey,TValue}"/> implementation</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An object of type <typeparamref name="T"/>, which contains the data retrieved from the web service</returns>
        /// <exception cref="ObjectDisposedException">Occurs when the current instance is disposed</exception>
        /// <exception cref="YoulessDataFormatException">Occurs when the response of the web service was not in the expected format</exception>
        /// <exception cref="YoulessException">Occurs when the response of the web service could not be retrieved</exception>
        public async Task<T> InvokeMethodAsync<T>(string method, [CanBeNull] object parameters, CancellationToken cancellationToken) where T : class, new() {
            this.EnsureNotDisposed();

            IDictionary<string, object> props = ObjectUtils.GetProperties(parameters ?? new object());
            
            // set format to json
            props["f"] = "j";

            // compose uri
            Uri relativePath = HttpUriComposer.Compose("/" + method, props);

            try {
                using (HttpResponseMessage response = await this._httpClient.ExecuteMethodAsync(relativePath, cancellationToken)) {

                    // read string, decode json
                    T item;
                    try {
                        item = SimpleJson.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }
                    catch (SerializationException ex) {
                        throw new YoulessDataFormatException("Could not decode JSON response", ex);
                    }

                    // execute raw-to-entity translation
                    ITranslateable translateableItem = item as ITranslateable;
                    if (translateableItem != null) {
                        translateableItem.Translate();
                    }

                    return item;
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
        /// <param name="isDisposing">Whether the object is disposing or finalizing</param>
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

        /// <summary>
        /// Ensures the current object is not disposed, or throws an <see cref="ObjectDisposedException"/>
        /// </summary>
        /// <exception cref="ObjectDisposedException">Occurs when the current instance is disposed</exception>
        protected void EnsureNotDisposed() {
            if (this._isDisposed) {
                throw new ObjectDisposedException(this.ToString());
            }
        }
    }
}