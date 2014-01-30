using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NDeproxy
{
    public class BodyWriter
    {
        static readonly Logger log = new Logger();

        public static void writeBody(object body, Stream outStream, bool chunked = false)
        {

            outStream.Flush();

            if (body != null)
            {
                if (chunked)
                {

                    writeBodyChunked(body, outStream);

                }
                else if (body is string)
                {
                    var sbody = (string)body;
                    log.debug("sending string body, length {0}", sbody.Length);
                    log.debug(sbody);
                    if (sbody.Length > 0)
                    {
                        var writer = new StreamWriter(outStream, Encoding.ASCII, 0);
                        writer.Write(sbody);
                        writer.Flush();
                    }

                }
                else if (body is byte[])
                {
                    var bbody = (byte[])body;
                    log.debug("sending binary body, length {0}", bbody.Length);
                    log.debug(bbody.ToString());
                    if (bbody.Length > 0)
                    {
                        outStream.Write(bbody, 0, bbody.Length);
                        outStream.Flush();
                    }

                }
                else
                {
                    throw new NotSupportedException("Unknown data type in message body");
                }

            }
            else
            {
                log.debug("No body to send");
            }

            outStream.Flush();
        }

        public static void writeBodyChunked(object body, Stream outStream)
        {

            // see rfc 2616, section 3.6.1

            byte[] buffer;
            if (body is string)
            {
                buffer = Encoding.ASCII.GetBytes(body as string);
            }
            else if (body is byte[])
            {
                buffer = body as byte[];
            }
            else
            {
                throw new NotSupportedException("Unknown data type in message body");
            }

            var writer = new StreamWriter(outStream);
            const int maxChunkDataSize = 4096; // in octets
            var bytes = new byte[maxChunkDataSize];
            int i = 0;

            // *chunk
            while (i < buffer.Length)
            {
                int nbytes = Math.Min(buffer.Length - i, maxChunkDataSize);
                // chunk-size
                writer.Write("{0:X}", nbytes);
                // [ chunk-extension ] will go here in the future
                // CRLF
                writer.Write("\r\n");
                writer.Flush();

                // chunk-data
                outStream.Write(buffer, i, nbytes);
                outStream.Flush();

                // CRLF
                writer.Write("\r\n");
                writer.Flush();
                i += nbytes;
            }

            // last-chunk
            // 1*("0")
            writer.Write("0");
            // [ chunk-extension ] will go here in the future
            // CRLF
            writer.Write("\r\n");
            writer.Flush();

            // trailer will go here in the future

            // CRLF
            writer.Write("\r\n");
            writer.Flush();
        }
    }
}
