// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CefSharp;
using CefSharp.Enums;
using Chromely.Core.Configuration;

namespace Chromely.CefSharp.Browser
{
    public class DefaultDragHandler : IDragHandler
    {
        protected static readonly object objLock = new object();
        protected readonly IChromelyConfiguration _config;

        public DefaultDragHandler(IChromelyConfiguration config)
        {
            _config = config;
        }

        bool IDragHandler.OnDragEnter(IWebBrowser ChromiumBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask)
        {
            return false;
        }

        /*
            <html>
            <head>
                <title>Draggable Regions Test</title>
                <style>
                    .titlebar {
                        -webkit-app-region: drag;
                        -webkit-user-select: none;
                        position: absolute;
                        top: 0px;
                        left: 50px;
                        width: 100%;
                        height: 32px;
                    }

                    .titlebar-button {
                        -webkit-app-region: no-drag;
                        position: absolute;
                        top: 0px;
                        width: 140px;
                        height: 32px;
                    }
                </style>
            </head>
            <body bgcolor="white">
                Draggable regions can be defined using the -webkit-app-region CSS property.
                <br />In the below example the red region is draggable and the blue sub-region is non-draggable.
                <br />Windows can be resized by default and closed using JavaScript <a href="#" onClick="window.close(); return false;">window.close()</a>.
                <div class="titlebar">
                    <div class="titlebar-button"></div>
                </div>
            </body>
            </html>
         */
        void IDragHandler.OnDraggableRegionsChanged(IWebBrowser ChromiumBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions)
        {
            var framelessOption = _config?.WindowOptions?.FramelessOption;
            if (framelessOption == null || !framelessOption.UseWebkitAppRegions)
            {
                return;
            }

            if (!browser.IsPopup)
            {
                lock (objLock)
                {
                    framelessOption.IsDraggable = (nativeHost, point) =>
                    {
                        var scale = nativeHost.GetWindowDpiScale();
                        point.X = (int)(point.X / scale);
                        point.Y = (int)(point.Y / scale);

                        var hitNoDrag = regions.Any(r => !r.Draggable && ContainsPoint(r, point));
                        if (hitNoDrag)
                        {
                            return false;
                        }

                        return regions.Any(r => r.Draggable && ContainsPoint(r, point));
                    };
                }
            }
        }

        private bool ContainsPoint(DraggableRegion region, Point point)
        {
            return point.X >= region.X && point.X <= (region.X + region.Width)
                && point.Y >= region.Y && point.Y <= (region.Y + region.Height);
        }
    }
}