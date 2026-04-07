using System.Net.Sockets;
using System.Text;

namespace QLTK_Lite.Network
{
    /// <summary>
    /// Protocol: [4 byte big-endian length] + [UTF-8 body]
    /// Dùng chung cho cả QLTK_Lite và NRO_Skia
    /// </summary>
    public static class SocketHelper
    {
        public static void Send(NetworkStream stream, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var header = new byte[]
            {
                (byte)(body.Length >> 24),
                (byte)(body.Length >> 16),
                (byte)(body.Length >>  8),
                (byte)(body.Length)
            };
            stream.Write(header, 0, 4);
            stream.Write(body, 0, body.Length);
        }

        public static string Receive(NetworkStream stream)
        {
            var header = ReadExact(stream, 4);
            if (header == null) return null;

            int length = (header[0] << 24)
                       | (header[1] << 16)
                       | (header[2] << 8)
                       | header[3];

            if (length <= 0 || length > 1024 * 1024) return null;

            var body = ReadExact(stream, length);
            return body == null ? null : Encoding.UTF8.GetString(body);
        }

        private static byte[] ReadExact(NetworkStream stream, int n)
        {
            var buf = new byte[n];
            int total = 0;
            while (total < n)
            {
                int read = stream.Read(buf, total, n - total);
                if (read == 0) return null; // connection closed
                total += read;
            }
            return buf;
        }
    }
}