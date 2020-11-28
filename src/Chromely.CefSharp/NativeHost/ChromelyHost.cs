// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

namespace Chromely.CefSharp.NativeHost
{
    public class ChromelyHost : NativeHostBase
    {
        public ChromelyHost(IKeyboadHookHandler keyboadHandler = null) : base(keyboadHandler)
        {
        }
    }
}