using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class MessageChain
    {
        public Request sentRequest;
        public Response receivedResponse;
        public HandlerWithContext defaultHandler;
        public Dictionary<Endpoint, HandlerWithContext> handlers = new Dictionary<Endpoint, HandlerWithContext>();
        public List<Handling> handlings = new List<Handling>();
        public List<Handling> orphanedHandlings = new List<Handling>();
        protected object _lock = new Object();

        public MessageChain(Handler defaultHandler,
                            Dictionary<Endpoint, HandlerWithContext> handlers = null)
            : this(defaultHandler.WithContext(), handlers)
        {
        }

        public MessageChain(HandlerWithContext defaultHandler = null,
                            Dictionary<Endpoint, HandlerWithContext> handlers = null)
        {
            this.defaultHandler = defaultHandler;
            this.handlers = handlers;
        }

        public void addHandling(Handling handling)
        {
            lock (this._lock)
            {
                this.handlings.Add(handling);
            }
        }

        public void addOrphanedHandling(Handling handling)
        {
            lock (this._lock)
            {
                this.orphanedHandlings.Add(handling);
            }
        }

        public override string ToString()
        {
            return string.Format("MessageChain(default_handler={0} sent_request={1}, handlings={2}, received_response={3}, orphaned_handlings={4})",
                defaultHandler, sentRequest, handlings, receivedResponse, orphanedHandlings);
        }
    }
}
