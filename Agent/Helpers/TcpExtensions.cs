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
    }
}
