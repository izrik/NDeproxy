using System;
using System.Collections.Generic;
using System.IO;

namespace NDeproxy
{
    public class HeaderReader
    {
        static readonly Logger log = new Logger();

        public static HeaderCollection readHeaders(Stream inStream)
        {

            log.debug("reading headers");

            var headers = HeaderCollection.fromStream(inStream);

            foreach (var header in headers)
            {
                log.debug("  {0}: {1}", header.name, header.value);
            }

            if (headers.size() < 1)
            {
                log.debug("no headers received");
            }

            return headers;
        }
    }
}
