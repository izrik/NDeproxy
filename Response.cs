using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class Response
    {
        public string code;
        public string message;
        public HeaderCollection headers;
        public object body;

        /**
     *
     * Creates a Response object
     *
     * @param code A numerical status code. This doesn"t have to be a valid
     * HTTP status code; for example, values >= 600 are acceptable also, as
     * are non-numbers.
     * @param message An optional message to go along with the status code. If
     * null, a suitable default will be provided based on the given status
     * code. If code is not a valid HTTP status code, then the default is
     * the empty string.
     * @param headers An optional collection of name/value pairs, either a
     * map, like "["name": "value"]", or a HeaderCollection. Defaults to an
     * empty map.
     * @param body An optional request body. Defaults to the empty string.
     * Both strings and byte arrays are acceptable. All other types are
     * ToString"d.
     */
        public Response(int code, string message = null, object headers = null, object body = null)
            : this(code.ToString(), message, headers, body)
        {
        }
        public Response(string code, string message = null, object headers = null, object body = null)
        {

            code = code.ToString();

            if (message == null)
            {
                if (HttpResponseMessage.messagesByResponseCode.ContainsKey(code))
                {
                    message = HttpResponseMessage.messagesByResponseCode[code];
                }
                else
                {
                    message = "";
                }
            }

            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            if (body == null)
            {
                body = "";
            }
            else if (!(body is byte[]) &&
                   !(body is string))
            {
                body = body.ToString();
            }

            this.code = code;
            this.message = message;
            this.headers = new HeaderCollection(headers);
            this.body = body;
        }

        public override string ToString()
        {
            return string.Format("Response(code={0}, message={1}, headers={2}, body={3})", code, message, headers, body);
        }
    }
}
