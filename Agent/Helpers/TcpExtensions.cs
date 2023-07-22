using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Helpers
{
    public static class TcpClientExtensions
    {
        public static bool DataAvailable(this TcpClient client)
        {
            var stream = client.GetStream();
            return stream.DataAvailable;
        }

        public static async Task<byte[]> ReadClient(this TcpClient client)
        {
            var stream = client.GetStream();

            using (var ms = new MemoryStream())
            {
                int read;

                do
                {
                    var buf = new byte[1024];
                    read = await stream.ReadAsync(buf, 0, buf.Length);

                    if (read == 0)
                        break;

                    await ms.WriteAsync(buf, 0, read);
                }
                while (read >= 1024);

                return ms.ToArray();
            }
        }

        public static async Task WriteClient(this TcpClient client, byte[] data)
        {
            var stream = client.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
