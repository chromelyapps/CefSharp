// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using System.Collections.Generic;

namespace Chromely.CefSharp.Browser
{
    public static class RequestExtension
    {
        public static string GetPostData(this IRequest request)
        {
            var elements = request?.PostData?.Elements;
            if (elements == null || (elements.Count == 0))
            {
                return string.Empty;
            }

            var dataElement = elements[0];
            return dataElement.GetBody();
        }

        public static IList<IPostDataElement> GetPostDataElements(this IRequest request)
        {
            return request?.PostData?.Elements;
        }
    }
}
