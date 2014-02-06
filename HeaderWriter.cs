using System;
using System.Collections.Generic;
using System.IO;

namespace NDeproxy
{
    public class HeaderWriter
    {
        static readonly Logger log = new Logger("HeaderWriter");

        public static void writeHeaders(Stream outStream, HeaderCollection headers)
        {

            var writer = new StreamWriter(outStream);

            log.debug("Sending headers");
            foreach (Header header in headers.getItems())
            {
                writer.Write("{0}: {1}", header.name, header.value);
                writer.Write("\r\n");
                log.debug("  \"{0}: {1}\"", header.name, header.value);
            }

            writer.Write("\r\n");
            writer.Flush();
        }
    }
}
