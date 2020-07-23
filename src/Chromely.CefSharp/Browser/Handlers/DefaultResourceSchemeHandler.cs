// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CefSharp;
using Chromely.Core.Logging;
using Chromely.Core.Network;
using Microsoft.Extensions.Logging;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp resource scheme handler.
    /// </summary>
    public class DefaultResourceSchemeHandler : ResourceHandler
    {
        protected Stream _stream;
        protected string _mimeType;

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

            // Check if file exists and not empty
            var fileInfo = new FileInfo(file);
            if ((fileInfo.Exists) && fileInfo.Length > 0)
            {
                Task.Run(() =>
                {
                    using (callback)
                    {
                        _stream = null;
                        _mimeType = "text/html";

                        try
                        {
                            byte[] fileBytes = File.ReadAllBytes(file);
                            _stream = new MemoryStream(fileBytes);

                            string extension = Path.GetExtension(file);
                            _mimeType = MimeMapper.GetMimeType(extension);
                        }
                        catch (Exception exception)
                        {
                            _stream = null;
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

        protected virtual void SetResponseInfoOnSuccess()
        {
            //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
            _stream.Position = 0;
            //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
            ResponseLength = _stream.Length;
            MimeType = _mimeType;
            StatusCode = (int)HttpStatusCode.OK;
            Stream = _stream;
        }
    }
}
