// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using Chromely.Core.Host;
using Chromely.Core.Infrastructure;

namespace Chromely.CefSharp
{
    public partial class WindowController
    {
        protected IntPtr _nativeHandle;

        #region

        protected virtual void OnWindowCreated(object sender, CreatedEventArgs createdEventArgs)
        {
            if (createdEventArgs != null)
            {
                _nativeHandle = createdEventArgs.Window;
                _window?.Create(_nativeHandle, createdEventArgs.WinXID);
            }
        }

        protected virtual void OnWindowMoving(object sender, MovingEventArgs movingEventArgs)
        {
            _window?.NotifyOnMove();
        }

        protected virtual void OnWindowSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (sizeChangedEventArgs != null)
            {
                _window?.Resize(sizeChangedEventArgs.Width, sizeChangedEventArgs.Height);
            }
        }

        protected void OnWindowClose(object sender, CloseEventArgs closeChangedEventArgs)
        {
            ChromelyAppUser.App.Properties.Save(_config);
        }

        #endregion

        protected void NativeHost_CreateAndShowWindow()
        {
            _nativeHost?.CreateWindow(_config.WindowOptions, _config.DebuggingMode);
        }

        protected void NativeHost_Run()
        {
            _nativeHost?.Run();
        }

        protected void NativeHost_Quit()
        {
            _nativeHost?.Exit();
        }
    }
}
