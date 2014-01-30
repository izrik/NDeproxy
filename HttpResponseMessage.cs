using System;
using System.Collections.Generic;

namespace NDeproxy
{
    public class HttpResponseMessage
    {
        /**
     * Table mapping response codes to messages
     * See RFC 2616.
     */
        public static Dictionary<string, string> messagesByResponseCode = new Dictionary<string, string>{
            { "100", "Continue" },
            { "101", "Switching Protocols" },
            { "200", "OK" },
            { "201", "Created" },
            { "202", "Accepted" },
            { "203", "Non-Authoritative Information" },
            { "204", "No Content" },
            { "205", "Reset Content" },
            { "206", "Partial Content" },
            { "300", "Multiple Choices" },
            { "301", "Moved Permanently" },
            { "302", "Found" },
            { "303", "See Other" },
            { "304", "Not Modified" },
            { "305", "Use Proxy" },
            { "307", "Temporary Redirect" },
            { "400", "Bad Request" },
            { "401", "Unauthorized" },
            { "402", "Payment Required" },
            { "403", "Forbidden" },
            { "404", "Not Found" },
            { "405", "Method Not Allowed" },
            { "406", "Not Acceptable" },
            { "407", "Proxy Authentication Required" },
            { "408", "Request Timeout" },
            { "409", "Conflict" },
            { "410", "Gone" },
            { "411", "Length Required" },
            { "412", "Precondition Failed" },
            { "413", "Request Entity Too Large" },
            { "414", "Request-URI Too Long" },
            { "415", "Unsupported Media Type" },
            { "416", "Requested Range Not Satisfiable" },
            { "417", "Expectation Failed" },
            { "500", "Internal Server Error" },
            { "501", "Not Implemented" },
            { "502", "Bad Gateway" },
            { "503", "Service Unavailable" },
            { "504", "Gateway Timeout" },
            { "505", "HTTP Version Not Supported" },
        };
        /**
     * Table mapping response codes to default bodies
     * See RFC 2616.
     */
        public static Dictionary<string, string> defaultBodiesByResponseCode = new Dictionary<string, string>{
            { "100", "Request received, please continue" },
            { "101", "Switching to new protocol; obey Upgrade header" },
            { "200", "Request fulfilled, document follows" },
            { "201", "Document created, URL follows" },
            { "202", "Request accepted, processing continues off-line" },
            { "203", "Request fulfilled from cache" },
            { "204", "Request fulfilled, nothing follows" },
            { "205", "Clear input form for further input." },
            { "206", "Partial content follows." },
            { "300", "Object has several resources -- see URI list" },
            { "301", "Object moved permanently -- see URI list" },
            { "302", "Object moved temporarily -- see URI list" },
            { "303", "Object moved -- see Method and URL list" },
            { "304", "Document has not changed since given time" },
            { "305", "You must use proxy specified in Location to access this resource." },
            { "307", "Object moved temporarily -- see URI list" },
            { "400", "Bad request syntax or unsupported method" },
            { "401", "No permission -- see authorization schemes" },
            { "402", "No payment -- see charging schemes" },
            { "403", "Request forbidden -- authorization will not help" },
            { "404", "Nothing matches the given URI" },
            { "405", "Specified method is invalid for this resource." },
            { "406", "URI not available in preferred format." },
            { "407", "You must authenticate with this proxy before proceeding." },
            { "408", "Request timed out; try again later." },
            { "409", "Request conflict." },
            { "410", "URI no longer exists and has been permanently removed." },
            { "411", "Client must specify Content-Length." },
            { "412", "Precondition in headers is false." },
            { "413", "Entity is too large." },
            { "414", "URI is too long." },
            { "415", "Entity body in unsupported format." },
            { "416", "Cannot satisfy request range." },
            { "417", "Expect condition could not be satisfied." },
            { "500", "Server got itself in trouble" },
            { "501", "Server does not support this operation" },
            { "502", "Invalid responses from another server/proxy." },
            { "503", "The server cannot process the request due to a high load" },
            { "504", "The gateway server did not receive a timely response" },
            { "505", "Cannot fulfill request." },
        };
    }
}
