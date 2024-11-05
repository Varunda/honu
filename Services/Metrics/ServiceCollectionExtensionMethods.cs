using Microsoft.Extensions.DependencyInjection;

namespace watchtower.Services.Metrics {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuMetrics(this IServiceCollection services) {
            // don't forget to add the meter name to Program.cs too!
            services.AddSingleton<EventHandlerMetric>();
            services.AddSingleton<HubMetric>();
            services.AddSingleton<EventQueueMetric>();
            services.AddSingleton<CensusMetric>();
            services.AddSingleton<QueueMetric>();
            services.AddSingleton<ImageProxyMetric>();
            services.AddSingleton<RealtimeStreamMetric>();
            services.AddSingleton<CharacterCacheMetric>();
        }

    }
}
