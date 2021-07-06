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
using watchtower.Controllers;
using watchtower.Hubs;
using watchtower.Realtime;
using watchtower.Services;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
using Newtonsoft.Json.Linq;
using watchtower.Models;
using System.IO;
using watchtower.Services.Hosted;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Db.Implementations;
using watchtower.Services.Census;
using watchtower.Services.Census.Implementations;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.Implementations;
using watchtower.Services.Implementations;
using watchtower.Services.Db.Readers;

namespace watchtower {

    public class Startup {

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
                //options.LogCensusErrors = true;
                options.LogCensusErrors = false;
            });

            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            });

            services.AddMvc(options => {

            }).AddRazorRuntimeCompilation();

            services.AddRazorPages();
            services.AddMemoryCache();

            services.Configure<DbOptions>(Configuration.GetSection("DbOptions"));

            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IDbCreator, DefaultDbCreator>();

            services.AddSingleton<IRealtimeMonitor, RealtimeMonitor>();
            services.AddSingleton<IEventHandler, Realtime.EventHandler>();

            services.AddSingleton<ICommandBus, CommandBus>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IBackgroundCharacterCacheQueue, CharacterCacheQueue>();

            // Db services
            services.AddSingleton<IOutfitDbStore, OutfitDbStore>();
            services.AddSingleton<IKillEventDbStore, KillEventDbStore>();
            services.AddSingleton<IExpEventDbStore, ExpEventDbStore>();
            services.AddSingleton<ICharacterDbStore, CharacterDbStore>();
            services.AddSingleton<IWorldTotalDbStore, WorldTotalDbStore>();

            // Readers
            services.AddSingleton<IDataReader<KillDbEntry>, KillDbEntryReader>();
            services.AddSingleton<IDataReader<KillDbOutfitEntry>, KillDbOutfitEntryReader>();

            // Census services
            services.AddSingleton<ICharacterCollection, CharacterCollection>();
            services.AddSingleton<IOutfitCollection, OutfitCollection>();

            // Repositories
            services.AddSingleton<ICharacterRepository, CharacterRepository>();
            services.AddSingleton<IOutfitRepository, OutfitRepository>();

            // Hosted services
            services.AddHostedService<DbCreatorHostedService>(); // Have first to ensure DBs exist
            services.AddHostedService<HostedRealtimeMonitor>();
            services.AddHostedService<EventCleanupService>();
            services.AddHostedService<DataBuilderService>();
            services.AddHostedService<HostedBackgroundCharacterCacheQueue>();
            services.AddHostedService<EventProcessService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime) {

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

    }
}
