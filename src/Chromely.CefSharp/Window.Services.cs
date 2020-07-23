// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core.Logging;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Chromely.Core;

namespace Chromely.CefSharp
{
    public partial class Window
    {
        public void RegisterHandlers()
        {
            SetCustomHandlers();
            RegisterJsHandlers();
        }

        protected virtual void SetCustomHandlers()
        {
            if (_handlersResolver == null)
            {
                return;
            }

            // ILifeSpanHandler
            if (LifeSpanHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(ILifeSpanHandler));
                if (handler is ILifeSpanHandler lifeSpanHandler)
                {
                    LifeSpanHandler = lifeSpanHandler;
                }
            }

            // ILoadHandler
            if (LoadHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(ILoadHandler));
                if (handler is ILoadHandler loadHandler)
                {
                    LoadHandler = loadHandler;
                }
            }

            // IRequestHandler
            if (RequestHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IRequestHandler));
                if (handler is IRequestHandler requestHandler)
                {
                    RequestHandler = requestHandler;
                }
            }

            // IDisplayHandler
            if (DisplayHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IDisplayHandler));
                if (handler is IDisplayHandler displayHandler)
                {
                    DisplayHandler = displayHandler;
                }
            }

            // IContextMenuHandler
            if (MenuHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IContextMenuHandler));
                if (handler is IContextMenuHandler contextMenuHandler)
                {
                    MenuHandler = contextMenuHandler;
                }
            }

            // IFocusHandler
            if (FocusHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IFocusHandler));
                if (handler is IFocusHandler focusHandler)
                {
                    FocusHandler = focusHandler;
                }
            }

            // IKeyboardHandler
            if (KeyboardHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IKeyboardHandler));
                if (handler is IKeyboardHandler keyboardHandler)
                {
                    KeyboardHandler = keyboardHandler;
                }
            }

            // IJsDialogHandler
            if (JsDialogHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IJsDialogHandler));
                if ( handler is IJsDialogHandler jsDialogHandler)
                {
                    JsDialogHandler = jsDialogHandler;
                }
            }

            // IDialogHandler
            if (DialogHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IDialogHandler));
                if (handler is IDialogHandler dialogHandler)
                {
                    DialogHandler = dialogHandler;
                }
            }

            // IDragHandler
            if (DragHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IDragHandler));
                if (handler is IDragHandler dragHandler)
                {
                    DragHandler = dragHandler;
                }
            }

            // IDownloadHandler
            if (DownloadHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IDownloadHandler));
                if (handler is IDownloadHandler downloadHandler)
                {
                    DownloadHandler = downloadHandler;
                }
            }

            // IFindHandler
            if (FindHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IFindHandler));
                if (handler is IFindHandler findHandler)
                {
                    FindHandler = findHandler;
                }
            }

            // IResourceRequestHandlerFactory
            if (ResourceRequestHandlerFactory == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IResourceRequestHandlerFactory));
                if (handler is IResourceRequestHandlerFactory resourceRequestHandlerFactory)
                {
                    ResourceRequestHandlerFactory = resourceRequestHandlerFactory;
                }
            }

            // IRenderProcessMessageHandler
            if (RenderProcessMessageHandler == null)
            {
                var handler = _handlersResolver.GetCustomOrDefaultHandler(typeof(IRenderProcessMessageHandler));
                if (handler is IRenderProcessMessageHandler renderProcessMessageHandler)
                {
                    RenderProcessMessageHandler = renderProcessMessageHandler;
                }
            }
        }

        protected virtual void RegisterJsHandlers()
        {
            JavascriptObjectRepository.ResolveObject += (sender, e) =>
            {
                var handlerList = _handlersResolver?.Invoke(typeof(IChromelyJsBindingHandler));
                if (handlerList != null && handlerList.Any())
                {
                    var repo = e.ObjectRepository;
                    foreach (var handlerObj in handlerList)
                    {
                        var handler = handlerObj as IChromelyJsBindingHandler;
                        if (handler == null)
                            continue;

                        if (!string.IsNullOrWhiteSpace(handler.ObjectName))
                        {
                            if (e.ObjectName == handler.ObjectName)
                            {
                                if (handler.BoundObject != null)
                                {
                                    var bindingOptions = handler.BindingOptions as BindingOptions;
                                    bindingOptions = bindingOptions ?? BindingOptions.DefaultBinder;
                                    repo.Register(name: handler.ObjectName, objectToBind: handler.BoundObject, isAsync: true, options: bindingOptions);
                                }
                                else
                                {
                                    Logger.Instance.Log.LogError($"Javascript Binding object: {handler.ObjectName} has no valid object to bind to.");
                                }
                            }
                        }
                    }
                }
            };
        }

        internal void BrowserInitializedChanged(object sender, EventArgs eventArgs)
        {
            if (NativeHost != null)
            {
                ResizeBrowser();
                IsBrowserInitializedChanged -= BrowserInitializedChanged;
            }
        }

        internal void ResizeBrowser()
        {
            if (_browser != null)
            {
                _browserWindowHandle = _browser.GetHost().GetWindowHandle();
                if (_browserWindowHandle != IntPtr.Zero)
                {
                    var size = NativeHost.GetWindowClientSize();
                    ResizeBrowser(size.Width, size.Height);
                }
            }
        }

        internal void ResizeBrowser(int width, int height)
        {
            NativeHost?.ResizeBrowser(_browserWindowHandle, width, height);
        }
    }
}
