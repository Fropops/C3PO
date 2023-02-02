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
            var ns = client.GetStream();
            return ns.DataAvailable;
        }

        public static bool IsAlive(this TcpClient client)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(client.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(client.Client.RemoteEndPoint)).ToArray();
            if (tcpConnections != null && tcpConnections.Length > 0)
            {
                TcpState stateOfConnection = tcpConnections.First().State;
                if (stateOfConnection == TcpState.Established)
                {
                    return true;
                    // Connection is OK
                }
            }
            return false;
        }

        public static async Task<TcpClient> AcceptTcpClientAsync(this TcpListener listener, CancellationTokenSource cts)
        {
            using (cts.Token.Register(listener.Stop))
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    return client;
                }
                catch (ObjectDisposedException ex)
                {
                    // Token was canceled - swallow the exception and return null
                    if (cts.Token.IsCancellationRequested) return null;
                    throw ex;
                }
            }
        }

        public static byte[] ReceivedData(this TcpClient client)
        {
            // Get the client's NetworkStream
            var stream = client.GetStream();

            // Check if data is available to be read
            if (stream.DataAvailable)
            {
                // Read data from the stream
                var buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Return the data that was read
                var data = new byte[bytesRead];
                Array.Copy(buffer, 0, data, 0, bytesRead);
                return data;
            }
            else
            {
                // Return an empty array if no data is available
                return new byte[0];
            }
        }


        public static byte[] ReceivedMessage(this TcpClient client, bool waitMessage = false)
        {
            // Get the client's NetworkStream
            var stream = client.GetStream();

            while(client.IsAlive() && !stream.DataAvailable && waitMessage)
            {
                Thread.Sleep(10);
            }

            if (!stream.DataAvailable)
                return new byte[0];

            MemoryStream ms = new MemoryStream();
            // Check if data is available to be read
            while (stream.DataAvailable)
            {
                // Read data from the stream
                var buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, bytesRead);
            }

            ms.Seek(0, SeekOrigin.Begin);

            var req = new Byte[ms.Length];
            ms.Read(req, 0, (int)ms.Length);

            ms.Close();
            return req;

        }

        public static void SendMessage(this TcpClient client, byte[] data)
        {
            // Get the client's NetworkStream
            var stream = client.GetStream();

            // Write the data to the stream
            stream.Write(data, 0, data.Length);
        }
    }
}
