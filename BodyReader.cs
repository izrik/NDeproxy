using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NDeproxy
{
    public class BodyReader
    {
        static readonly Logger log = new Logger("BodyReader");

        public static object readBody(Stream inStream, HeaderCollection headers, bool tryConvertToString=true)
        {

            if (headers == null)
                return null;


            if (headers == null)
                return null;

            byte[] bindata = null;

            /*
           RFC 2616 section 4.4, Message Length
           1. Any response message that must not return a body (1xx, 204,
                304, HEAD) should be terminated by the first empty line
                after the header fields, regardless of entity headers.
           2. If the Transfer-Encoding header is present and has a value
                other than "identity", then it uses chunked encoding.
           3. If Content-Length is present, it specifies both the
                transfer-length and entity-length in octets (these must be
                the same)
           4. multipart/byteranges
           5. server closes the connection, for response bodies
         */


            // # 2
            if (headers.contains("Transfer-Encoding"))
            {
                if (headers["Transfer-Encoding"] == "identity")
                {

                    // ignore Transfer-Encoding. proceed to #3

                }
                else if (headers["Transfer-Encoding"] == "chunked")
                {

                    bindata = readChunkedBody(inStream);

                }
                else
                {

                    // rfc 2616 § 3.6
                    //
                    // A server which receives an entity-body with a transfer-coding it does
                    // not understand SHOULD return 501 (Unimplemented), and close the
                    // connection. A server MUST NOT send transfer-codings to an HTTP/1.0
                    // client.

                    log.error("Non-identity transfer encoding, not yet supported in deproxy.  Unable to read response body.");
                    return null;
                }
            }

            // # 3
            if (bindata == null && headers.contains("Content-Length"))
            {
                int length = int.Parse(headers["Content-Length"]);
                log.debug("Headers contain Content-Length: {0}", length);

                if (length > 0)
                {
                    bindata = new byte[length];
                    int i;
                    int count = 0;
                    log.debug("  starting to read body");
                    for (i = 0; i < length; i++)
                    {
                        int ii = inStream.ReadByte();
                        log.debug("   [{0}] = {1}", i, ii);
                        byte bb = (byte)ii;
                        bindata[i] = bb;
                        count++;
                    }
//            def count = inStream.read(bindata);

                    if (count != length)
                    {
                        // end of stream or some error
                        // TODO: what does the spec say should happen in this case?
                    }
                }
            }

            // # 4 multipart/byteranges

            // else, there is no body (?)

            if (bindata == null)
            {
                log.debug("Returning null");
                return null;
            }

            if (tryConvertToString)
            {
                // TODO: switch this to true, and always try to read chardata unless
                // it"s a known binary content type
                bool tryCharData = false;

                if (!headers.contains("Content-type"))
                {
                    tryCharData = true;
                }
                else
                {
                    string contentType = headers["Content-Type"];

                    if (contentType != null)
                    {
                        contentType = contentType.ToLower();
                        // use startsWith in order to ignore any charset or other
                        // parameters on the header value
                        if (contentType.StartsWith("text/") ||
                        contentType.StartsWith("application/atom+xml") ||
                        contentType.StartsWith("application/ecmascript") ||
                        contentType.StartsWith("application/json") ||
                        contentType.StartsWith("application/javascript") ||
                        contentType.StartsWith("application/rdf+xml") ||
                        contentType.StartsWith("application/rss+xml") ||
                        contentType.StartsWith("application/soap+xml") ||
                        contentType.StartsWith("application/xhtml+xml") ||
                        contentType.StartsWith("application/xml") ||
                        contentType.StartsWith("application/xml-dtd") ||
                        contentType.StartsWith("application/xop+xml") ||
                        contentType.StartsWith("image/svg+xml") ||
                        contentType.StartsWith("message/http") ||
                        contentType.StartsWith("message/imdn+xml"))
                        {

                            tryCharData = true;
                        }
                    }
                }

                if (tryCharData)
                {
                    string chardata = null;

                    try
                    {
                        chardata = Encoding.ASCII.GetString(bindata);
                    }
                    catch (Exception e)
                    {
                    }

                    if (chardata != null)
                    {
                        return chardata;
                    }
                }
            }

            return bindata;
        }

        static int GetIntegerValue(char c, int radix)
        {
            int val = -1;
            if (char.IsDigit(c))
                val = (int)(c - '0');
            else if (char.IsLower(c))
                val = (int)(c - 'a') + 10;
            else if (char.IsUpper(c))
                val = (int)(c - 'A') + 10;
            else
                throw new ArgumentOutOfRangeException("c", "The value is neither a letter nor a digit.");

            if (val >= radix)
                throw new ArgumentOutOfRangeException("c", "The character is outside the set of acceptable values for the chosen radix.");

            return val;
        }

        public static byte[] readChunkedBody(Stream inStream)
        {

            // see rfc 2616, section 3.6.1

            // Chunked-Body   = *chunk
            //                  last-chunk
            //                  trailer
            //                  CRLF
            //
            // chunk          = chunk-size [ chunk-extension ] CRLF
            //                  chunk-data CRLF
            // chunk-size     = 1*HEX
            // last-chunk     = 1*("0") [ chunk-extension ] CRLF
            //
            // chunk-extension= *( ";" chunk-ext-name [ "=" chunk-ext-val ] )
            // chunk-ext-name = token
            // chunk-ext-val  = token | quoted-string
            // chunk-data     = chunk-size(OCTET)
            // trailer        = *(entity-header CRLF)

            var chunks = new List<byte[]>();

            while (true)
            {

                // chunk-size [ chunk-extension ] CRLF
                // 1*("0") [ chunk-extension ] CRLF
                var line = LineReader.readLine(inStream);

                // find the extent of the chunk-size
                int i;
                for (i = 0; i < line.Length; i++)
                {
                    var value = GetIntegerValue(line[i], 16);
                    if (value < 0 || value > 15)
                        break;
                }

                if (i < 1)
                {
                    // started with an invalid character
                    throw new FormatException("Invalid chunk size");
                }

                int length = Convert.ToInt32(line.Substring(0, i), 16);

                // ignore any chunk-extension for now

                // last-chunk = 1*("0") ...
                if (length < 1)
                    break;

                // chunk-data CRLF
                byte[] chunkData = new byte[length];
                inStream.Read(chunkData, 0, length);
                LineReader.readLine(inStream);

                chunks.Add(chunkData);
            }

            var trailer = HeaderReader.readHeaders(inStream);
            // we don't do anything with the trailer yet.
            // according to the rfc, everything in the trailer should be an
            // entity-header. There are also additional requirements


            // merge all the chunks together

            int totalLength = 0;
            foreach (byte[] chunk in chunks)
            {
                totalLength += chunk.Length;
            }

            byte[] buffer = new byte[totalLength];

            int index = 0;
            foreach (byte[] chunk in chunks)
            {
                chunk.CopyTo(buffer, index);
                index += chunk.Length;
            }

            return buffer;
        }
    }
}
