using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotTCPServer
    {
        private readonly int _bindPort;
        private readonly IPAddress _bindAddress;

        private IMessageService _messageService;

        public PivotTCPServer(int bindPort, IPAddress bindAddress = null)
        {
            _bindPort = bindPort;
            _bindAddress = bindAddress ?? IPAddress.Any;

            _messageService = ServiceProvider.GetService<IMessageService>();
        }

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public async Task Start()
        {
            var listener = new TcpListener(_bindAddress, _bindPort);
            listener.Start(100);

            while (!_tokenSource.IsCancellationRequested)
            {
                // this blocks until a connection is received or token is cancelled
                var client = await listener.AcceptTcpClientAsync(_tokenSource);

                // do something with the connected client
                var thread = new Thread(async () => await HandleClient(client));
                thread.Start();
            }
            // handle client in new thread

            listener.Stop();
        }

        private async Task HandleClient(TcpClient client)
        {
            if (client == null)
                return;

            var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            var id = endpoint.Address + ":" + endpoint.Port;

            try
            {
                while (!_tokenSource.IsCancellationRequested && client.IsAlive())
                {

                    // read from client
                    if (!client.DataAvailable())
                        continue;

                    var req = client.ReceivedMessage();
                    //Convert.FromBase64String(b64results).Deserialize<List<MessageResult>>();
                    var responses = req.Deserialize<List<MessageResult>>();
                    _messageService.EnqueueResults(responses);

                    List<string> relays = new List<string>();
                    foreach (var mr in responses)
                    {
                        if (!relays.Contains(mr.Header.Owner))
                            relays.Add(mr.Header.Owner);
                    }

                    Debug.WriteLine($"TCP Pivot {this._bindAddress}:{this._bindPort} Sending task to Relays {string.Join(",", relays)}");
                    var tasks = this._messageService.GetMessageTasksToRelay(relays);
                    client.SendData(tasks.Serialize());
#if DEBUG
                    /*Console.WriteLine($"Relaying to \\\\{link.Hostname}\\{link.AgentId} :");
                    foreach (var mt in responses)
                    {
                        Console.WriteLine($"In ({mt.Header.Owner})");
                        foreach (var t in mt.Items)
                            Console.WriteLine($"Task ({mt.Header.Owner}) : {t.Command} ");
                    }
                    foreach (var mr in ret.Item1)
                    {
                        Console.WriteLine($"Out ({mr.Header.Owner})");
                        foreach (var r in mr.Items)
                            Console.WriteLine($"Result ({mr.Header.Owner}) : {r.Status} ");
                    }
                    var relays = string.Join(",", ret.Item2);
                    Console.WriteLine();*/
#endif
                }

                // sos cpu
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            finally
            {
                Debug.WriteLine($"TCP Pivot {id} : disconnected");
            }
        }


        public void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}
