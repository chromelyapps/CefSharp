// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using Chromely.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp http scheme handler.
    /// </summary>
    public abstract class DefaultExternalRequestSchemeHandlerBase : ResourceHandler
    {
        protected bool _disposed;
        protected Stream _dataStream;
        protected CancellationTokenSource _cancellationTokenSource;
        protected byte[] _rentedBuffer;

        public DefaultExternalRequestSchemeHandlerBase()
        {
        }

        /// <summary>
        /// Finalizes object
        /// </summary>
        ~DefaultExternalRequestSchemeHandlerBase()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public new void Dispose()
        {
            Dispose(true);
            base.Dispose();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            //breaking disposable pattern, because i'm not sure, if this method is used right
            //so i'm freening all resources without taking account disposing parameter

            //if (disposing)
            //{
            //here should be freed only managed resources
            //}
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            FreeResources();

            // Free any unmanaged objects here.
            //

            _disposed = true;
        }

        /// <summary>
        /// Prepares cef web request. 
        /// </summary>
        /// <returns>If request should be handled, returns true, otherwize false.</returns>
        protected abstract bool PrepareRequest(IRequest request);

        /// <summary>
        /// Called async after Request processed. Should load data required to populate headers and later data.
        /// </summary>
        /// <returns>Returns true, if data available, otherwize false.</returns>
        protected abstract Task<bool> LoadResourceData(CancellationToken cancellationToken);

        /// <summary>
        /// If data stream available returns it, otherwize returns null
        /// </summary>
        protected abstract Task<Stream> GetResourceDataStream(CancellationToken cancellationToken);

        /// <summary>
        /// Sets response info on request success
        /// </summary>
        protected abstract void SetResponseInfoOnSuccess();

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
            if (!this.PrepareRequest(request))
            {
                return CefReturnValue.Cancel;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _dataStream = null;

            Task.Run(async () =>
            {
                using (callback)
                {
                    try
                    {
                        if (await LoadResourceData(_cancellationTokenSource.Token))
                        {
                            _dataStream = await GetResourceDataStream(_cancellationTokenSource.Token);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.Instance.Log.LogInformation("The request was canceled.");
                    }
                    catch (Exception ex) when (ex.Message == "The request was aborted: The request was canceled.")
                    {
                        Logger.Instance.Log.LogInformation("The request was canceled.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log.LogError(ex, "Exception thrown while loading resource");
                    }

                    if (_dataStream == null)
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

            return CefReturnValue.ContinueAsync;
        }

        private void FreeBuffer()
        {
            if (_rentedBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_rentedBuffer);
                _rentedBuffer = null;
            }
        }

        private void FreeResources()
        {
            FreeBuffer();
            _dataStream?.Dispose();
            _dataStream = null;
        }
    }
}
