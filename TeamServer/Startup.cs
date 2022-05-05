using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
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

            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamServer v1"));
            }
            

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            this.StartDefaultListener(app);
        }

        private void StartDefaultListener(IApplicationBuilder app)
        {
            var listenerService = app.ApplicationServices.GetService<IListenerService>();
            var agentService = app.ApplicationServices.GetService<IAgentService>();
            var fileService = app.ApplicationServices.GetService<IFileService>();
            var binMakerService = app.ApplicationServices.GetService<IBinMakerService>();

            var factory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger("Default Listener Start");

            var listener = new HttpListener("Default Listener", "192.168.56.1", 8080);
            listener.Init(agentService, fileService, binMakerService);
            listener.Start();
            try
            {
                logger.LogInformation(binMakerService.GenerateStagersFor(listener));
            }
            catch(Exception ex)
            {
                logger.LogError($"Unable to create stagers for {listener.Name}");
                logger.LogError(ex.ToString());
            }

            listenerService.AddListener(listener);
        }
    }
}
