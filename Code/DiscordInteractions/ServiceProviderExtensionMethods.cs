using Microsoft.Extensions.DependencyInjection;

namespace watchtower.Code.DiscordInteractions {

    public static class ServiceProviderExtensionMethods {

        public static void AddHonuDiscord(this IServiceCollection services) {
            services.AddSingleton<LookupDiscordInteractions>();
        }

    }
}
