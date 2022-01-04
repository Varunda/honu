using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Services.Census.Implementations;

namespace watchtower.Services.Census {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuCollectionServices(this IServiceCollection services) {
            // Character collections
            services.AddSingleton<ICharacterCollection, CharacterCollection>();
            services.AddSingleton<ICharacterWeaponStatCollection, CharacterWeaponStatCollection>();
            services.AddSingleton<ICharacterHistoryStatCollection, CharacterHistoryStatCollection>();
            services.AddSingleton<ICharacterItemCollection, CharacterItemCollection>();
            services.AddSingleton<ICharacterStatCollection, CharacterStatCollection>();
            services.AddSingleton<CharacterFriendCollection>();
            services.AddSingleton<CharacterAchievementCollection>();

            services.AddSingleton<OutfitCollection, OutfitCollection>();

            // Static collections
            services.AddSingleton<IStaticCollection<PsItem>, ItemCollection>();
            services.AddSingleton<ItemCollection>();
            services.AddSingleton<IMapCollection, MapCollection>();
            services.AddSingleton<IFacilityCollection, FacilityCollection>();

            // Directive collections
            services.AddSingleton<DirectiveCollection>();
            services.AddSingleton<DirectiveTreeCollection>();
            services.AddSingleton<DirectiveTierCollection>();
            services.AddSingleton<DirectiveTreeCategoryCollection>();
            services.AddSingleton<CharacterDirectiveCollection>();
            services.AddSingleton<CharacterDirectiveTreeCollection>();
            services.AddSingleton<CharacterDirectiveTierCollection>();
            services.AddSingleton<CharacterDirectiveObjectiveCollection>();

            // Objective collections
            services.AddSingleton<IStaticCollection<PsObjective>, ObjectiveCollection>();
            services.AddSingleton<ObjectiveCollection>();
            services.AddSingleton<IStaticCollection<ObjectiveType>, ObjectiveTypeCollection>();
            services.AddSingleton<ObjectiveTypeCollection>();
            services.AddSingleton<IStaticCollection<ObjectiveSet>, ObjectiveSetCollection>();
            services.AddSingleton<ObjectiveSetCollection>();

            services.AddSingleton<IStaticCollection<Achievement>, AchievementCollection>();
            services.AddSingleton<AchievementCollection>();
        }
    }

}