using Microsoft.Extensions.DependencyInjection;

namespace watchtower.Services.Queues {

    public static class ServiceCollectionExtentionMethods {

        /// <summary>
        ///     And the various queues Honu uses
        /// </summary>
        /// <param name="services">Extension instance</param>
        public static void AddHonuQueueServices(this IServiceCollection services) {
            services.AddSingleton<CensusRealtimeEventQueue>();
            services.AddSingleton<CharacterCacheQueue, CharacterCacheQueue>();
            services.AddSingleton<SessionStarterQueue, SessionStarterQueue>();
            services.AddSingleton<DiscordMessageQueue, DiscordMessageQueue>();
            services.AddSingleton<CharacterUpdateQueue>();
            services.AddSingleton<WeaponPercentileCacheQueue>();
            services.AddSingleton<LogoutUpdateBuffer>();
            services.AddSingleton<JaegerSignInOutQueue>();
            services.AddSingleton<WeaponUpdateQueue>();
            services.AddSingleton<PsbAccountPlaytimeUpdateQueue>();
            services.AddSingleton<SessionEndQueue>();
            services.AddSingleton<AlertEndQueue>();
            services.AddSingleton<WrappedGenerationQueue>();
            services.AddSingleton<FacilityControlEventProcessQueue>();
        }

    }
}
