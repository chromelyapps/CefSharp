// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;
using Chromely.CefSharp.Browser;
using Chromely.Core;
using Chromely.Core.Infrastructure;
using Chromely.Core.Network;

namespace Chromely.CefSharp
{
    public partial class WindowController
    {
        protected virtual void RegisterDefaultSchemeHandlers()
        {
            if (_handlersResolver == null)
                return;

            IDictionary<UrlSchemeType, Type> urlTypesMapper = new Dictionary<UrlSchemeType, Type>();
            urlTypesMapper.Add(UrlSchemeType.Resource, typeof(IDefaultResourceCustomHandler));
            urlTypesMapper.Add(UrlSchemeType.AssemblyResource, typeof(IDefaultAssemblyResourceCustomHandler));

            foreach (var urlType in urlTypesMapper)
            {
                var handler = _handlersResolver.GetDefaultHandler(typeof(ISchemeHandlerFactory), urlType.Value);

                if (handler is ISchemeHandlerFactory schemeHandler)
                {
                    // Register default resource handlers
                    var schemes = _config?.UrlSchemes.GetSchemesByType(urlType.Key);
                    if (schemes != null && schemes.Any())
                    {
                        foreach (var item in schemes)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Name))
                            {
                                _requestSchemeHandlerProvider.Add(item, null);

                                _settings.RegisterScheme(new CefCustomScheme
                                {
                                    SchemeName = item.Scheme,
                                    DomainName = item.Host,
                                    SchemeHandlerFactory = schemeHandler,
                                    IsCorsEnabled = true,
                                    IsSecure = true
                                });
                            }
                        }
                    }
                }
            }

            urlTypesMapper = new Dictionary<UrlSchemeType, Type>();
            urlTypesMapper.Add(UrlSchemeType.LocalRquest, typeof(IDefaultRequestCustomHandler));
            urlTypesMapper.Add(UrlSchemeType.ExternalRequest, typeof(IDefaultExernalRequestCustomHandler));

            foreach (var urlType in urlTypesMapper)
            {
                var handler = _handlersResolver.GetDefaultHandler(typeof(IResourceHandler), urlType.Value);

                if (handler is IResourceHandler requestHandler)
                {
                    // Register default local resource handlers
                    var schemes = _config?.UrlSchemes.GetSchemesByType(urlType.Key);
                    if (schemes != null && schemes.Any())
                    {
                        foreach (var item in schemes)
                        {
                            _requestSchemeHandlerProvider.Add(item, requestHandler);
                        }
                    }
                }
            }
        }

        protected virtual void RegisterCustomSchemeHandlers()
        {
            // Register custom request handlers
            var schemeHandlerList = _handlersResolver?.Invoke(typeof(IChromelySchemeHandler));
            if (schemeHandlerList != null && schemeHandlerList.Any())
            {
                foreach (var schemeHandlerObj in schemeHandlerList)
                {
                    var schemehandler = schemeHandlerObj as IChromelySchemeHandler;
                    if (schemehandler == null ||
                        schemehandler.Scheme == null ||
                        string.IsNullOrWhiteSpace(schemehandler.Scheme.Scheme) ||
                        string.IsNullOrWhiteSpace(schemehandler.Scheme.Host))
                        continue;

                    if (schemehandler.Handler is IResourceHandler handler)
                    {
                        _requestSchemeHandlerProvider.Add(schemehandler.Scheme, handler);
                    }

                    if (schemehandler.HandlerFactory is ISchemeHandlerFactory handlerFactory)
                    {
                        _settings.RegisterScheme(new CefCustomScheme
                        {
                            SchemeName = schemehandler.Scheme.Scheme,
                            DomainName = schemehandler.Scheme.Host,
                            SchemeHandlerFactory = handlerFactory,
                            IsCorsEnabled = schemehandler.IsCorsEnabled,
                            IsSecure = schemehandler.IsSecure
                        });
                    }
                }
            }
        }
    }
}
