// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp resource scheme handler factory.
    /// </summary>
    public class DefaultResourceSchemeHandlerFactory : ISchemeHandlerFactory, IDefaultCustomHandler
    {
        protected readonly IChromelyErrorHandler _chromelyErrorHandler;

        public DefaultResourceSchemeHandlerFactory(IChromelyErrorHandler chromelyErrorHandler)
        {
            _chromelyErrorHandler = chromelyErrorHandler;
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <param name="schemeName">
        /// The scheme name.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="IResourceHandler"/>.
        /// </returns>
        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            return new DefaultResourceSchemeHandler(_chromelyErrorHandler);
        }
    }
}
