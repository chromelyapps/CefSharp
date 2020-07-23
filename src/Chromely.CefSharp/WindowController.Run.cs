// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Reflection;
using CefSharp;
using Chromely.CefSharp.Browser;
using Chromely.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Chromely.CefSharp
{
    public partial class WindowController
    {
        private CefSettings _settings;

        /// <summary>
        /// Runs the application.
        /// This call does not return until the application terminates
        /// or an error is occured.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>   
        /// <returns>
        ///  0 successfully run application - now terminated
        ///  1 on internal exception (see log for more information).
        /// </returns>
        public override int Run(string[] args)
        {
            try
            {
                return RunInternal(args);
            }
            catch (Exception exception)
            {
                Logger.Instance.Log.LogError(exception, "HostBase:Run");
                return 1;
            }
        }

        public override void Quit()
        {
            NativeHost_Quit();
        }

        protected int RunInternal(string[] args)
        {
            // For Windows 7 and above, best to include relevant app.manifest entries as well
            Cef.EnableHighDPISupport();

            var localFolder = _config.AppExeLocation;
            if (string.IsNullOrWhiteSpace(localFolder))
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                localFolder = Path.GetDirectoryName(new Uri(codeBase).LocalPath);
            }
            var localesDirPath = Path.Combine(localFolder ?? throw new InvalidOperationException(), "locales");

            _settings = new CefSettings
            {
                LocalesDirPath = localesDirPath,
                Locale = "en-US",
                RemoteDebuggingPort = 20480,
                MultiThreadedMessageLoop = true,
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                LogSeverity = LogSeverity.Default,
                LogFile = "logs\\chromely.cef_" + DateTime.Now.ToString("yyyyMMdd") + ".log",
            };

            // Update configuration settings
            _settings.Update(_config.CustomSettings);
            _settings.UpdateCommandOptions(_config.CommandLineOptions);
            _settings.UpdateCommandLineArgs(_config.CommandLineArgs);

            // Set DevTools url
            string devtoolsUrl = _config.DevToolsUrl;
            if (string.IsNullOrWhiteSpace(devtoolsUrl))
            {
                _config.DevToolsUrl = $"http://127.0.0.1:{_settings.RemoteDebuggingPort}";
            }
            else
            {
                Uri uri = new Uri(devtoolsUrl);
                if (uri.Port <= 80)
                {
                    _config.DevToolsUrl = $"{devtoolsUrl}:{_settings.RemoteDebuggingPort}";
                }
            }

            RegisterDefaultSchemeHandlers();
            RegisterCustomSchemeHandlers();

            // Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(_settings, true, browserProcessHandler: null);

            _window.RegisterHandlers();
            _window.Init(_settings);

            NativeHost_CreateAndShowWindow();
            NativeHost_Run();

            Cef.Shutdown();

            NativeHost_Quit();
            
            return 0;
        }
    }
}
