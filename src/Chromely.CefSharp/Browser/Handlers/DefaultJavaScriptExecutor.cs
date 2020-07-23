// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using CefSharp;
using Chromely.Core;
using Chromely.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Chromely.CefSharp.Browser
{
    public class DefaultJavaScriptExecutor : IChromelyJavaScriptExecutor
    {
        /// <summary>
        /// The browser.
        /// </summary>
        private readonly IBrowser _browser;

        /// <summary>
        /// Gets the browser.
        /// </summary>
        public DefaultJavaScriptExecutor(IBrowser browser)
        {
            _browser = browser;
        }

        public object ExecuteScript(string frameName, string script)
        {
            var frame = _browser?.GetFrame(frameName);
            if (frame == null)
            {
                Logger.Instance.Log.LogWarning($"Frame {frameName} does not exist.");
                return null;
            }

            var javascriptResponse = frame.EvaluateScriptAsync(script);
            javascriptResponse.Wait();
            return javascriptResponse.Result;
        }

        public object ExecuteScript(string script)
        {
            var frame = _browser?.MainFrame;
            if (frame == null)
            {
                Logger.Instance.Log.LogWarning("Cannot accces main frame.");
                return null;
            }

            var javascriptResponse = frame.EvaluateScriptAsync(script);
            javascriptResponse.Wait();
            return javascriptResponse.Result;
        }

        public object GetBrowser()
        {
            return _browser;
        }

        public object GetMainFrame()
        {
            return _browser?.MainFrame;
        }

        public object GetFrame(string name)
        {
            return _browser?.GetFrame(name);
        }

        public List<long> GetFrameIdentifiers => _browser?.GetFrameIdentifiers()?.ToList();

        public List<string> GetFrameNames => _browser?.GetFrameNames()?.ToList();
    }
}
