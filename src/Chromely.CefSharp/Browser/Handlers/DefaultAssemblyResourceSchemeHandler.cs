// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp;
using Chromely.Core.Configuration;
using Chromely.Core.Logging;
using Chromely.Core.Network;
using Microsoft.Extensions.Logging;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp resource scheme handler.
    /// </summary>
    public class DefaultAssemblyResourceSchemeHandler : ResourceHandler
    {
        private const string STATUSTEXT_OK = "OK";
        private const string STATUSTEXT_ZEROFILESIZE = "Resource loading error: file size is zero.";
        private const string STATUSTEXT_FILENOTFOUND = "File not found.";
        private const string STATUSTEXT_BADREQUEST = "Resource loading error.";

        protected readonly IChromelyRequestSchemeHandlerProvider _requestSchemeHandlerProvider;
        protected Regex _regex = new Regex("[/]");
        protected Stream _stream;
        protected string _mimeType;

        public DefaultAssemblyResourceSchemeHandler(IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider)
        {
            _requestSchemeHandlerProvider = requestSchemeHandlerProvider;
        }

        /// <summary>
        /// The process request async.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {
            var u = new Uri(request.Url);
            var fileAbsolutePath = u.AbsolutePath;
            var file = u.Authority + fileAbsolutePath;

            var fileInfo = new FileInfo(file);
            // Check if file exists 
            if (!fileInfo.Exists)
            {
                StatusCode = (int)HttpStatusCode.NotFound;
                StatusText = STATUSTEXT_FILENOTFOUND;
                callback.Continue();

                Logger.Instance.Log.LogWarning($"File: {file}: {StatusText}");
            }
            // Check if file exists but empty
            else if (fileInfo.Length == 0)
            {
                StatusCode = (int)HttpStatusCode.BadRequest;
                StatusText = STATUSTEXT_ZEROFILESIZE;
                callback.Continue();

                Logger.Instance.Log.LogWarning($"File: {file}: {StatusText}");
            }
            else 
            {
                Task.Run(() =>
                {
                    using (callback)
                    {
                        _stream = null;
                        _mimeType = "text/html";

                        try
                        {
                            if (!ProcessAssmblyEmbeddedFile(request.Url, file, fileAbsolutePath))
                            {
                                ProcessLocalFile(file);
                            }
                        }
                        catch (Exception exception)
                        {
                            _stream = null;
                            StatusCode = (int)HttpStatusCode.BadRequest;
                            StatusText = STATUSTEXT_BADREQUEST;
                            Logger.Instance.Log.LogError(exception, exception.Message);
                        }

                        if (_stream == null)
                        {
                            callback.Cancel();
                        }
                        else
                        {
                            SetResponseInfoOnSuccess();
                            callback.Continue();
                        }
                    }
                });
            }

            return CefReturnValue.ContinueAsync;
        }


        private bool ProcessLocalFile(string file)
        {
            // Check if file exists and not empty
            var fileInfo = new FileInfo(file);
            if ((fileInfo.Exists) && fileInfo.Length > 0)
            {
                byte[] fileBytes = File.ReadAllBytes(file);
                _stream = new MemoryStream(fileBytes);

                string extension = Path.GetExtension(file);
                _mimeType = MimeMapper.GetMimeType(extension);
            }

            return false;
        }

        private bool ProcessAssmblyEmbeddedFile(string url, string file, string fileAbsolutePath)
        {
            var scheme = _requestSchemeHandlerProvider?.GetScheme(url);
            if (scheme == null ||
                scheme.AssemblyOptions == null ||
                scheme.AssemblyOptions.TargetAssembly == null ||
                scheme.UrlSchemeType != UrlSchemeType.AssemblyResource)
            {
                return false;
            }

            var option = scheme.AssemblyOptions;
            var manifestName = string.Join(".", option.DefaultNamespace, option.RootFolder, _regex.Replace(fileAbsolutePath, ".")).Replace("..", ".").Replace("..", ".");
            _stream = option.TargetAssembly.GetManifestResourceStream(manifestName);
            if (_stream != null && _stream.Length > 0)
            {
                string extension = Path.GetExtension(file);
                _mimeType = MimeMapper.GetMimeType(extension);
                return true;
            }

            return false;
        }

        protected void SetResponseInfoOnSuccess()
        {
            //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
            _stream.Position = 0;
            //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
            ResponseLength = _stream.Length;
            MimeType = _mimeType;
            StatusCode = (int)HttpStatusCode.OK;
            StatusText = STATUSTEXT_OK;
            Stream = _stream;
        }
    }
}
