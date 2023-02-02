using Agent.Communication;
using Agent.Helpers;
using Agent.Service.Pivoting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Helpers;
using System.IO;

namespace Agent.Service
{
    public class WebHostService : RunningService, IWebHostService
    {
        public override string ServiceName => "WebHost";

        public Dictionary<string, byte[]> Files { get; private set; } = new Dictionary<string, byte[]>();

        protected ConnexionUrl Connexion { get; set; }


        private HttpListener Listener;
        public void Start(ConnexionUrl conn)
        {
            this.Connexion = conn;

            this.Listener = new HttpListener();
            string url = "http://" + this.Connexion.Address + ":" + this.Connexion.Port + "/";
            this.Listener.Prefixes.Add(url);
            this.Listener.Start();
            base.Start();
        }

        public override void Stop()
        {
            if (this.Listener != null)
                this.Listener.Stop();
            base.Stop();
        }

        public override async Task Process()
        {
            if(this.Listener == null)
            {
                this.Stop();
                return;
            }
            // this blocks until a connection is received or token is cancelled
            var client = await this.Listener.AcceptHttpClientAsync(this._tokenSource);

            // do something with the connected client
            var thread = new Thread(async () => await HandleClient(client));
            thread.Start();
        }

        private async Task HandleClient(HttpListenerContext client)
        {
            if (client == null)
                return;

            try
            {
                HttpListenerRequest request = client.Request;
                HttpListenerResponse response = client.Response;

                if (!client.Request.Url.LocalPath.ToLower().StartsWith("/wh/") || client.Request.HttpMethod != "GET")
                {
                    await response.ReturnNotFound();
                    return;
                }

                var filename = client.Request.Url.LocalPath.Replace("/wh/", string.Empty);
                if(!this.Files.ContainsKey(filename.ToLower()))
                {
                    await response.ReturnNotFound();
                    return;
                }

                await response.ReturnFile(this.Files[filename.ToLower()]);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }
            finally
            {
                //Debug.WriteLine($"HTTP WebHost  : disconnected");
            }
        }

       
    }
}
