// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Text;
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
        private const string STATUSTEXT_OK = "OK";
        private const string STATUSTEXT_ZEROFILESIZE = "Resource loading error: file size is zero.";
        private const string STATUSTEXT_FILENOTFOUND = "File not found.";
        private const string STATUSTEXT_BADREQUEST = "Resource loading error.";

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

            var fileInfo = new FileInfo(file);
            // Check if file exists 
            if (!fileInfo.Exists)
            {
                SetResponseInfoOnFailure((int)HttpStatusCode.NotFound, STATUSTEXT_FILENOTFOUND);
                callback.Continue();

                Logger.Instance.Log.LogWarning($"File: {file}: {StatusText}");
            }
            // Check if file exists but empty
            else if (fileInfo.Length == 0)
            {
                SetResponseInfoOnFailure((int)HttpStatusCode.BadRequest, STATUSTEXT_ZEROFILESIZE);
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
                            byte[] fileBytes = File.ReadAllBytes(file);
                            _stream = new MemoryStream(fileBytes);

                            string extension = Path.GetExtension(file);
                            _mimeType = MimeMapper.GetMimeType(extension);
                        }
                        catch (Exception exception)
                        {
                            SetResponseInfoOnFailure((int)HttpStatusCode.BadRequest, STATUSTEXT_BADREQUEST);
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
            StatusText = STATUSTEXT_OK;
            Stream = _stream;
        }

        protected void SetResponseInfoOnFailure(int status, string statusText)
        {
            _stream = GetMemoryStream(statusText, Encoding.UTF8);
            StatusCode = status;
            StatusText = statusText;
        }
    }
}
