using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NDeproxy
{
    public class DefaultClientConnector : BareClientConnector
    {
        public DefaultClientConnector()
        : this(null)
        {
        }

        public DefaultClientConnector(Socket socket)
        : base(socket)
        {
        }

        Response sendRequest(Request request, bool https, string host, int port, RequestParams rparams)
        {

            if (rparams.sendDefaultRequestHeaders)
            {

                if (request.body != null)
                {

                    if (rparams.usedChunkedTransferEncoding)
                    {

                        if (!request.headers.contains("Transfer-Encoding"))
                        {
                            request.headers.add("Transfer-Encoding", "chunked");
                        }

                    }
                    else if (!request.headers.contains("Transfer-Encoding") ||
                           request.headers["Transfer-Encoding"] == "identity")
                    {

                        int length;
                        string contentType;
                        if (request.body is string)
                        {
                            length = (request.body as string).Length;
                            contentType = "text/plain";
                        }
                        else if (request.body is byte[])
                        {
                            length = (request.body as byte[]).Length;
                            contentType = "application/octet-stream";
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown data type in requestBody");
                        }

                        if (length > 0)
                        {
                            if (!request.headers.contains("Content-Length"))
                            {
                                request.headers.add("Content-Length", length);
                            }
                            if (!request.headers.contains("Content-Type"))
                            {
                                request.headers.add("Content-Type", contentType);
                            }
                        }
                    }
                }

                if (!request.headers.contains("Host"))
                {
                    int? port2 = port;
                    if ((port == 80 && !https) ||
                        (port == 443 && https))
                    {
                        port2 = null;
                    }
                    request.headers.add("Host", HostHeader.CreateHostHeaderValueNoCheck(host, port2));
                }
                if (!request.headers.contains("Accept"))
                {
                    request.headers.add("Accept", "*/*");
                }
                if (!request.headers.contains("Accept-Encoding"))
                {
                    request.headers.add("Accept-Encoding", "identity");
                }
                if (!request.headers.contains("User-Agent"))
                {
                    request.headers.add("User-Agent", Deproxy.VERSION_STRING);
                }
            }

            return base.sendRequest(request, https, host, port, rparams);
        }
    }
}
