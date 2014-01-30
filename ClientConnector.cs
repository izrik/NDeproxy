using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public interface ClientConnector
    {
        Response sendRequest(Request request, bool https, string host, int? port, RequestParams rparams);
    }
}
