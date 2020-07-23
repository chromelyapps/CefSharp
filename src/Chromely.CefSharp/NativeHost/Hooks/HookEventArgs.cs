// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;

namespace Chromely.CefSharp.NativeHost
{
	public class HookEventArgs : EventArgs
	{
		public int HookCode;    // Hook code
		public IntPtr wParam;   // WPARAM argument
		public IntPtr lParam;   // LPARAM argument
	}
}
