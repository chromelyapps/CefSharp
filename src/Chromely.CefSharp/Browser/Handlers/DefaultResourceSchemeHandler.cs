// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Chromely.Core;
using Chromely.Core.Network;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp resource scheme handler.
    /// </summary>
    public class DefaultResourceSchemeHandler : ResourceHandler
    {
        private const string STATUSTEXT_OK = "OK";

        protected IChromelyResource _chromelyResource;
        protected IChromelyErrorHandler _chromelyErrorHandler;
        protected FileInfo _fileInfo;

        public DefaultResourceSchemeHandler(IChromelyErrorHandler chromelyErrorHandler)
        {
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
            var uri = new Uri(request.Url);
            var fileName = uri.AbsolutePath;

            var u = new Uri(request.Url);
            var file = u.Authority + u.AbsolutePath;

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
                            byte[] fileBytes = File.ReadAllBytes(file);
                            _chromelyResource.Content = new MemoryStream(fileBytes);

                            string extension = Path.GetExtension(file);
                            _chromelyResource.MimeType = MimeMapper.GetMimeType(extension);
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

        protected virtual void SetResponseInfoOnSuccess()
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
