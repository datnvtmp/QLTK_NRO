using System.Net.Sockets;
using System.Text;

/// <summary>
/// Dùng chung cho cả QLTK Lite và NRO_Skia
/// Protocol: [4 byte big-endian length] + [UTF-8 body]
/// </summary>
public static class SocketHelper
{
    // Gửi: 4 byte header + body
    public static void Send(NetworkStream stream, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var length = body.Length;

        // Big-endian header
        var header = new byte[4];
        header[0] = (byte)(length >> 24);
        header[1] = (byte)(length >> 16);
        header[2] = (byte)(length >> 8);
        header[3] = (byte)(length);

        stream.Write(header, 0, 4);
        stream.Write(body, 0, body.Length);
    }

    // Nhận: đọc header → biết length → đọc đủ body
    public static string Receive(NetworkStream stream)
    {
        var header = ReadExact(stream, 4);
        if (header == null) return null;

        int length = (header[0] << 24) | (header[1] << 16)
                   | (header[2] << 8) | header[3];

        if (length <= 0 || length > 1024 * 1024) return null; // tối đa 1MB

        var body = ReadExact(stream, length);
        return body == null ? null : Encoding.UTF8.GetString(body);
    }

    // Đọc đúng n byte, dù TCP chia bao nhiêu packet
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