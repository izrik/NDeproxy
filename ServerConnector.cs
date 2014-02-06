using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public interface ServerConnector
    {
        void shutdown();
    }
    public delegate ServerConnector ServerConnectorFactory(Endpoint endpoint, string name);
}
