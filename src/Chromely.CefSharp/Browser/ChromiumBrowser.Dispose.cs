// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using System;
using System.Threading;

namespace Chromely.CefSharp.Browser
{
    public abstract partial class ChromiumBrowser 
    {
        #region Disposal

        private bool _disposed;

        ~ChromiumBrowser()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Attempt to move the disposeSignaled state from 0 to 1. If successful, we can be assured that
            // this thread is the first thread to do so, and can safely dispose of the object.
            if (Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 0)
            {
                return;
            }

            if (disposing)
            {
                FreeUnmanagedResources();
            }

            InternalDispose(disposing);

            _disposed = true;
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void FreeUnmanagedResources()
        {
            // Repeated if missed
            Cef.Shutdown();

            _browser = null;

            if (BrowserSettings != null)
            {
                BrowserSettings.Dispose();
                BrowserSettings = null;
            }

            if (_managedCefBrowserAdapter != null)
            {
                _managedCefBrowserAdapter.Dispose();
                _managedCefBrowserAdapter = null;
            }
        }

        private void InternalDispose(bool disposing)
        {
            if (disposing)
            {
                CanExecuteJavascriptInMainFrame = false;
                IsBrowserInitialized = false;

                // Don't maintain a reference to event listeners anylonger:
                AddressChanged = null;
                ConsoleMessage = null;
                FrameLoadEnd = null;
                FrameLoadStart = null;
                IsBrowserInitializedChanged = null;
                LoadError = null;
                LoadingStateChanged = null;
                StatusMessage = null;
                TitleChanged = null;
                JavascriptMessageReceived = null;

                // Release reference to handlers, except LifeSpanHandler which is done after Disposing
                // ManagedCefBrowserAdapter otherwise the ILifeSpanHandler.DoClose will not be invoked.
                this.SetHandlersToNullExceptLifeSpan();

                _browser = null;

                if (_managedCefBrowserAdapter != null)
                {
                    _managedCefBrowserAdapter.Dispose();
                    _managedCefBrowserAdapter = null;
                }

                //Dispose of BrowserSettings if we created it, if user created then they're responsible
                if (_browserSettings != null && _browserSettings.FrameworkCreated)
                {
                    _browserSettings.Dispose();
                }

                _browserSettings = null;

                // LifeSpanHandler is set to null after managedCefBrowserAdapter.Dispose so ILifeSpanHandler.DoClose
                // is called.
                LifeSpanHandler = null;
            }

            Cef.RemoveDisposable(this);
        }

        #endregion
    }
}
