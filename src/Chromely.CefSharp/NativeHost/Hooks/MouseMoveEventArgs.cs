// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.Drawing;

namespace Chromely.CefSharp.NativeHost
{
    public class MouseMoveEventArgs : EventArgs
    {
        public MouseMoveEventArgs(int xDelta, int yDelta)
        {
            DeltaChangeSize = new Size(xDelta, yDelta);
        }

        public MouseMoveEventArgs(Size deltaSize)
        {
            DeltaChangeSize = deltaSize;
        }

        public Size DeltaChangeSize { get; set; }
     }
}
