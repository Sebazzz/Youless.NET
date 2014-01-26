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
        /// Represents the current day as parameter to <see cref="GetDayMeasurements(int,System.Threading.CancellationToken)"/>
        /// </summary>
        public const int CurrentDay = 0;

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
        /// <returns></returns>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetLastHourMeasurement() {
            return await GetLastHourMeasurement(CancellationToken.None);
        }

        /// <summary>
        /// Gets the measurements from the last hour - these are the most accurate (60 seconds interval)
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetLastHourMeasurement(CancellationToken ct) {
            Task<YoulessUsageData> data = GetMeasurements(new Tuple<string, int>("h", 1), ct);
            Task<YoulessUsageData> data2 = GetMeasurements(new Tuple<string, int>("h", 2), ct);

            return (await data).MergeWith(await data2);
        }

        /// <summary>
        /// Gets the daily measurements (from now until the last 24 hours). The measurements are returned in 10 minute intervals.
        /// </summary>
        /// <returns></returns>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetDailyMeasurements() {
            return await GetDailyMeasurements(CancellationToken.None);
        }

        /// <summary>
        /// Gets the daily measurements (from now until the last 24 hours). The measurements are returned in 10 minute intervals.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetDailyMeasurements(CancellationToken ct) {
            Task<YoulessUsageData>[] dataTasks = {
                                                     GetHourMeasurements(8, ct),
                                                     GetHourMeasurements(16, ct),
                                                     GetHourMeasurements(24, ct)
                                                 };

            return (await dataTasks[0]).MergeWith(await dataTasks[1]).MergeWith(await dataTasks[2]);
        }

        /// <summary>
        /// Gets the measurements of the specified hour (either now to 8 hours back, 8 to 16 hours back, or 16 to 24 hours back)
        /// </summary>
        /// <param name="numberOfHours">Either 8, 16, or 24, specifying measurements for either 0-8 hours, 8-16 hours, or 16-24 hours</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Occurs when <paramref name="numberOfHours"/> is not 8, 16, or 24</exception>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetHourMeasurements(int numberOfHours, CancellationToken ct) {
            if (numberOfHours%8 != 0) {
                throw new ArgumentException("Argument must either be 8, 16, or 24.", "numberOfHours");
            }
            if (numberOfHours < 8 || numberOfHours > 24) {
                throw new ArgumentOutOfRangeException("numberOfHours", "Argument must either be 8, 16, or 24.");
            }

            int param = numberOfHours/8;
            return await GetMeasurements(new Tuple<string, int>("w", param), ct);
        }


        /// <summary>
        /// Gets the measurements of the specified day, up to 6 days back
        /// </summary>
        /// <param name="day">An argument ranging from 0 (<see cref="CurrentDay"/>) through 6</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the <paramref name="day"/> is below 0 or higher than 6</exception>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetDayMeasurements(int day) {
            return await GetDayMeasurements(day, CancellationToken.None);
        }

        /// <summary>
        /// Gets the measurements of the specified day, up to 6 days back
        /// </summary>
        /// <param name="day">An argument ranging from 0 (<see cref="CurrentDay"/>) through 6</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the <paramref name="day"/> is below 0 or higher than 6</exception>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetDayMeasurements(int day, CancellationToken ct) {
            if (day < 0 || day > 6) {
                throw new ArgumentOutOfRangeException("day", "Argument must be in range [0 ... 6]");
            }

            return await GetMeasurements(new Tuple<string, int>("d", day), ct);
        }

        /// <summary>
        /// Gets the measurements of the specified month in the year. The value returned will specify the starting point.
        /// </summary>
        /// <param name="month">A value in the range of [1 ... 12] specifying the month</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the <paramref name="month"/> is below 1 or higher than 12</exception>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
        public async Task<YoulessUsageData> GetMonthMeasurements(int month, CancellationToken ct) {
            if (month < 1 || month > 12) {
                throw new ArgumentOutOfRangeException("month", "Argument must be in range [1 ... 12]");
            }

            return await GetMeasurements(new Tuple<string, int>("m", month), ct);
        }

        /// <summary>
        /// Gets the measurements from the Youless using the specified parameters
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <include file='YoulessServiceClient.xml' path='/doc/common/webapi-exceptions/exception'/>
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