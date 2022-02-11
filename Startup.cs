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
using watchtower.Services.Census.Readers;
using watchtower.Code.Converters;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using watchtower.Services.Queues;
using watchtower.Models.Queues;
using Microsoft.Extensions.Logging;
using watchtower.Models.Report;
using watchtower.Services.Hosted.PSB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

//using honu_census;

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

            string stuff = ((IConfigurationRoot)Configuration).GetDebugView();
            Console.WriteLine(stuff);

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

            //services.AddSingleton<HonuCensus>();

            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol(options => {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddMvc(options => {

            }).AddJsonOptions(config => {
                config.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
            }).AddRazorRuntimeCompilation();

            services.AddSwaggerGen(doc => {
                doc.SwaggerDoc("api", new OpenApiInfo() { Title = "API", Version = "v0.1" });

                Console.Write("Including XML documentation in: ");
                foreach (string file in Directory.GetFiles(AppContext.BaseDirectory, "*.xml")) {
                    Console.Write($"{Path.GetFileName(file)} ");
                    doc.IncludeXmlComments(file);
                }
                Console.WriteLine("");
            });

            string gIDKey = "Authentication:Google:ClientId";
            string gSecretKey = "Authentication:Google:ClientSecret";

            string? googleClientID = Configuration[gIDKey];
            string? googleSecret = Configuration[gSecretKey];

            if (string.IsNullOrEmpty(googleClientID) == false && string.IsNullOrEmpty(googleSecret) == false) {
                services.AddAuthentication(options => {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                }).AddCookie(options => {
                    //options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    //options.Cookie.SameSite = SameSiteMode.Lax;
                }).AddGoogle(options => {
                    options.ClientId = googleClientID;
                    options.ClientSecret = googleSecret;
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                });

                Console.WriteLine($"Added Google auth");
            } else {
                Console.WriteLine($"===============================================================");
                Console.WriteLine($"!!! GOOGLE AUTH NOT SETUP");
                Console.WriteLine($"!!! missing either '{gIDKey}' ({string.IsNullOrEmpty(googleClientID)}) or '{gSecretKey}' ({string.IsNullOrEmpty(googleSecret)})");
                Console.WriteLine($"!!! GOOGLE AUTH NOT SETUP");
                Console.WriteLine($"===============================================================");
            }

            services.AddRazorPages();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            services.Configure<DbOptions>(Configuration.GetSection("DbOptions"));
            services.Configure<DiscordOptions>(Configuration.GetSection("Discord"));

            services.AddTransient<IActionResultExecutor<ApiResponse>, ApiResponseExecutor>();
            services.AddSingleton<IDbHelper, DbHelper>();
            services.AddSingleton<IDbCreator, DefaultDbCreator>();

            services.AddSingleton<IRealtimeMonitor, RealtimeMonitor>();
            services.AddSingleton<IEventHandler, Realtime.EventHandler>();
            services.AddSingleton<CommandBus, CommandBus>();
            services.AddSingleton<ICharacterStatGeneratorStore, CharacterStatGeneratorStore>();
            services.AddSingleton<IServiceHealthMonitor, ServiceHealthMonitor>();

            // Queues
            services.AddSingleton<CensusRealtimeEventQueue>();
            services.AddSingleton<CharacterCacheQueue, CharacterCacheQueue>();
            services.AddSingleton<SessionStarterQueue, SessionStarterQueue>();
            services.AddSingleton<DiscordMessageQueue, DiscordMessageQueue>();
            services.AddSingleton<CharacterUpdateQueue>();
            services.AddSingleton<WeaponPercentileCacheQueue>();
            services.AddSingleton<LogoutUpdateBuffer>();
            services.AddSingleton<ExtraStatHoster>();
            services.AddSingleton<JaegerSignInOutQueue>();

            services.AddHonuDatabasesServices(); // Db services
            services.AddHonuDatabaseReadersServices(); // DB readers
            services.AddHonuCollectionServices(); // Census services
            services.AddHonuCensusReadersServices(); // Census readers
            services.AddHonuRepositoryServices(); // Repositories

            // Hosted services
            services.AddHostedService<DbCreatorStartupService>(); // Have first to ensure DBs exist

            services.AddHostedService<HostedRealtimeMonitor>();
            services.AddHostedService<EventCleanupService>();
            services.AddHostedService<DataBuilderService>();
            services.AddHostedService<WorldDataBroadcastService>();
            services.AddHostedService<RealtimeResubcribeService>();
            services.AddHostedService<WorldOverviewBroadcastService>();
            services.AddHostedService<CharacterStatGeneratorPopulator>();
            services.AddHostedService<ZoneCheckerService>();
            services.AddHostedService<FacilityPopulatorStartupService>();
            services.AddHostedService<DirectiveCollectionsPopulator>();
            services.AddHostedService<ObjectiveCollectionsPopulator>();
            services.AddHostedService<AlertLoadStartupService>();

            // Hosted queues
            services.AddHostedService<HostedBackgroundCharacterCacheQueue>();
            services.AddHostedService<EventProcessService>();
            services.AddHostedService<HostedSessionStarterQueue>();
            services.AddHostedService<HostedJaegerSignInOutProcess>();

            if (Configuration.GetValue<bool>("Discord:Enabled") == true) {
                services.AddHostedService<DiscordService>();
            }

            services.AddTransient<CurrentHonuAccount>();

            //services.AddHostedService<PsbNamedImportStartupService>();
            //services.AddHostedService<PsbNamedCheckerService>();

            if (OFFLINE_MODE == true) {
                services.AddHostedService<OfflineDataMockService>();
            } else {
                services.AddHostedService<HostedBackgroundCharacterWeaponStatQueue>();
                services.AddHostedService<HostedBackgroundWeaponPercentileCacheQueue>();
                services.AddHostedService<HostedBackgroundLogoutBuffer>();
                //services.AddHostedService<CharacterDatesFixerStartupService>();
                //services.AddHostedService<OutfitMemberFixerStartupService>();
            }

            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Add(IPAddress.Parse("64.227.19.86"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime lifetime, ILogger<Startup> logger) {

            app.UseForwardedHeaders();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            logger.LogInformation($"Environment: {env.EnvironmentName}");

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSwagger(doc => { });
            app.UseSwaggerUI(doc => {
                doc.SwaggerEndpoint("/swagger/api/swagger.json", "api");
                doc.RoutePrefix = "api-doc";
                doc.DocumentTitle = "Honu API documentation";
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "psb",
                    pattern: "psb/{action}",
                    defaults: new { controller = "Psb" }
                );

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
                    name: "outfitviewer",
                    pattern: "/o/{outfitID}/{*.}",
                    defaults: new { controller = "Home", action = "OutfitViewer" }
                );

                endpoints.MapControllerRoute(
                    name: "itemstatviewer",
                    pattern: "/i/{itemID}/{*.}",
                    defaults: new { controller = "Home", action = "ItemStatViewer" }
                );

                endpoints.MapControllerRoute(
                    name: "directcharacter",
                    pattern: "/p/{name}/{*.}",
                    defaults: new { controller = "Home", action = "Player" }
                );

                endpoints.MapControllerRoute(
                    name: "outfitreport",
                    pattern: "/report/{*.}",
                    defaults: new { controller = "Home", action = "Report" }
                );

                endpoints.MapControllerRoute(
                    name: "worlddata",
                    pattern: "/view/{*.}",
                    defaults: new { controller = "Home", action = "Index" }
                );

                endpoints.MapControllerRoute(
                    name: "jaegernsa",
                    pattern: "/jaegernsa/{*.}",
                    defaults: new { controller = "Home", action = "JaegerNsa" }
                );

                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "/api/{controller}/{action}"
                );

                endpoints.MapHub<WorldDataHub>("/ws/data");
                endpoints.MapHub<WorldOverviewHub>("/ws/overview");
                endpoints.MapHub<ReportHub>("/ws/report");
                endpoints.MapHub<RealtimeMapHub>("/ws/realtime-map");

                endpoints.MapSwagger();
            });
        }

    }
}
