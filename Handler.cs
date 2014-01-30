using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public delegate Response Handler(Request request);
    public delegate Response HandlerWithContext(Request request, HandlerContext context);

    public static class HandlerHelper
    {
        public static HandlerWithContext WithContext(this Handler handler)
        {
            return ((Request request, HandlerContext context) => handler(request));
        }
    }
}
