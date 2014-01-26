namespace YoulessNet.Internal {
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    /// <summary>
    /// Static helper class for first-time authentication to the Youless
    /// </summary>
    /// <remarks>
    /// This is the only non-PCL compatible implementation in the library. This class performs manual
    /// authentication by manually sending the correct HTTP request. The Youless API returns a malformed
    /// HTTP response when authentication is succesful, and the <see cref="WebRequest"/> underlying the
    /// <see cref="YoulessHttpClient"/> cannot handle that. Known issue: this class does not respect the 
    /// proxy settings of the system, so non-transparent proxies will not work correctly.
    /// </remarks>
    internal static class YoulessAuthenticator {
        private static readonly IFormatProvider HttpFormatProvider = CultureInfo.InvariantCulture;

        /// <summary>
        /// Performs authentication to the Youless API in an asynchronous manner
        /// </summary>
        /// <param name="hostName">Specifies the host name or IP address to connect to</param>
        /// <param name="port">Specifies the port to access</param>
        /// <param name="password">Specifies the password to use</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="YoulessAuthenticationException">Authentication information provided was considered invalid</exception>
        /// <exception cref="YoulessException">Unknown error occurred while connecting to the Youless</exception>
        public static async Task<YoulessAuthenticationInformation> AuthenticateAsync(string hostName, int port, string password, CancellationToken cancellationToken) {
            using (StreamSocket socket = new StreamSocket()) {
                // try to connect first
                try {
                    await socket.ConnectAsync(new HostName(hostName), port.ToString(HttpFormatProvider));
                } catch (Exception ex) {
                    throw new YoulessException("Could not connect to the Youless and perform authentication", ex);
                }

                cancellationToken.ThrowIfCancellationRequested();

                // send our data
                string response;
                try {
                    response = await SendAuthenticationInformation(hostName, password, socket.OutputStream, socket.InputStream, cancellationToken);
                }catch (Exception ex) {
                    throw new YoulessException("Unknown error occurred while reading response from Youless or writing request to Youless", ex);
                }

                // parse response
                return ParseResponseHeaders(response);
            }
        }

        /// <summary>
        /// Parses the response header returns from the Youless
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static YoulessAuthenticationInformation ParseResponseHeaders(string response) {
            string[] responseHeaders = response.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            int ptr = 0;
            while (ptr < responseHeaders.Length) {
                string header = responseHeaders[ptr];

                // check status header
                if (header.Contains("HTTP")) {
                    if (!(header.Contains("200") || header.Contains("302") || header.Contains("307"))) {
                        throw new YoulessAuthenticationException("Could not perform authentication: Header returns '" + header +
                                                                 "'");
                    }
                }

                // check cookie header
                if (header.Contains("Set-Cookie")) {
                    string cookieKeyValuePair = header.Substring("Set-Cookie".Length + 1);
                    string[] cookieKeyValue = cookieKeyValuePair.Split(new[] {'='}, 2);

                    if (cookieKeyValue.Length != 2) {
                        throw new YoulessException("Unexpected cookie response '" + cookieKeyValuePair + "' from Youless");
                    }

                    return new YoulessAuthenticationInformation {
                                                                    CookieName = cookieKeyValue[0].Trim(' '),
                                                                    CookieValue =
                                                                        Uri.UnescapeDataString(cookieKeyValue[1].Trim(' '))
                                                                };
                }

                ptr++;
            }

            throw new YoulessAuthenticationException("Youless did not provide authentication information");
        }


        private static async Task<string> SendAuthenticationInformation(string host, string password, IOutputStream requestStream, IInputStream responseStream, CancellationToken ct) {
            // initialize header
            using (DataWriter dw = new DataWriter(requestStream)) {
                WriteHeader(dw, GetRequestHeader(password));
                WriteHeader(dw, GetHostHeader(host));
                WriteHeader(dw, "Connection: close");
                WriteHeader(dw, null);

                await dw.FlushAsync().AsTask(ct);
                await dw.StoreAsync().AsTask(ct);

#if WINDOWS_PHONE
                // read back the response
                using (DataReader dr = new DataReader(responseStream)) {
                    uint read = await dr.LoadAsync(1024);

                    return dr.ReadString(read);
                }
#else
                // read back the response
                using (StreamReader sr = new StreamReader(responseStream.AsStreamForRead(), Encoding.UTF8)) {
                    return await sr.ReadToEndAsync();
                }
#endif
            }
        }

        private static string GetHostHeader(string hostName) {
            return String.Format(HttpFormatProvider, "Host: {0}", hostName);
        }

        private static string GetRequestHeader(string password) {
            // construct URI first
            string path = "/L?w="+Uri.EscapeDataString(password);

            return String.Format(HttpFormatProvider, "GET {0} HTTP/1.1", path);
        }

        private static void WriteHeader(DataWriter dw, string header) {
            if (header != null) {
                dw.WriteString(header);
            }

            dw.WriteString("\r\n");
        }
    }

    /// <summary>
    /// Authentication information for the Youless
    /// </summary>
    internal struct YoulessAuthenticationInformation {
        public string CookieName { get; set; }
        public string CookieValue { get; set; }
    }
}
