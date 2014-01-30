using System.Text;
using System.IO;

namespace NDeproxy
{
    public class LineReader
    {
        // utility method to be used by all parts of the system for reading from
        // sockets. this way, there"s a consistent implementation and consistent
        // policy for end-of-line
        public static string readLine(TextReader reader)
        {
            int value = reader.Read();
            if (value < 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // naive definition of line termination
            // just consider \n, not \r or \r\n
            while (value >= 0 && value != '\n')
            {
                char ch = (char)value;
                if (ch != '\r')
                {
                    sb.Append(ch);
                }
                value = reader.Read();
            }

            return sb.ToString();
        }

        public static string readLine(Stream inStream)
        {
            return readLine(new UnbufferedStreamReader(inStream));
        }
    }
}
