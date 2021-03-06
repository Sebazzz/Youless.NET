﻿namespace YoulessNet.Internal {
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an <see cref="HttpClient"/> tailored to calling the Youless API
    /// </summary>
    internal sealed class YoulessHttpClient : HttpClient {
        private readonly HttpClientHandler _handler;
        private ICredentials _credentials;
        private bool _hasAuthenticated;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Http.HttpClient"/> class.
        /// </summary>
        private YoulessHttpClient(Uri baseUri, ICredentials credentials, HttpClientHandler requestHandler)
            : base(requestHandler) {
            this._credentials = credentials;
            this.BaseAddress = baseUri;
            this._handler = requestHandler;
        }

        /// <summary>
        /// Creates an <see cref="YoulessHttpClient"/> instance
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Handler object is owned by instance")]
        public static YoulessHttpClient Create(Uri baseUri, ICredentials credentials) {
            // create a suitable HTTP handler
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.AllowAutoRedirect = false;
            handler.UseCookies = true;

            return new YoulessHttpClient(baseUri, credentials, handler);
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>.The task object representing the asynchronous operation.
        /// </returns>
        /// <param name="request">The HTTP request message to send.</param><param name="cancellationToken">The cancellation token to cancel operation.</param><exception cref="T:System.InvalidOperationException">The request message was already sent by the <see cref="T:System.Net.Http.HttpClient"/> instance.</exception>
        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            HttpResponseMessage msg;

            if (this._hasAuthenticated) {
                Debug.WriteLine("YoulessHttpClient: Executing authenticated '{1}' request to {0}", request.RequestUri, request.Method);

                // execute request
                msg = await base.SendAsync(request, cancellationToken);

                if (msg.StatusCode == HttpStatusCode.Forbidden) {
                    Debug.WriteLine("YoulessHttpClient: Authentication required. Re-requesting, first performing authentication.");

                    // sometimes we need to re-authenticate
                    this._hasAuthenticated = false;
                    msg.Dispose();
                } else {
                    EnsureRequestSuccess(msg);
                    return msg;
                }
            }
            
            // authenticate, if required
            if (!_hasAuthenticated) {
                Debug.WriteLine("YoulessHttpClient: Executing unauthenticated '{1}' request to {0}", request.RequestUri, request.Method);
                
                string password = this.GetPassword(request.RequestUri);

                if (!String.IsNullOrEmpty(password)) {
                    await AuthenticateAsync(request.RequestUri, password, cancellationToken);
                    this._hasAuthenticated = true;
                }

                return await base.SendAsync(request, cancellationToken);
            }


            // execute request
            msg = await base.SendAsync(request, cancellationToken);
            EnsureRequestSuccess(msg);

            return msg;
        }

        /// <summary>
        /// Ensures the request is an success, otherwise throws an <see cref="YoulessException"/>
        /// </summary>
        /// <param name="msg"></param>
        private static void EnsureRequestSuccess(HttpResponseMessage msg) {
            try {
                msg.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) {
                msg.Dispose();

                if (msg.StatusCode == HttpStatusCode.NotFound) {
                    throw new NotSupportedException("Method is not supported", ex);
                }

                throw new YoulessException("Unknown no-success error while executing request", ex);
            }
        }

        /// <summary>
        /// Executes authentication to the Youless API in an async manner
        /// </summary>
        /// <param name="sourceUri"></param>
        /// <param name="password"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task AuthenticateAsync(Uri sourceUri, string password, CancellationToken cancellationToken) {
            YoulessAuthenticationInformation authInfo =
                await YoulessAuthenticator.AuthenticateAsync(sourceUri.Host, sourceUri.Port, password, cancellationToken);

            // set cookie
            UriBuilder rootUri = new UriBuilder(
                sourceUri.Scheme, sourceUri.Host, sourceUri.Port);

            this._handler.CookieContainer.Add(
                rootUri.Uri,
                new Cookie(authInfo.CookieName, authInfo.CookieValue));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpClient"/> and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            this._credentials = null;
        }

        private string GetPassword(Uri url) {
            if (this._credentials == null) {
                return null;
            }

            NetworkCredential cred = this._credentials.GetCredential(url, null);
            if (cred != null) {
                return cred.Password;
            }

            return null;
        }

        /// <summary>
        /// Executes the remote method asynchronously
        /// </summary>
        /// <param name="compose"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Ownership is taken by SendAsync method")]
        public Task<HttpResponseMessage> ExecuteMethodAsync(Uri compose, CancellationToken cancellationToken) {
            Uri finalUri = new Uri(this.BaseAddress, compose);

            return this.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, finalUri), cancellationToken);
        }
    }
}