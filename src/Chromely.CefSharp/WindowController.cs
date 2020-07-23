// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using Chromely.CefSharp.Browser;
using Chromely.Core;
using Chromely.Core.Configuration;
using Chromely.Core.Host;
using Chromely.Core.Network;

namespace Chromely.CefSharp
{
    public partial class WindowController : ChromelyWindowController
    {
        protected IChromelyRequestSchemeHandlerProvider _requestSchemeHandlerProvider;

        public WindowController(IChromelyWindow window, 
                                IChromelyNativeHost nativeHost, 
                                IChromelyConfiguration config, 
                                IChromelyRouteProvider routeProvider, 
                                IChromelyRequestTaskRunner requestTaskRunner, 
                                IChromelyCommandTaskRunner commandTaskRunner,
                                IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider,
                                ChromelyHandlersResolver handlersResolver)
            : base(window, nativeHost, config, routeProvider, requestTaskRunner, commandTaskRunner, handlersResolver)
        {
            // WindowController.NativeWindow
            _nativeHost.HostCreated += OnWindowCreated;
            _nativeHost.HostMoving += OnWindowMoving;
            _nativeHost.HostSizeChanged += OnWindowSizeChanged;
            _nativeHost.HostClose += OnWindowClose;

            _requestSchemeHandlerProvider = requestSchemeHandlerProvider;
        }
    }
}
