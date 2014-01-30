using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class HandlerContext
    {
        //    bool requestWasChunked;
        public bool usedChunkedTransferEncoding = false;
        public bool sendDefaultResponseHeaders = true;
    }
}
