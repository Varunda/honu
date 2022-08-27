using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Models.Report;
using watchtower.Services.Db.Implementations;
using watchtower.Services.Repositories.PSB;

namespace watchtower.Services.Repositories {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuRepositoryServices(this IServiceCollection services) {
            services.AddSingleton<OutfitRepository>();
            services.AddSingleton<WorldDataRepository>();
            services.AddSingleton<DataBuilderRepository>();
            services.AddSingleton<MapRepository>();
            services.AddSingleton<WorldPopulationRepository>();

            // Character repos
            services.AddSingleton<CharacterRepository>();
            services.AddSingleton<CharacterFriendRepository>();
            services.AddSingleton<CharacterWeaponStatRepository>();
            services.AddSingleton<CharacterHistoryStatRepository>();
            services.AddSingleton<CharacterItemRepository>();
            services.AddSingleton<CharacterStatRepository>();
            services.AddSingleton<CharacterDirectiveRepository>();
            services.AddSingleton<CharacterDirectiveTreeRepository>();
            services.AddSingleton<CharacterDirectiveTierRepository>();
            services.AddSingleton<CharacterDirectiveObjectiveRepository>();
            services.AddSingleton<CharacterAchievementRepository>();

            // Static repos
            services.AddSingleton<IStaticRepository<PsItem>, ItemRepository>();
            services.AddSingleton<ItemRepository>();
            services.AddSingleton<IStaticRepository<PsObjective>, ObjectiveRepository>();
            services.AddSingleton<ObjectiveRepository>();
            services.AddSingleton<IStaticRepository<ObjectiveType>, ObjectiveTypeRepository>();
            services.AddSingleton<ObjectiveTypeRepository>();
            services.AddSingleton<IStaticRepository<ObjectiveSet>, ObjectiveSetRepository>();
            services.AddSingleton<ObjectiveSetRepository>();
            services.AddSingleton<IStaticRepository<PsVehicle>, VehicleRepository>();
            services.AddSingleton<VehicleRepository>();
            services.AddSingleton<IStaticRepository<ItemCategory>, ItemCategoryRepository>();
            services.AddSingleton<ItemCategoryRepository>();
            services.AddSingleton<IStaticRepository<Achievement>, AchievementRepository>();
            services.AddSingleton<AchievementRepository>();
            services.AddSingleton<ExperienceTypeRepository>();
            services.AddSingleton<IStaticRepository<ExperienceType>, ExperienceTypeRepository>();
            services.AddSingleton<DirectiveRepository>();
            services.AddSingleton<DirectiveTreeRepository>();
            services.AddSingleton<DirectiveTierRepository>();
            services.AddSingleton<DirectiveTreeCategoryRepository>();

            services.AddSingleton<ReportRepository>();
            services.AddSingleton<PsbAccountRepository>();
            services.AddSingleton<WorldOverviewRepository>();
            services.AddSingleton<FacilityRepository>();

            services.AddSingleton<AlertPlayerDataRepository>();
            services.AddSingleton<AlertRepository>();
            services.AddSingleton<AlertPopulationRepository>();

            services.AddSingleton<CensusRealtimeHealthRepository>();

            services.AddSingleton<RealtimeNetworkBuilder>();
            services.AddSingleton<RealtimeNetworkRepository>();

            services.AddSingleton<BadHealthRepository>();
            services.AddSingleton<SessionRepository>();

            services.AddSingleton<OutfitWarsMatchRepository>();
        }

    }

}