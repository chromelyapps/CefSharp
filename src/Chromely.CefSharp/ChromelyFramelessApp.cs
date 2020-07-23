// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using Chromely.CefSharp.NativeHost;
using Chromely.Core.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Chromely.CefSharp
{
    /// <summary>
    /// Simplest Chromely Frameless app implementation.
    /// Be sure to call base implementations on derived implementations.
    /// </summary>
    public class ChromelyFramelessApp : ChromelyAppBase
    {
        public sealed override void ConfigureCoreServices(ServiceCollection services)
        {
            base.ConfigureCoreServices(services);
            services.TryAddSingleton<IChromelyNativeHost, ChromelyFramelessHost>();
        }
    }
}
