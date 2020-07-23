// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using Chromely.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Chromely.CefSharp
{
    public static class BrowserLauncher
    {
        public static void Open(string url)
        {
            try
            {
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    try
                    {
                        // hack because of this: https://github.com/dotnet/corefx/issues/10361
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    catch (Exception exception)
                    {
                        Logger.Instance.Log.LogError(exception, "BrowserLauncher:Open");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Log.LogError(exception, "BrowserLauncher:Open");
            }
        }
    }
}
