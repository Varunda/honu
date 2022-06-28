using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuCollectionServices(this IServiceCollection services) {
            // Character collections
            services.AddSingleton<CharacterCollection>();
            services.AddSingleton<CharacterWeaponStatCollection>();
            services.AddSingleton<CharacterHistoryStatCollection>();
            services.AddSingleton<CharacterItemCollection>();
            services.AddSingleton<CharacterStatCollection>();
            services.AddSingleton<CharacterFriendCollection>();
            services.AddSingleton<CharacterAchievementCollection>();

            services.AddSingleton<OutfitCollection, OutfitCollection>();

            // Static collections
            services.AddSingleton<IStaticCollection<PsItem>, ItemCollection>();
            services.AddSingleton<ItemCollection>();
            services.AddSingleton<MapCollection>();
            services.AddSingleton<FacilityCollection>();
            services.AddSingleton<IStaticCollection<ItemCategory>, ItemCategoryCollection>();
            services.AddSingleton<ItemCategoryCollection>();
            services.AddSingleton<IStaticCollection<ItemType>, ItemTypeCollection>();
            services.AddSingleton<ItemTypeCollection>();

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

            services.AddSingleton<IStaticCollection<PsVehicle>, VehicleCollection>();
            services.AddSingleton<VehicleCollection>();

            services.AddSingleton<IStaticCollection<Achievement>, AchievementCollection>();
            services.AddSingleton<AchievementCollection>();
        }
    }

}