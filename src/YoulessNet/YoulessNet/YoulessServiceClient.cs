namespace YoulessNet {
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a client to a Youless web service
    /// </summary>
    public class YoulessServiceClient : IDisposable {
        private bool _isDisposed;
        private YoulessRequestInvoker _requestInvoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessServiceClient"/> class. 
        /// </summary>
        /// <param name="requestInvoker">Request invoker. The service takes ownership of the instance.</param>
        public YoulessServiceClient([NotNull] YoulessRequestInvoker requestInvoker) {
            if (requestInvoker == null) {
                throw new ArgumentNullException("requestInvoker");
            }

            this._requestInvoker = requestInvoker;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessServiceClient"/> class
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="credentials"></param>
        public YoulessServiceClient(string hostName, int port, ICredentials credentials) {
            this._requestInvoker = new YoulessRequestInvoker(hostName, port, credentials);
        }

        /// <summary>
        /// Gets the current status of the Youless Energy Meter in an asynchronous manner
        /// </summary>
        /// <returns></returns>
        public async Task<YoulessStatus> GetStatusAsync() {
            return await GetStatusAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the current status of the Youless Energy Meter in an asynchronous manner
        /// </summary>
        /// <returns></returns>
        public async Task<YoulessStatus> GetStatusAsync(CancellationToken ct) {
            return await this._requestInvoker.InvokeMethodAsync<RawYoulessStatus>(Methods.Status, null, ct);
        }

        /// <summary>
        ///     Disposes the <see cref="YoulessServiceClient" /> instance
        /// </summary>
        public void Dispose() {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the current instance
        /// </summary>
        ~YoulessServiceClient() {
            this.Dispose(false);
        }

        /// <summary>
        ///     Disposes the current <see cref="YoulessServiceClient" /> instance
        /// </summary>
        /// <param name="isDisposing">Whether the object is disposing or finalizing</param>
        protected virtual void Dispose(bool isDisposing) {
            if (!this._isDisposed) {
                if (isDisposing) {
                    if (this._requestInvoker != null) {
                        this._requestInvoker.Dispose();
                    }
                }

                this._requestInvoker = null;
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

        /// <summary>
        /// Enumerates the methods available on the Youless Web Interface
        /// </summary>
        /// <remarks>
        /// Many of the method are short one letter characters
        /// </remarks>
        private static class Methods {
            /// <summary>
            /// Gets the method of the status page
            /// </summary>
            public const string Status = "a";
        }
    }
}