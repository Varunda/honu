using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Models.Report;
using watchtower.Services.Db.Implementations;
using watchtower.Services.Repositories.Implementations;

namespace watchtower.Services.Repositories {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuRepositoryServices(this IServiceCollection services) {
            services.AddSingleton<ICharacterRepository, CharacterRepository>();
            services.AddSingleton<OutfitRepository>();
            services.AddSingleton<IWorldDataRepository, WorldDataRepository>();
            services.AddSingleton<ItemRepository>();
            services.AddSingleton<IStaticRepository<PsItem>, ItemRepository>();
            services.AddSingleton<IDataBuilderRepository, DataBuilderRepository>();
            services.AddSingleton<IMapRepository, MapRepository>();
            services.AddSingleton<ICharacterWeaponStatRepository, CharacterWeaponStatRepository>();
            services.AddSingleton<ICharacterHistoryStatRepository, CharacterHistoryStatRepository>();
            services.AddSingleton<ICharacterItemRepository, CharacterItemRepository>();
            services.AddSingleton<ICharacterStatRepository, CharacterStatRepository>();
            services.AddSingleton<WorldPopulationRepository, WorldPopulationRepository>();
            services.AddSingleton<CharacterFriendRepository>();
            services.AddSingleton<DirectiveRepository>();
            services.AddSingleton<DirectiveTreeRepository>();
            services.AddSingleton<DirectiveTierRepository>();
            services.AddSingleton<DirectiveTreeCategoryRepository>();
            services.AddSingleton<CharacterDirectiveRepository>();
            services.AddSingleton<CharacterDirectiveTreeRepository>();
            services.AddSingleton<CharacterDirectiveTierRepository>();
            services.AddSingleton<CharacterDirectiveObjectiveRepository>();
            services.AddSingleton<CharacterAchievementRepository>();

            services.AddSingleton<IStaticRepository<PsObjective>, ObjectiveRepository>();
            services.AddSingleton<ObjectiveRepository>();
            services.AddSingleton<IStaticRepository<ObjectiveType>, ObjectiveTypeRepository>();
            services.AddSingleton<ObjectiveTypeRepository>();
            services.AddSingleton<IStaticRepository<ObjectiveSet>, ObjectiveSetRepository>();
            services.AddSingleton<ObjectiveSetRepository>();

            services.AddSingleton<IStaticRepository<Achievement>, AchievementRepository>();
            services.AddSingleton<AchievementRepository>();
            services.AddSingleton<ReportRepository>();
        }

    }

}