using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using watchtower.Census;
using watchtower.Controllers;
using watchtower.Hubs;
using watchtower.Realtime;
using watchtower.Services;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
using Newtonsoft.Json.Linq;
using watchtower.Models;
using System.IO;
using watchtower.Services.Hosted;

namespace watchtower {

    public class Startup {

        private IFileEventLoader? _EventLoader;

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddRouting();

            services.AddCensusServices(options => {
                options.CensusServiceId = "asdf";
                options.CensusServiceNamespace = "ps2";
                options.LogCensusErrors = true;
            });

            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            });

            services.AddMvc(options => {

            }).AddRazorRuntimeCompilation();

            services.AddRazorPages();

            services.AddSingleton<IRealtimeMonitor, RealtimeMonitor>();
            services.AddSingleton<IEventHandler, Realtime.EventHandler>();
            services.AddSingleton<ICharacterCollection, CharacterCollection>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<ICommandBus, CommandBus>();
            services.AddSingleton<IFileEventLoader, FileEventLoader>();

            services.AddHostedService<HostedRealtimeMonitor>();
            services.AddHostedService<EventCleanupService>();
            services.AddHostedService<DataBuilderService>();
            services.AddHostedService<HostedFileEventLoader>();
            services.AddHostedService<EventProcessService>();
            services.AddHostedService<BackgroundFileSaver>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime, IFileEventLoader fileLoader) {

            _EventLoader = fileLoader;

            lifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}"
                );

                endpoints.MapHub<DataHub>("/ws/data");
            });
        }

        private void OnShutdown() {
            if (_EventLoader != null) {
                _EventLoader.Save("PreviousEvents.json").ConfigureAwait(false).GetAwaiter().GetResult();
            } else {
                Console.WriteLine($"_EventLoader is null");
            }
        }

    }
}
