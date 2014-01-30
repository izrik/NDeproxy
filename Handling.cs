using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class Handling
    {
        public readonly Endpoint endpoint;
        public readonly Request request;
        public readonly Response response;
        public readonly string connection;

        public Handling(Endpoint endpoint, Request request, Response response, string connection)
        {
            this.endpoint = endpoint;
            this.request = request;
            this.response = response;
            this.connection = connection;
        }

        public override string ToString()
        {
            return string.Format("Handling(endpoint={0}, request={1}, response={2}, connection={3})",
                endpoint,
                request,
                response,
                connection);
        }
    }
}
