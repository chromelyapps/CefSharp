// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core.Network;

namespace Chromely.CefSharp.Browser
{
    public interface IChromelyRequestSchemeHandlerProvider
    {
        void Add(UrlScheme urlScheme, IResourceHandler handler);
        void Add(string scheme, string host, UrlSchemeType urlSchemeType, IResourceHandler handler);
        UrlScheme GetScheme(string url);
        IResourceHandler GetHandler(string url);
        IResourceHandler GetHandler(UrlScheme urlScheme);
        bool IsSchemeRegistered(string url);
        bool IsSchemeRegistered(string scheme, string host);
        bool IsSchemeRegistered(UrlScheme urlScheme);
    }
}
