// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// Loads external HTTP request resources like proxy.
    /// Omits X-Frame-Options headers and adds Access-Control-Allow-Origin: * header
    /// </summary>
    public class DefaultExternalRequestSchemeHandler : DefaultExternalRequestSchemeHandlerBase
    {
        protected readonly HttpClient _httpClient;
        protected HttpRequestMessage _httpRequest;
        protected HttpResponseMessage _httpResponseMessage;
        protected long _responseLenght;

        /// <summary>
        /// Initializes a new instance of the Chromely.CefGlue.Browser.Handlers.ExternalRequestSchemeHandler class.
        /// </summary>
        public DefaultExternalRequestSchemeHandler()
        {
            _httpClient = new HttpClient(
                                new HttpClientHandler
                                {
                                    AllowAutoRedirect = false,
                                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                                }, true);
            _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Converts http request method name into object.
        /// </summary>
        protected virtual HttpMethod GetHttpMethod(string methodName)
        {
            switch (methodName.ToUpper())
            {
                case "GET":
                    return HttpMethod.Get;
                case "PUT":
                    return HttpMethod.Put;
                case "POST":
                    return HttpMethod.Post;
                case "DELETE":
                    return HttpMethod.Delete;
                case "HEAD":
                    return HttpMethod.Head;
                case "OPTIONS":
                    return HttpMethod.Options;
                case "TRACE":
                    return HttpMethod.Trace;
            }

            throw new ArgumentException($"Unknown http method: {methodName}");
        }

        /// <summary>
        /// Builds http request message from cef request
        /// </summary>
        /// <param name="request"></param>
        protected virtual HttpRequestMessage BuildHttpRequest(IRequest request)
        {
            var httpRequest = new HttpRequestMessage(GetHttpMethod(request.Method), request.Url);
            var cefHeaders = request.Headers;
            foreach (var key in cefHeaders.AllKeys)
                httpRequest.Headers.TryAddWithoutValidation(key, cefHeaders.GetValues(key));

            var dataElements = request.GetPostDataElements();
            if (dataElements != null && dataElements.Any())
            {
                httpRequest.Content = new StreamContent(new PostDataStream(dataElements.ToArray()));
            }

            return httpRequest;
        }

        /// <inheritdoc/>
        protected override bool PrepareRequest(IRequest request)
        {
            _httpRequest = BuildHttpRequest(request);
            return true;
        }

        /// <inheritdoc/>
        protected override async Task<bool> LoadResourceData(CancellationToken cancellationToken)
        {
            _httpResponseMessage = await _httpClient.SendAsync(_httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return _httpResponseMessage.Content != null;
        }
        /// <inheritdoc/>
        protected override Task<Stream> GetResourceDataStream(CancellationToken cancellationToken)
        {
            return _httpResponseMessage.Content.ReadAsStreamAsync();
        }

        /// <inheritdoc/>
        protected override void SetResponseInfoOnSuccess()
        {
            if (_httpResponseMessage == null)
            {
                StatusCode = (int)HttpStatusCode.BadRequest;
                StatusText = "An error occurred";
                return;
            }

            //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
            _dataStream.Position = 0;
            //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
            ResponseLength = _dataStream.Length;
            Stream = _dataStream;

            MimeType = _httpResponseMessage.Content?.Headers?.ContentType?.MediaType;
            StatusCode = (int)_httpResponseMessage.StatusCode;
            StatusText = _httpResponseMessage.ReasonPhrase;

            // Process headers
            if (Headers != null)
            {
                foreach (var header in _httpResponseMessage.Headers)
                {
                    if (header.Key == "X-Frame-Options")
                        continue;
                    foreach (var val in header.Value)
                        Headers.Add(header.Key, val);
                }

                if (_httpResponseMessage.Content != null)
                {
                    foreach (var header in _httpResponseMessage.Content.Headers)
                    {
                        foreach (var val in header.Value)
                            Headers.Add(header.Key, val);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _httpRequest?.Dispose();
            _httpResponseMessage?.Dispose();

            _httpRequest = null;
            _httpResponseMessage = null;

            base.Dispose(disposing);
        }
    }
}
