using System;
using System.Collections.Generic;
using System.IO;

namespace NDeproxy
{
    public class UnbufferedStreamReader : TextReader
    {
        Stream _stream;

        public UnbufferedStreamReader(Stream stream)
        {
            _stream = stream;
        }

        public override int Read()
        {
            int value = _stream.ReadByte();

            if (value < 0)
                return -1;

            return (byte)value;
        }
    }
}
