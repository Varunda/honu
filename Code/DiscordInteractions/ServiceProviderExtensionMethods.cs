using Microsoft.Extensions.DependencyInjection;

namespace watchtower.Code.DiscordInteractions {

    public static class ServiceProviderExtensionMethods {

        /// <summary>
        ///     Add interaction services that generate responses based on inputs
        /// </summary>
        /// <param name="services">extension instance</param>
        public static void AddHonuDiscord(this IServiceCollection services) {
            services.AddSingleton<LookupDiscordInteractions>();
            services.AddSingleton<SubscribeDiscordInteractions>();
        }

    }
}
