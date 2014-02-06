using System;
using System.Collections.Generic;
using System.Threading;

namespace NDeproxy
{
    public class Handlers
    {
        // Handler function.
        // Returns a 200 OK Response, with no additional headers or response body.
        public static Response simpleHandler(Request request, HandlerContext context)
        {
            return new Response(200, "OK");
        }
        // Handler function.
        // Returns a 200 OK Response, with the same headers and body as the request.
        public static Response echoHandler(Request request, HandlerContext context)
        {
            return new Response(200, "OK", request.headers, request.body);
        }
        // Handler creator
        // Returns a closure (handler) that waits for the given amount of time
        // before forawrding the request to another handler.
        // Note: timeout is in milliseconds
        public static HandlerWithContext Delay(int timeout, Handler nextHandler)
        {
            return Delay(timeout, nextHandler.WithContext());
        }

        public static HandlerWithContext Delay(int timeout, HandlerWithContext nextHandler = null)
        {
            if (nextHandler == null)
                nextHandler = Handlers.simpleHandler;

            return (Request request, HandlerContext context) =>
            {
                Thread.Sleep(timeout);
                return nextHandler(request, context);
            };
        }

        public static HandlerWithContext Route(string host, int port, bool https = false,
                                               ClientConnector connector = null)
        {
            if (connector == null)
            {
                connector = new BareClientConnector();
            }

            return (Request request, HandlerContext context) =>
            {

                Request request2 = new Request(
                                       request.method,
                                       request.path,
                                       new HeaderCollection(request.headers),
                                       request.body
                                   );

                if (request2.headers.contains("Host"))
                {
                    request2.headers.deleteAll("Host");
                }

                request2.headers.add("Host", string.Format("{0}:{1}", host, port));

                RequestParams rparams = new RequestParams();
                Response response = connector.sendRequest(request2, https, host, port, rparams);

                return response;
            };
        }
    }
}
