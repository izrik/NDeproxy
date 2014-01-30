using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

namespace NDeproxy
{
    public class BareClientConnector : ClientConnector
    {
        readonly Logger log = new Logger();

        public BareClientConnector()
            : this(null)
        {
        }

        public BareClientConnector(Socket socket)
        {
            this.socket = socket;
        }

        Socket socket;

        public Response sendRequest(Request request, bool https, string host, int? port, RequestParams rparams)
        {
            //"""Send the given request to the host and return the Response."""
            log.debug("sending request: https={0}, host={1}, port={2}", https, host, port);

            if (port == null)
            {
                port = (https ? 443 : 80);
            }

            var ipAddresses = Dns.GetHostAddresses(host);

            string requestLine = string.Format("{0} {1} HTTP/1.1", request.method, request.path ?? "/");

            log.debug("creating socket: host={0}, port={1}", host, port);
            Socket s;
            if (this.socket != null)
            {
                s = this.socket;
            }
//            else if (https)
//            {
//                s = SSLSocketFactory.getDefault().createSocket(host, port);
//            }
            else
            {
                s = SocketHelper.Client(host, port.Value);
            }

            Stream outStream = new NetworkStream(s);
            var writer = new StreamWriter(outStream, Encoding.ASCII);

            writer.Write(requestLine);
            writer.Write("\r\n");
            log.debug("Sending \"{0}\"", requestLine);

            writer.Flush();

            HeaderWriter.writeHeaders(outStream, request.headers);

            writer.Flush();
            outStream.Flush();

            BodyWriter.writeBody(request.body, outStream, rparams.usedChunkedTransferEncoding);

            Stream inStream = new NetworkStream(s);

            log.debug("reading response line");
            string responseLine = LineReader.readLine(inStream);
            log.debug("response read: {0}", responseLine);

            var words = responseLine.Split(new [] { ' ', '\t' }, 3);
            if (words.Length != 3)
            {
                throw new InvalidOperationException();
            }

            var proto = words[0];
            var code = words[1];
            var message = words[2];

            HeaderCollection headers = HeaderReader.readHeaders(inStream);

            log.debug("reading body");
            var body = BodyReader.readBody(inStream, headers);

            log.debug("creating response object");
            var response = new Response(code, message, headers, body);

            log.debug("returning response object");
            return response;
        }
    }
}
