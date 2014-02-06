using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class Request
    {
        public readonly string method;
        public readonly string path;
        public readonly HeaderCollection headers;
        public readonly object body;

        /**
     *
     * Creates a Request object
     *
     * @param method The HTTP method to use, such as "GET", "POST", or "PUT".
     * @param path The path of the resource requested, without host info,
     * e.g. "/path/to/resource"
     * @param headers An optional collection of name/value pairs, either a
     * map, like "["name": "value"]", or a HeaderCollection. Defaults to an
     * empty map.
     * @param body An optional request body. Defaults to the empty string.
     * Both strings and byte arrays are acceptable. All other types are
     * ToString"d.
     */
//        public Request(string method, string path, object headers = null, string body = null)
//            : this(method, path, headers, (object)body)
//        {
//        }
//        public Request(string method, string path, object headers = null, byte[] body = null)
//            : this(method, path, headers, (object)body)
//        {
//        }
        public Request(string method, string path, object headers = null, object body = null)
        {

            if (body == null)
            {
                body = "";
            }
            else if (!(body is byte[]) &&
                     !(body is string))
            {
                body = body.ToString();
            }

            this.method = method;
            this.path = path;
            this.headers = new HeaderCollection(headers);
            this.body = body;
        }

        public override string ToString()
        {
            return string.Format("Request(method={0}, path={1}, headers={2}, body={3})", method, path, headers, body);
        }
    }
}
