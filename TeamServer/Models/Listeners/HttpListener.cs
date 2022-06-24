using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class HttpListener : Listener
    {
        

        public HttpListener(string name, int bindPort, string ip, int? publicPort = null) : base(name, bindPort, ip, publicPort)
        {
        }

        private CancellationTokenSource _tokenSource;

        public override async Task Start()
        {


            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseUrls($"https://*:{BindPort}");
                    host.Configure(ConfigureApp);
                    host.ConfigureServices(ConfigureServices);
                    host.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, BindPort, listenOptions =>
                        {
                            //listenOptions.UseHttps("sslcert.pfx");
                            //listenOptions.UseHttps("certs/ts.pfx", "teamserver");
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
                e.MapControllerRoute("/", "/", new { Controller = "HttpListener", Action = "HandleImplant" });
                e.MapControllerRoute("SetupDownload", "/SetupDownload", new { Controller = "HttpListener", Action = "SetupDownload" });
                e.MapControllerRoute("DownloadChunk", "/DownloadChunk", new { Controller = "HttpListener", Action = "DownloadChunk" });
                e.MapControllerRoute("SetupUpload", "/SetupUpload", new { Controller = "HttpListener", Action = "SetupUpload" });
                e.MapControllerRoute("UploadChunk", "/UploadChunk", new { Controller = "HttpListener", Action = "UploadChunk" });
                e.MapControllerRoute("ModuleInfo", "/ModuleInfo", new { Controller = "HttpListener", Action = "ModuleInfo" });
                e.MapControllerRoute("Stager", "/Stager", new { Controller = "HttpListener", Action = "DownloadStager" });
            });
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}
