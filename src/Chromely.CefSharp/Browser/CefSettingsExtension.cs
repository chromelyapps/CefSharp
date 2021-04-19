// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The cef settings extension.
    /// </summary>
    internal static class CefSettingsExtension
    {
        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="cefSettings">
        /// The cef settings.
        /// </param>
        /// <param name="customSettings">
        /// The custom settings.
        /// </param>
        public static void Update(this CefSettings cefSettings, IDictionary<string, string> customSettings)
        {
            if ((cefSettings == null) ||
                (customSettings == null) ||
                (customSettings.Count == 0))
            {
                return;
            }

            foreach (var setting in customSettings)
            {
                bool boolResult;
                int intResult;

                if (string.IsNullOrWhiteSpace(setting.Value))
                {
                    continue;
                }

                switch (setting.Key.ToUpper())
                {
                    case CefSettingKeys.NOSANDBOX:
                        break;

                    case CefSettingKeys.BROWSERSUBPROCESSPATH:
                        cefSettings.BrowserSubprocessPath = setting.Value;
                        break;

                    case CefSettingKeys.MULTITHREADEDMESSAGELOOP:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.MultiThreadedMessageLoop = boolResult;
                        }

                        break;

                    case CefSettingKeys.EXTERNALMESSAGEPUMP:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.ExternalMessagePump = boolResult;
                        }

                        break;

                    case CefSettingKeys.WINDOWLESSRENDERINGENABLED:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.WindowlessRenderingEnabled = boolResult;
                        }

                        break;

                    case CefSettingKeys.COMMANDLINEARGSDISABLED:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.CommandLineArgsDisabled = boolResult;
                        }

                        break;

                    case CefSettingKeys.CACHEPATH:
                        cefSettings.CachePath = setting.Value;
                        break;

                    case CefSettingKeys.USERDATAPATH:
                        cefSettings.UserDataPath = setting.Value;
                        break;

                    case CefSettingKeys.PERSISTSESSIONCOOKIES:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.PersistSessionCookies = boolResult;
                        }

                        break;

                    case CefSettingKeys.PERSISTUSERPREFERENCES:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.PersistUserPreferences = boolResult;
                        }

                        break;

                    case CefSettingKeys.USERAGENT:
                        cefSettings.UserAgent = setting.Value;
                        break;

                    case CefSettingKeys.PRODUCTVERSION:
                        cefSettings.ProductVersion = setting.Value;
                        break;

                    case CefSettingKeys.LOCALE:
                        cefSettings.Locale = setting.Value;
                        break;

                    case CefSettingKeys.CEFLOGFILE:
                    case CefSettingKeys.LOGFILE:
                        cefSettings.LogFile = setting.Value;
                        break;

                    case CefSettingKeys.LOGSEVERITY:
                        switch (setting.Value.ToUpper())
                        {
                            case LogSeverityOption.DEFAULT:
                                cefSettings.LogSeverity = LogSeverity.Default;
                                break;
                            case LogSeverityOption.VERBOSE:
                                cefSettings.LogSeverity = LogSeverity.Verbose;
                                break;
                            case LogSeverityOption.INFO:
                                cefSettings.LogSeverity = LogSeverity.Info;
                                break;
                            case LogSeverityOption.ERROR:
                                cefSettings.LogSeverity = LogSeverity.Warning;
                                break;
                            case LogSeverityOption.EXTERNAL:
                                cefSettings.LogSeverity = LogSeverity.Error;
                                break;
                            case LogSeverityOption.FATAL:
                                cefSettings.LogSeverity = LogSeverity.Fatal;
                                break;
                            case LogSeverityOption.DISABLE:
                                cefSettings.LogSeverity = LogSeverity.Disable;
                                break;
                        }

                        break;

                    case CefSettingKeys.JAVASCRIPTFLAGS:
                        cefSettings.JavascriptFlags = setting.Value;
                        break;

                    case CefSettingKeys.RESOURCESDIRPATH:
                        cefSettings.ResourcesDirPath = setting.Value;
                        break;

                    case CefSettingKeys.LOCALESDIRPATH:
                        cefSettings.LocalesDirPath = setting.Value;
                        break;

                    case CefSettingKeys.PACKLOADINGDISABLED:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.PackLoadingDisabled = boolResult;
                        }

                        break;

                    case CefSettingKeys.REMOTEDEBUGGINGPORT:
                        if (setting.Value.TryParseInteger(out intResult))
                        {
                            cefSettings.RemoteDebuggingPort = intResult;
                        }

                        break;

                    case CefSettingKeys.UNCAUGHTEXCEPTIONSTACKSIZE:
                        if (setting.Value.TryParseInteger(out intResult))
                        {
                            cefSettings.UncaughtExceptionStackSize = intResult;
                        }

                        break;

                    case CefSettingKeys.IGNORECERTIFICATEERRORS:
                        if (setting.Value.TryParseBoolean(out boolResult))
                        {
                            cefSettings.IgnoreCertificateErrors = boolResult;
                        }

                        break;

                    case CefSettingKeys.ACCEPTLANGUAGELIST:
                        cefSettings.AcceptLanguageList = setting.Value;
                        break;

                    // Not supported by CefSharp
                    case CefSettingKeys.FOCUSEDNODECHANGEDENABLED:
                        break;
                }
            }
        }

        /// <summary>
        /// The update command line args.
        /// </summary>
        /// <param name="cefSettings">
        /// The cef settings.
        /// </param>
        /// <param name="commandLineOptions">
        /// The command line options.
        /// </param>
        public static void UpdateCommandOptions(this CefSettings cefSettings, List<string> commandLineOptions)
        {
            if ((cefSettings == null) ||
                (commandLineOptions == null) ||
                (cefSettings.CefCommandLineArgs == null))
            {
                return;
            }

            foreach (var commandOpt in commandLineOptions)
            {
                cefSettings.CefCommandLineArgs.Add(commandOpt);
            }
        }

        /// <summary>
        /// The update command line args.
        /// </summary>
        /// <param name="cefSettings">
        /// The cef settings.
        /// </param>
        /// <param name="commandLineArgs">
        /// The command line args.
        /// </param>
        public static void UpdateCommandLineArgs(this CefSettings cefSettings, IDictionary<string, string> commandLineArgs)
        {
            if ((cefSettings == null) ||
                (commandLineArgs == null) ||
                (cefSettings.CefCommandLineArgs == null))
            {
                return;
            }

            foreach (var commandArg in commandLineArgs)
            {
                cefSettings.CefCommandLineArgs.Add(commandArg.Key, commandArg.Value);
            }
        }
    }
}
