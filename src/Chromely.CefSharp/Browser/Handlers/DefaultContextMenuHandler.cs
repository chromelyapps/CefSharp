// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core.Configuration;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp context menu handler.
    /// </summary>
    public class DefaultContextMenuHandler : IContextMenuHandler
    {
        protected readonly IChromelyConfiguration _config;
        protected const int ShowDevTools = 26501;
        protected const int CloseDevTools = 26502;
        protected readonly bool debugging;

        public DefaultContextMenuHandler(IChromelyConfiguration config)
        {
            _config = config;
            debugging = _config.DebuggingMode;
        }

        /// <summary>
        /// The on before context menu.
        /// </summary>
        /// <param name="browserControl">
        /// The browser control.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        void IContextMenuHandler.OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
             // To disable the menu then call clear
             model.Clear();

            // Removing existing menu item
            // Remove "View Source" option
            model.Remove(CefMenuCommand.ViewSource);

            if (debugging)
            {
                // Add new custom menu items
                model.AddItem((CefMenuCommand)ShowDevTools, "Show DevTools");
                model.AddItem((CefMenuCommand)CloseDevTools, "Close DevTools");
            }
        }

        /// <summary>
        /// The on context menu command.
        /// </summary>
        /// <param name="browserControl">
        /// The browser control.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="commandId">
        /// The command id.
        /// </param>
        /// <param name="eventFlags">
        /// The event flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IContextMenuHandler.OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {

            if (debugging)
            {
                if ((int)commandId == ShowDevTools)
                {
                    browser.ShowDevTools();
                }

                if ((int)commandId == CloseDevTools)
                {
                    browser.CloseDevTools();
                }
            }

            return false;
        }

        /// <summary>
        /// The on context menu dismissed.
        /// </summary>
        /// <param name="browserControl">
        /// The browser control.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="frame">
        /// The frame.
        /// </param>
        void IContextMenuHandler.OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
        }

        /// <summary>
        /// The run context menu.
        /// </summary>
        /// <param name="browserControl">
        /// The browser control.
        /// </param>
        /// <param name="browser">
        /// The browser.
        /// </param>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IContextMenuHandler.RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}