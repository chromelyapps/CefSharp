// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using CefSharp.Handler;
using Chromely.Core.Configuration;
using Chromely.Core.Network;

namespace Chromely.CefSharp.Browser
{
    public class DefaultRequestHandler : RequestHandler
    {
        public static readonly string VersionNumberString = $"Chromium: {Cef.ChromiumVersion}, CEF: {Cef.CefVersion}, CefSharp: {Cef.CefSharpVersion}";

        protected readonly IChromelyConfiguration _config;
        protected readonly IChromelyRequestSchemeHandlerProvider _requestSchemeHandlerProvider;
        protected readonly IResourceRequestHandler _resourceRequestHandler;
        protected readonly IChromelyCommandTaskRunner _commandTaskRunner;

        public DefaultRequestHandler(IChromelyConfiguration config,
                                     IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider,
                                     IResourceRequestHandler resourceRequestHandler,
                                     IChromelyCommandTaskRunner commandTaskRunner)
        {
            _config = config;
            _requestSchemeHandlerProvider = requestSchemeHandlerProvider;
            _resourceRequestHandler = resourceRequestHandler;
            _commandTaskRunner = commandTaskRunner;
        }

        protected override bool OnBeforeBrowse(IWebBrowser ChromiumBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            var isUrlExternal = _config?.UrlSchemes?.IsUrlRegisteredExternalBrowserScheme(request.Url);
            if (isUrlExternal.HasValue && isUrlExternal.Value)
            {
                BrowserLauncher.Open(request.Url);
                return true;
            }

            var isUrlCommand = _config?.UrlSchemes?.IsUrlRegisteredCommandScheme(request.Url);
            if (isUrlCommand.HasValue && isUrlCommand.Value)
            {
                _commandTaskRunner.RunAsync(request.Url);
                return true;
            }

            return false;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser ChromiumBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (_requestSchemeHandlerProvider.IsSchemeRegistered(request.Url)) 
            {
                return _resourceRequestHandler;
            }

            return null;
        }
    }

    public class DefaultResourceRequestHandler : ResourceRequestHandler
    {
        protected readonly IChromelyRequestSchemeHandlerProvider _requestSchemeHandlerProvider;

        public DefaultResourceRequestHandler(IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider)
        {
            _requestSchemeHandlerProvider = requestSchemeHandlerProvider;
        }

        protected override IResourceHandler GetResourceHandler(IWebBrowser ChromiumBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return _requestSchemeHandlerProvider.GetHandler(request.Url);
        }
    }
}
