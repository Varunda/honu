using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using watchtower.Code.Hubs.Implementations;
using watchtower.Realtime;
using watchtower.Services;
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
using watchtower.Code.Hubs;
using System.Text.Json;
using watchtower.Models.Events;

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
            }).AddJsonProtocol(options => {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddMvc(options => {

            }).AddRazorRuntimeCompilation();

            services.AddRazorPages();
            services.AddMemoryCache();

            services.Configure<DbOptions>(Configuration.GetSection("DbOptions"));

            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IDbCreator, DefaultDbCreator>();

            services.AddSingleton<IRealtimeMonitor, RealtimeMonitor>();
            services.AddSingleton<IEventHandler, EventHandler>();

            services.AddSingleton<ICommandBus, CommandBus>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IBackgroundCharacterCacheQueue, CharacterCacheQueue>();
            services.AddSingleton<IServiceHealthMonitor, ServiceHealthMonitor>();

            // Db services
            services.AddSingleton<IOutfitDbStore, OutfitDbStore>();
            services.AddSingleton<IKillEventDbStore, KillEventDbStore>();
            services.AddSingleton<IExpEventDbStore, ExpEventDbStore>();
            services.AddSingleton<ICharacterDbStore, CharacterDbStore>();
            services.AddSingleton<IWorldTotalDbStore, WorldTotalDbStore>();
            services.AddSingleton<IItemDbStore, ItemDbStore>();

            // Readers
            services.AddSingleton<IDataReader<KillDbEntry>, KillDbEntryReader>();
            services.AddSingleton<IDataReader<KillDbOutfitEntry>, KillDbOutfitEntryReader>();
            services.AddSingleton<IDataReader<KillEvent>, KillEventReader>();

            // Census services
            services.AddSingleton<ICharacterCollection, CharacterCollection>();
            services.AddSingleton<IOutfitCollection, OutfitCollection>();
            services.AddSingleton<IItemCollection, ItemCollection>();

            // Repositories
            services.AddSingleton<ICharacterRepository, CharacterRepository>();
            services.AddSingleton<IOutfitRepository, OutfitRepository>();
            services.AddSingleton<IWorldDataRepository, WorldDataRepository>();
            services.AddSingleton<IItemRepository, ItemRepository>();

            // Hosted services
            services.AddHostedService<DbCreatorHostedService>(); // Have first to ensure DBs exist
            services.AddHostedService<HostedRealtimeMonitor>();
            services.AddHostedService<EventCleanupService>();
            services.AddHostedService<DataBuilderService>();
            services.AddHostedService<HostedBackgroundCharacterCacheQueue>();
            services.AddHostedService<EventProcessService>();
            services.AddHostedService<WorldDataBroadcastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "selectworld",
                    pattern: "/",
                    defaults: new { controller = "Home", action = "SelectWorld" }
                );

                endpoints.MapControllerRoute(
                    name: "worlddata",
                    pattern: "/view/{*.}",
                    defaults: new { controller = "Home", action = "Index" }
                );

                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "/api/{controller}/{action}"
                );

                endpoints.MapHub<WorldDataHub>("/ws/data");
            });
        }

    }
}
