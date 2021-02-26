// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp;
using Chromely.Core;
using Chromely.Core.Network;

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
        protected IChromelyResource _chromelyResource;
        protected readonly IChromelyErrorHandler _chromelyErrorHandler;
        protected FileInfo _fileInfo;
        protected Regex _regex = new Regex("[/]");

        public DefaultAssemblyResourceSchemeHandler(IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider, IChromelyErrorHandler chromelyErrorHandler)
        {
            _requestSchemeHandlerProvider = requestSchemeHandlerProvider;
            _chromelyResource = new ChromelyResource();
            _chromelyErrorHandler = chromelyErrorHandler;
            _fileInfo = null;
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

            _fileInfo = new FileInfo(file);
            // Check if file exists 
            if (!_fileInfo.Exists)
            {
                _chromelyResource = _chromelyErrorHandler.HandleError(_fileInfo);
                callback.Continue();
            }
            // Check if file exists but empty
            else if (_fileInfo.Length == 0)
            {
                _chromelyResource = _chromelyErrorHandler.HandleError(_fileInfo);
                callback.Continue();
            }
            else 
            {
                Task.Run(() =>
                {
                    using (callback)
                    {
                        _chromelyResource.Content = null;
                        _chromelyResource.MimeType = "text/html";

                        try
                        {
                            if (!ProcessAssmblyEmbeddedFile(request.Url, file, fileAbsolutePath))
                            {
                                ProcessLocalFile(file);
                            }
                        }
                        catch (Exception exception)
                        {
                            _chromelyResource = _chromelyErrorHandler.HandleError(_fileInfo, exception);
                        }

                        if (_chromelyResource.Content == null)
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
                _chromelyResource.Content = new MemoryStream(fileBytes);

                string extension = Path.GetExtension(file);
                _chromelyResource.MimeType = MimeMapper.GetMimeType(extension);
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
            var stream = option.TargetAssembly.GetManifestResourceStream(manifestName);
            if (stream != null && stream.Length > 0)
            {
                stream.CopyTo(_chromelyResource.Content);
                string extension = Path.GetExtension(file);
                _chromelyResource.MimeType = MimeMapper.GetMimeType(extension);
                return true;
            }

            return false;
        }

        protected void SetResponseInfoOnSuccess()
        {
            //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
            _chromelyResource.Content.Position = 0;
            //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
            ResponseLength = _chromelyResource.Content.Length;
            MimeType = _chromelyResource.MimeType;
            StatusCode = (int)HttpStatusCode.OK;
            StatusText = STATUSTEXT_OK;
            Stream = _chromelyResource.Content;
        }
    }
}
