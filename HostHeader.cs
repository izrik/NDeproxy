using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NDeproxy
{
    public class HostHeader : Header
    {
        // http://tools.ietf.org/html/rfc2616#section-14.23
        public HostHeader(string host, int? port = null)
        : base("Host", CreateHostHeaderValue(host, port))
        {

            this.host = host;
            this.port = port;
        }

        public readonly string host;
        public readonly int? port;
        //    static readonly string alpha                = /(?x) ( [a-zA-Z] )/
        static readonly string alpha = @"([a-zA-Z])";
        //    static readonly string alphanum             = /(?x) ( [a-zA-Z\d] )/
        static readonly string alphanum = @"( [a-zA-Z\d] )";
        //    static readonly string domainlabelPattern   = /(?x) ( ${alphanum} | ( ${alphanum} ( ${alphanum} | \- )* ${alphanum} ) )/
        static readonly string domainlabelPattern = string.Format(@"( ({0}) | ( ({0}) ( ({0}) | - )* ({0}) ) )", alphanum);
        //    static readonly string toplabelPattern      = /(?x) ( ${alpha}  | ( ${alpha} ( ${alphanum} | \- )* ${alphanum} ) )/
        static readonly string toplabelPattern = string.Format(@"( ({0})  | ( ({0}) ( ({1}) | - )* ({1}) ) )", alpha, alphanum);
        //    static readonly string hostnamePattern      = /(?x) ( ${domainlabelPattern} \. )* ( ${toplabelPattern} ) (\.?) /
        static readonly string hostnamePattern = string.Format(@"( ({0}) \. )* ( ({1}) ) (\.?)", domainlabelPattern, toplabelPattern);
        //    static readonly string IPv4addressPattern   = /(?x) ( [\d]+ \. [\d]+ \. [\d]+ \. [\d]+ ) /
        static readonly string IPv4addressPattern = @"( [\d]+ \. [\d]+ \. [\d]+ \. [\d]+ )";
        //    static readonly string hostPattern          = /(?x) ( ${hostnamePattern} | ${IPv4addressPattern} )/
        static readonly string hostPattern = string.Format(@"( ({0}) | ({1}) )", hostnamePattern, IPv4addressPattern);
        //    static readonly string portPattern          = /(?x) ( [\d]* )/
        static readonly string portPattern = @"( [\d]* )";



        public static string CreateHostHeaderValue(string host, int? port = null, bool? https = null)
        {

            if (!Regex.IsMatch(host, string.Format("^{0}$", hostPattern), RegexOptions.IgnorePatternWhitespace))
            {
                throw new ArgumentException("The value provided does not contain a valid hostname", "host");
            }

            if (port != null &&
                (port.Value < 0 ||
                port.Value > 65535))
            {
                throw new ArgumentException("The value provided contains an invalid port", "port");
            }

            return CreateHostHeaderValueNoCheck(host, port, https);
        }

        public static string CreateHostHeaderValueNoCheck(string host, int? port = null, bool? https = null)
        {

            if (port != null)
            {
                return string.Format("{0}:{1}", host, port.Value);
            }
            else if (https != null)
            {
                if (https.Value)
                {
                    return string.Format("{0}:443", host);
                }
                else
                {
                    return string.Format("{0}:80", host);
                }
            }
            else
            {
                return host;
            }
        }

        public static HostHeader fromString(string value)
        {

            //""" Takes a header value of the form "hostname:port" and returns a HostHeader"""

            // RFC 2616 § 14.23
            // Host = "Host" ":" host [ ":" port ]

            // RFC 2396 § 3.2.2
            // host          = hostname | IPv4address
            // hostname      = *( domainlabel "." ) toplabel [ "." ]
            // domainlabel   = alphanum | alphanum *( alphanum | "-" ) alphanum
            // toplabel      = alpha    | alpha *( alphanum | "-" ) alphanum
            // IPv4address   = 1*digit "." 1*digit "." 1*digit "." 1*digit
            // port          = *digit

            value = value.Trim();

            string host;
            string portStr;
            if (value.Contains(":"))
            {
                var parts = value.Split(new []{ ':' }, 2);
                host = parts[0].Trim();
                portStr = parts[1].Trim();
            }
            else
            {
                host = value;
                portStr = "";
            }


            if (!string.IsNullOrEmpty(portStr))
            {
                if (!Regex.IsMatch(portStr, string.Format("^{0}$", portPattern), RegexOptions.IgnorePatternWhitespace))
                {
                    throw new ArgumentException("The value provided does not contain a valid port", "value");
                }

                int port = int.Parse(portStr);
                return new HostHeader(host, port);
            }
            else
            {
                return new HostHeader(host);
            }
        }
    }
}
