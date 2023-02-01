using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Controllers;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            //services.AddSingleton<AgentsController>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamServer", Version = "v1" });
            });


            services.AddLogging();

            services.AddSingleton<IListenerService, ListenerService>();
            services.AddSingleton<IAgentService, AgentService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IBinMakerService, BinMakerService>();
            services.AddSingleton<ISocksService, SocksService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamServer v1"));
            }
            

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            //this.StartHttpHost(app);
            this.StartDefaultListener(app);
        }


        private IListenerService ls;
        private IFileService fs;

        private void StartHttpHost(IApplicationBuilder app)
        {
            ls = app.ApplicationServices.GetService<IListenerService>();
            fs = app.ApplicationServices.GetService<IFileService>();

            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseUrls($"http://*:80");
                    host.Configure(ConfigureHttpHostApp);
                    host.ConfigureServices(ConfigureHttpHostServices);                
                });
            var host = hostBuilder.Build();
            host.RunAsync();
        }


        private void ConfigureHttpHostServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IListenerService>(ls);
            services.AddSingleton(fs);
        }

        private void ConfigureHttpHostApp(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(e =>
            {
                
                e.MapControllerRoute("/", "/{id}", new { Controller = "HttpHost", Action = "Dload" });
                e.MapControllerRoute("/", "/", new { Controller = "HttpHost", Action = "Index" });
            });
        }

        private void StartDefaultListener(IApplicationBuilder app)
        {
            var listenerService = app.ApplicationServices.GetService<IListenerService>();
            var agentService = app.ApplicationServices.GetService<IAgentService>();
            var fileService = app.ApplicationServices.GetService<IFileService>();
            var binMakerService = app.ApplicationServices.GetService<IBinMakerService>();
            var config = app.ApplicationServices.GetService<IConfiguration>();

            var factory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger("Default Listener Start");

            binMakerService.GenerateB64s();

            var defaultListenersConfig = config.GetValue<string>("ListenersConfig");
            IEnumerable<ListenerConfig> listeners = JsonConvert.DeserializeObject<IEnumerable<ListenerConfig>>(defaultListenersConfig);
            foreach (var listenerConf in listeners)
            {
                var listener = new HttpListener(listenerConf.Name, listenerConf.BindPort, listenerConf.Address, listenerConf.Secured);
                listener.Init(agentService, fileService, binMakerService, listenerService, logger);
                listener.Start();
                listenerService.AddListener(listener);
            }
        }
    }
}
