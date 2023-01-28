using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
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

                var t = new Thread(async () => await HandleClient(client));
                t.Start();

               /* try
                {
                    //Init
                    var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    var src = endpoint.Address + ":" + endpoint.Port;

                    //Console.WriteLine($"{id} : Connected");

                    //// read data from client
                    var data = client.ReceivedData();

                    if (data == null || data.Length == 0)
                    {
                        continue;
                    }

                    //// read the first byte, which is the SOCKS version
                    var version = Convert.ToInt32(data[0]);

                    // read connect request
                    var request = Socks4Request.FromBytes(data);

                    // connect to destination
                    //var destination = new TcpClient();
                    //await destination.ConnectAsync(request.DestinationAddress, request.DestinationPort);

                    var id = src + "|" +  request.DestinationAddress + ":" + request.DestinationPort;


                    agent.SendProxyRequest(new SocksMessage()
                    {
                        Source = id,
                        ConnexionState = true
                    });

                    if (this.Clients.TryAdd(id, new ClientWrapper() { TcpClient = client, Id = id }))
                    {
                        Logger.Log($"Socks5 : new client {id} added");
                    }
                    else
                    {
                        Logger.Log($"Socks5 : cannot add new client {id}");
                    }

                }
                catch (Exception ex)
                {
                    Logger.Log($"Socks5 : Error {ex.ToString()}");
                    continue;
                }*/



                // do something with the connected client
                //var thread = new Thread(async () => await HandleClient(client));
                //thread.Start();
            }
            // handle client in new thread

            listener.Stop();
            this.IsRunning = false;
        }

        private async Task HandleClients()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                //Logger.Log($"Socks5 :Handle Clients {this.Clients.Count}");
                foreach (var key in this.Clients.Keys.ToList())
                {
                    var client = this.Clients[key];
                    try
                    {
                        var receivedMessages = this.agent.GetProxyResponses(client.Id);
                        if (!client.AckReceived)
                        {
                            //Logger.Log($"Socks5 : {id} No ack");
                            if (receivedMessages.Any())
                            {
                                //Logger.Log($"Socks5 : {id} Messages received");
                                client.AckReceived = true;
                                if (receivedMessages.Any(r => r.ConnexionState))
                                {
                                    SendConnectReply(client.TcpClient, true);
                                    Logger.Log($"Socks5 : {client.Id} Connection accepted");
                                }
                                else
                                {
                                    SendConnectReply(client.TcpClient, false);
                                    Logger.Log($"Socks5 : {client.Id} Connection refused");
                                    break;
                                }
                            }
                            else //no message, no ack => waiting
                                continue;
                        }

                        if (!client.TcpClient.IsAlive())
                        {
                            agent.SendProxyRequest(new SocksMessage()
                            {
                                Source = client.Id,
                                ConnexionState = false
                            });
                            Logger.Log($"Socks5 : {client.Id} Connection closed (source)");
                            this.Clients.Remove(client.Id, out _);
                            continue;
                        }

                        // read from client
                        if (client.TcpClient.DataAvailable())
                        {
                            var req = client.TcpClient.ReceivedData();

                            // send to destination
                            //destination.SendData(req);
                            //Should send data to the agent
                            agent.SendProxyRequest(new SocksMessage()
                            {
                                Source = client.Id,
                                ConnexionState = true,
                                Data = Convert.ToBase64String(req)
                            });

                            Logger.Log($"Socks5 : {client.Id} Client => Dest {req.Length}");

                        }

                        // read from destination
                        //should read from the agent responses
                        while (receivedMessages.Any())
                        {
                            var mess = receivedMessages.Dequeue();
                            if (!mess.ConnexionState)
                            {
                                Logger.Log($"Socks5 : {client.Id} Connection closed (dest)");
                                try
                                {
                                    client.TcpClient.Close();
                                }
                                catch { }

                                this.Clients.Remove(client.Id, out _);
                                break;
                            }

                            if (!string.IsNullOrEmpty(mess.Data))
                            {
                                var resp = Convert.FromBase64String(mess.Data);
                                client.TcpClient.SendData(resp);
                                Logger.Log($"Socks5 : {client.Id} Dest => Client {resp.Length}");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine(ex);
#endif
                    }
                    finally
                    {
                        //Console.WriteLine($"{id} : disconnected");
                    }
                }
                // sos cpu
                await Task.Delay(10);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                if (client == null)
                    return;

                var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                var src = endpoint.Address + ":" + endpoint.Port;

                //Console.WriteLine($"{id} : Connected");

                //// read data from client
                var data = client.ReceivedData();

                if (data == null || data.Length == 0)
                    return;

                //// read the first byte, which is the SOCKS version
                var version = Convert.ToInt32(data[0]);

                // read connect request
                var request = Socks4Request.FromBytes(data);

                // connect to destination
                //var destination = new TcpClient();
                //await destination.ConnectAsync(request.DestinationAddress, request.DestinationPort);

                var id = src + "|" +  request.DestinationAddress + ":" + request.DestinationPort;

                Logger.Log($"Socks5 : {src} Connection requested to {id}");

                agent.SendProxyRequest(new SocksMessage()
                {
                    Source = id,
                    ConnexionState = true
                });

                if (this.Clients.TryAdd(id, new ClientWrapper() { TcpClient = client, Id = id }))
                {
                    Logger.Log($"Socks5 : new client {id} added");
                }
                else
                {
                    Logger.Log($"Socks5 : cannot add new client {id}");
                }
            }
            catch { }
        }


        #region old
        /*  private async Task HandleClient(TcpClient client)
          {
              if (client == null)
                  return;

              bool ackReceived = false;

              var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
              var src = endpoint.Address + ":" + endpoint.Port;
              try
              {

                  //Console.WriteLine($"{id} : Connected");

                  //// read data from client
                  var data = client.ReceivedData();

                  if (data == null || data.Length == 0)
                      return;

                  //// read the first byte, which is the SOCKS version
                  var version = Convert.ToInt32(data[0]);

                  // read connect request
                  var request = Socks4Request.FromBytes(data);

                  // connect to destination
                  //var destination = new TcpClient();
                  //await destination.ConnectAsync(request.DestinationAddress, request.DestinationPort);

                  var id = src + "|" +  request.DestinationAddress + ":" + request.DestinationPort;

                  Logger.Log($"Socks5 : {src} Connection requested to {id}");

                  agent.SendProxyRequest(new SocksMessage()
                  {
                      Source = id,
                      ConnexionState = true
                  });

                  while (!_tokenSource.IsCancellationRequested)
                  {
                      var receivedMessages = this.agent.GetProxyResponses(id);
                      if (!ackReceived)
                      {
                          //Logger.Log($"Socks5 : {id} No ack");
                          if (receivedMessages.Any())
                          {
                              //Logger.Log($"Socks5 : {id} Messages received");
                              ackReceived = true;
                              if (receivedMessages.Any(r => r.ConnexionState))
                              {
                                  SendConnectReply(client, true);
                                  Logger.Log($"Socks5 : {id} Connection accepted");
                              }
                              else
                              {
                                  SendConnectReply(client, false);
                                  Logger.Log($"Socks5 : {id} Connection refused");
                                  break;
                              }
                          }
                          else //no message, no ack => waiting
                              continue;
                      }




                      if (!client.IsAlive())
                      {
                          agent.SendProxyRequest(new SocksMessage()
                          {
                              Source = id,
                              ConnexionState = false
                          });
                          Logger.Log($"Socks5 : {id} Connection closed (source)");
                          break;
                      }


                      // read from client
                      if (client.DataAvailable())
                      {
                          var req = client.ReceivedData();

                          // send to destination
                          //destination.SendData(req);
                          //Should send data to the agent
                          agent.SendProxyRequest(new SocksMessage()
                          {
                              Source = id,
                              ConnexionState = true,
                              Data = Convert.ToBase64String(req)
                          });

                          Logger.Log($"Socks5 : {id} Client => Dest {req.Length}");

                      }

                      // read from destination
                      //should read from the agent responses
                      bool shouldleave = false;
                      while (receivedMessages.Any())
                      {
                          var mess = receivedMessages.Dequeue();
                          if (!mess.ConnexionState)
                          {
                              Logger.Log($"Socks5 : {id} Connection closed (dest)");
                              shouldleave = true;
                          }

                          if (!string.IsNullOrEmpty(mess.Data))
                          {
                              var resp = Convert.FromBase64String(mess.Data);
                              client.SendData(resp);
                              Logger.Log($"Socks5 : {id} Dest => Client {resp.Length}");
                          }
                      }

                      if (shouldleave)
                          break;
                      //if (destination.DataAvailable())
                      //{
                      //    var resp = destination.ReceivedData();

                      //    // send back to client
                      //    client.SendData(resp);

                      //    Console.WriteLine($"{id} : Dest => Client {resp.Length}");
                      //}

                      // sos cpu
                      await Task.Delay(10);
                  }
              }
              catch (Exception ex)
              {
      #if DEBUG
                      Console.WriteLine(ex);
      #endif
                  try
                  {
                      SendConnectReply(client, false);
                  }
                  catch { }
              }
              finally
              {
                  //Console.WriteLine($"{id} : disconnected");
              }
          }
        */
        #endregion

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
    }
}
