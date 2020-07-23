// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core.Configuration;
using Chromely.Core.Network;

namespace Chromely.CefSharp.Browser
{
    public class DefaultLifeSpanHandler : ILifeSpanHandler
    {
        protected readonly IChromelyConfiguration _config;
        protected readonly IChromelyCommandTaskRunner _commandTaskRunner;

        public DefaultLifeSpanHandler(IChromelyConfiguration config, IChromelyCommandTaskRunner commandTaskRunner)
        {
            _config = config;
            _commandTaskRunner = commandTaskRunner;
        }

        bool ILifeSpanHandler.OnBeforePopup(IWebBrowser ChromiumBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            //NOTE: DevTools popups DO NOT trigger OnBeforePopup.

            //Set newBrowser to null unless your attempting to host the popup in a new instance of ChromiumBrowser
            //This option is typically used in WPF. This example demos using IWindowInfo.SetAsChild
            //Older branches likely still have an example of this method if you choose to go down that path.
            newBrowser = null;

            var isUrlExternal = _config?.UrlSchemes?.IsUrlRegisteredExternalBrowserScheme(targetUrl);
            if (isUrlExternal.HasValue && isUrlExternal.Value)
            {
                BrowserLauncher.Open(targetUrl);
                return true;
            }

            var isUrlCommand = _config?.UrlSchemes?.IsUrlRegisteredCommandScheme(targetUrl);
            if (isUrlCommand.HasValue && isUrlCommand.Value)
            {
                _commandTaskRunner.RunAsync(targetUrl);
                return true;
            }

            return false;
        }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser ChromiumBrowser, IBrowser browser)
        {
        }

        bool ILifeSpanHandler.DoClose(IWebBrowser ChromiumBrowser, IBrowser browser)
        {
            //The default CEF behaviour (return false) will send a OS close notification (e.g. WM_CLOSE).
            //See the doc for this method for full details.
            return false;
        }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser ChromiumBrowser, IBrowser browser)
        {
        }
    }
}
