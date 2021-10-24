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
using DaybreakGames.Census;
using watchtower.Services.Offline;
using DaybreakGames.Census.Stream;
using watchtower.Models;
using watchtower.Services.Hosted.Startup;
using watchtower.Models.Census;
using watchtower.Services.CharacterViewer;
using watchtower.Services.CharacterViewer.Implementations;

namespace watchtower {

    public class Startup {

        // Will watchtower attempt to connect to the Census Streaming service?
        //      if false, yes, operation is performed as normal and events are recorded in real time
        //      if true, no, no connections are made, and mock events are created to create fake data
        private readonly bool OFFLINE_MODE = false;

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddRouting();

            if (OFFLINE_MODE == false) {
                services.AddCensusServices(options => {
                    options.CensusServiceId = "asdf";
                    options.CensusServiceNamespace = "ps2";
                    //options.LogCensusErrors = true;
                    options.LogCensusErrors = false;
                });
            } else {
                services.AddSingleton<ICensusQueryFactory, OfflineCensusQueryFactory>();
                services.AddSingleton<ICensusClient, OfflineCensusClient>();
                services.AddSingleton<ICensusStreamClient, OfflineCensusStreamClient>();
            }

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
            services.Configure<DiscordOptions>(Configuration.GetSection("Discord"));

            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IDbCreator, DefaultDbCreator>();

            services.AddSingleton<IRealtimeMonitor, RealtimeMonitor>();
            services.AddSingleton<IEventHandler, EventHandler>();

            services.AddSingleton<ICommandBus, CommandBus>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IBackgroundCharacterCacheQueue, CharacterCacheQueue>();
            services.AddSingleton<IBackgroundSessionStarterQueue, BackgroundSessionStarterQueue>();
            services.AddSingleton<IServiceHealthMonitor, ServiceHealthMonitor>();
            services.AddSingleton<IDiscordMessageQueue, DiscordMessageQueue>();
            services.AddSingleton<IBackgroundCharacterWeaponStatQueue, BackgroundCharacterWeaponStatQueue>();
            services.AddSingleton<IBackgroundWeaponPercentileCacheQueue, BackgroundWeaponPercentileCacheQueue>();
            services.AddSingleton<ICharacterStatGeneratorStore, CharacterStatGeneratorStore>();

            // Db services
            services.AddSingleton<IOutfitDbStore, OutfitDbStore>();
            services.AddSingleton<IKillEventDbStore, KillEventDbStore>();
            services.AddSingleton<IExpEventDbStore, ExpEventDbStore>();
            services.AddSingleton<ICharacterDbStore, CharacterDbStore>();
            services.AddSingleton<IWorldTotalDbStore, WorldTotalDbStore>();
            services.AddSingleton<IItemDbStore, ItemDbStore>();
            services.AddSingleton<ISessionDbStore, SessionDbStore>();
            services.AddSingleton<IFacilityControlDbStore, FacilityControlDbStore>();
            services.AddSingleton<IFacilityDbStore, FacilityDbStore>();
            services.AddSingleton<IMapDbStore, MapDbStore>();
            services.AddSingleton<ICharacterWeaponStatDbStore, CharacterWeaponStatDbStore>();
            services.AddSingleton<IWeaponStatPercentileCacheDbStore, WeaponStatPercentileCacheDbStore>();
            services.AddSingleton<ICharacterHistoryStatDbStore, CharacterHistoryStatDbStore>();

            // Readers
            services.AddSingleton<IDataReader<KillDbEntry>, KillDbEntryReader>();
            services.AddSingleton<IDataReader<KillDbOutfitEntry>, KillDbOutfitEntryReader>();
            services.AddSingleton<IDataReader<KillEvent>, KillEventReader>();
            services.AddSingleton<IDataReader<ExpEvent>, ExpEventReader>();
            services.AddSingleton<IDataReader<KillItemEntry>, KillItemEntryReader>();
            services.AddSingleton<IDataReader<FacilityControlDbEntry>, FacilityControlEntryReader>();
            services.AddSingleton<IDataReader<PsFacilityLink>, PsFacilityLinkReader>();
            services.AddSingleton<IDataReader<PsMapHex>, PsMapHexReader>();
            services.AddSingleton<IDataReader<OutfitPopulation>, OutfitPopulationReader>();
            services.AddSingleton<IDataReader<PsItem>, ItemDbStore>();
            services.AddSingleton<IDataReader<PsCharacter>, CharacterDbStore>();

            // Census services
            services.AddSingleton<ICharacterCollection, CharacterCollection>();
            services.AddSingleton<IOutfitCollection, OutfitCollection>();
            services.AddSingleton<IItemCollection, ItemCollection>();
            services.AddSingleton<IMapCollection, MapCollection>();
            services.AddSingleton<IFacilityCollection, FacilityCollection>();
            services.AddSingleton<ICharacterWeaponStatCollection, CharacterWeaponStatCollection>();
            services.AddSingleton<ICharacterHistoryStatCollection, CharacterHistoryStatCollection>();

            // Repositories
            services.AddSingleton<ICharacterRepository, CharacterRepository>();
            services.AddSingleton<IOutfitRepository, OutfitRepository>();
            services.AddSingleton<IWorldDataRepository, WorldDataRepository>();
            services.AddSingleton<IItemRepository, ItemRepository>();
            services.AddSingleton<IDataBuilderRepository, DataBuilderRepository>();
            services.AddSingleton<IMapRepository, MapRepository>();
            services.AddSingleton<ICharacterWeaponStatRepository, CharacterWeaponStatRepository>();
            services.AddSingleton<ICharacterHistoryStatRepository, CharacterHistoryStatRepository>();

            // Hosted services
            services.AddHostedService<DbCreatorStartupService>(); // Have first to ensure DBs exist

            services.AddHostedService<HostedRealtimeMonitor>();
            services.AddHostedService<EventCleanupService>();
            services.AddHostedService<DataBuilderService>();
            services.AddHostedService<WorldDataBroadcastService>();
            services.AddHostedService<RealtimeResubcribeService>();
            services.AddHostedService<WorldOverviewBroadcastService>();
            services.AddHostedService<CharacterStatGeneratorPopulator>();

            services.AddHostedService<HostedBackgroundCharacterCacheQueue>();
            services.AddHostedService<EventProcessService>();
            services.AddHostedService<HostedSessionStarterQueue>();
            services.AddHostedService<FacilityPopulatorStartupService>();
            services.AddHostedService<ZoneStateStartupService>();
            services.AddHostedService<HostedBackgroundCharacterWeaponStatQueue>();
            services.AddHostedService<HostedBackgroundWeaponPercentileCacheQueue>();

            if (Configuration.GetValue<bool>("Discord:Enabled") == true) {
                services.AddHostedService<DiscordService>();
            }

            if (OFFLINE_MODE == true) {
                services.AddHostedService<OfflineDataMockService>();
            }
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
                    pattern: "/{action}",
                    defaults: new { controller = "Home", action = "SelectWorld" }
                );

                endpoints.MapControllerRoute(
                    name: "charview",
                    pattern: "/c/{charID}/{*.}",
                    defaults: new { controller = "Home", action = "CharacterViewer" }
                );

                endpoints.MapControllerRoute(
                    name: "sessionviewer",
                    pattern: "/s/{sessionID}/{*.}",
                    defaults: new { controller = "Home", action = "SessionViewer" }
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
                endpoints.MapHub<WorldOverviewHub>("/ws/overview");
            });
        }

    }
}
