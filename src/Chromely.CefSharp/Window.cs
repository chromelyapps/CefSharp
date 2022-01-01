// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using Chromely.CefSharp.Browser;
using Chromely.Core;
using Chromely.Core.Configuration;
using Chromely.Core.Host;
using System;
using static Chromely.Interop.User32;

namespace Chromely.CefSharp
{
    public partial class Window : ChromiumBrowser, IChromelyWindow
    {
        protected ChromelyHandlersResolver _handlersResolver;

        public Window(IChromelyNativeHost nativeHost, 
                      IChromelyConfiguration config,
                      ChromelyHandlersResolver handlersResolver)
            : base(nativeHost, config)
        {
            _handlersResolver = handlersResolver;

            IsBrowserInitializedChanged += BrowserInitializedChanged;
        }

        public IntPtr Handle 
        {
            get
            {
                if (NativeHost != null)
                {
                    return NativeHost.Handle;
                }

                return IntPtr.Zero;
            }
        }

        public virtual void Init(object settings)
        {
            Initialize(settings as CefSettings);
        }
        public virtual void Create(IntPtr hostHandle, IntPtr winXID)
        {
            CreateBrowser(hostHandle);
        }

        public virtual void SetTitle(string title)
        {
            NativeHost?.SetWindowTitle(title);
        }

        public virtual void NotifyOnMove()
        {
            NotifyMoveOrResize();
        }

        public virtual void Resize(int width, int height)
        {
            ResizeBrowser(width,  height);
        }

        public virtual void Minimize()
        {
            if (Handle != IntPtr.Zero)
            {
                if (_config.WindowOptions.WindowFrameless)
                {
                    _config.WindowOptions.WindowState = WindowState.Minimize;
                }

                ShowWindow(Handle, SW.SHOWMINIMIZED);
            }
        }
        public virtual void Maximize()
        {
            if (Handle != IntPtr.Zero)
            {
                if (_config.WindowOptions.WindowFrameless)
                {
                    _config.WindowOptions.WindowState = WindowState.Maximize;
                }

                ShowWindow(Handle, SW.SHOWMAXIMIZED);
            }
        }
        public virtual void Restore()
        {
            if (Handle != IntPtr.Zero)
            {
                if (_config.WindowOptions.WindowFrameless)
                {
                    _config.WindowOptions.WindowState = WindowState.Normal;
                }

                ShowWindow(Handle, SW.RESTORE);
            }
        }

        public virtual void Close()
        {
            if (Handle != IntPtr.Zero)
            {
                SendMessageW(Handle, WM.CLOSE);
            }
        }
    }
}
