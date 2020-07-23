// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp.Internals;

namespace Chromely.CefSharp.Browser
{
    internal static class InternalWebBrowserExtensions
    {
        internal static void SetHandlersToNullExceptLifeSpan(this IWebBrowserInternal browser)
        {
            browser.DialogHandler = null;
            browser.FindHandler = null;
            browser.RequestHandler = null;
            browser.DisplayHandler = null;
            browser.LoadHandler = null;
            browser.KeyboardHandler = null;
            browser.JsDialogHandler = null;
            browser.DragHandler = null;
            browser.DownloadHandler = null;
            browser.MenuHandler = null;
            browser.FocusHandler = null;
            browser.ResourceRequestHandlerFactory = null;
            browser.RenderProcessMessageHandler = null;
        }
    }
}
