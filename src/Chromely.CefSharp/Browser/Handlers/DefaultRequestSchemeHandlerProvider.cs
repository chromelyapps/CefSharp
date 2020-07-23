// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core.Network;
using System;
using System.Collections.Generic;

namespace Chromely.CefSharp.Browser.Handlers
{
    public class DefaultRequestSchemeHandlerProvider : IChromelyRequestSchemeHandlerProvider
    {
        protected IDictionary<string, Tuple<UrlScheme,IResourceHandler>> _handlerMap;

        public DefaultRequestSchemeHandlerProvider()
        {
            _handlerMap = new Dictionary<string, Tuple<UrlScheme, IResourceHandler>>();
        }

        public void Add(UrlScheme urlScheme, IResourceHandler handler)
        {
            if (handler != null)
            {
                string key = Key(urlScheme);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (!_handlerMap.ContainsKey(key))
                    {
                        _handlerMap[key] = new Tuple<UrlScheme, IResourceHandler>(urlScheme, handler);
                    }
                }
            }
        }

        public void Add(string scheme, string host, UrlSchemeType urlSchemeType, IResourceHandler handler)
        {
            if (handler != null)
            {
                string key = Key(scheme, host);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (!_handlerMap.ContainsKey(key))
                    {
                        var urlScheme = new UrlScheme(scheme, host, string.Empty, urlSchemeType);
                        _handlerMap[key] = new Tuple<UrlScheme, IResourceHandler>(urlScheme, handler);
                    }
                }
            }
        }

        public UrlScheme GetScheme(string url)
        {
            string key = Key(url);
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (_handlerMap.ContainsKey(key))
                {
                    return _handlerMap[key].Item1;
                }
            }

            return null;
        }

        public IResourceHandler GetHandler(string url)
        {
            string key = Key(url);
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (_handlerMap.ContainsKey(key))
                {
                    return _handlerMap[key].Item2;
                }
            }

            return null;
        }

        public IResourceHandler GetHandler(UrlScheme urlScheme)
        {
            string key = Key(urlScheme);
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (_handlerMap.ContainsKey(key))
                {
                    return _handlerMap[key].Item2;
                }
            }

            return null;
        }

        public bool IsSchemeRegistered(string url)
        {
            string key = Key(url);
            if (!string.IsNullOrWhiteSpace(key))
            {
                return _handlerMap.ContainsKey(key);
            }

            return false;
        }

        public bool IsSchemeRegistered(string scheme, string host)
        {
            string key = Key(scheme, host);
            if (!string.IsNullOrWhiteSpace(key))
            {
                return _handlerMap.ContainsKey(key);
            }

            return false;
        }

        public bool IsSchemeRegistered(UrlScheme urlScheme)
        {
            string key = Key(urlScheme);
            if (!string.IsNullOrWhiteSpace(key))
            {
                return _handlerMap.ContainsKey(key);
            }

            return false;
        }

        private string Key(UrlScheme scheme)
        {
            if (scheme == null)
                return string.Empty;

            return Key(scheme.Scheme, scheme.Host);
        }

        private string Key(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var uri = new Uri(url);
            return Key(uri.Scheme, uri.Host);
        }

        private string Key(string scheme, string host)
        {
            return $"{scheme}::{host}";
        }
    }
}
