namespace YoulessNet {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
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
        /// Gets the measurements from the last hour - these are the most accurate (60 seconds interval)
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<YoulessUsageData> GetLastHourMeasurement(CancellationToken ct) {
            YoulessUsageData data = await GetMeasurements(new Tuple<string, int>("h", 1), ct);
            YoulessUsageData data2 = await GetMeasurements(new Tuple<string, int>("h", 2), ct);

            return data.MergeWith(data2);
        }

        /// <summary>
        /// Gets the measurements from the Youless using the specified parameters
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected async Task<YoulessUsageData> GetMeasurements(Tuple<string, int> parameter, CancellationToken ct) {
            IDictionary @params = new Dictionary<string, int>();
            @params[parameter.Item1] = parameter.Item2;

            return await this._requestInvoker.InvokeMethodAsync<RawYoulessUsageData>(Methods.Measurements, @params, ct);
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

            /// <summary>
            /// Gets the method of the measurements page
            /// </summary>
            public const string Measurements = "V";
        }
    }
}