// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.CefSharp.Browser;
using Chromely.CefSharp.Browser.Handlers;
using Chromely.Core;
using Chromely.Core.Defaults;
using Chromely.Core.Host;
using Chromely.Core.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Chromely.CefSharp
{
    public abstract class ChromelyAppBase : ChromelyApp
    {
        public override void ConfigureCoreServices(IServiceCollection services)
        {
            base.ConfigureCoreServices(services);

            // Add window core services if not already added.
            // Expected window core services are -
            // IChromelyNativeHost, IChromelyWindow

            services.TryAddSingleton<IChromelyInfo, ChromelyInfo>();
            services.TryAddSingleton<IChromelyRouteProvider, DefaultRouteProvider>();
            services.TryAddSingleton<IChromelyRequestTaskRunner, DefaultRequestTaskRunner>();
            services.TryAddSingleton<IChromelyCommandTaskRunner, DefaultCommandTaskRunner>();

            services.TryAddSingleton<IChromelyWindow, Window>();
            services.TryAddSingleton<ChromelyWindowController, WindowController>();
        }

        public sealed override void ConfigureDefaultHandlers(IServiceCollection services)
        {
            base.ConfigureDefaultHandlers(services);

            services.AddSingleton<IChromelyJsBindingHandler, ChromelyBoundObjectHandler>();

            // Add default resource/request handlers
            services.AddSingleton<IChromelyRequestSchemeHandlerProvider, DefaultRequestSchemeHandlerProvider>();
            services.AddSingleton<IResourceRequestHandler, DefaultResourceRequestHandler>();

            services.AddSingleton<ISchemeHandlerFactory, ChromelyResourceSchemeHandlerFactory>();
            services.AddSingleton<ISchemeHandlerFactory, ChromelyAssemblyResourceSchemeHandlerFactory>();
            services.AddSingleton<IResourceHandler, ChromelyRequestSchemeHandler>();
            services.AddSingleton<IResourceHandler, ChromelyExternalRequestSchemeHandler>();

            // Adde default custom handlers
            services.AddSingleton<ILifeSpanHandler, ChromelyLifeSpanHandler>();
            services.AddSingleton<IRequestHandler, ChromelyRequestHandler>();
            services.AddSingleton<IContextMenuHandler, ChromelyContextMenuHandler>();
            services.AddSingleton<IDragHandler, ChromelyDragHandler>();
            services.AddSingleton<IDownloadHandler, ChromelyDownloadHandler>();
        }
    }
}
