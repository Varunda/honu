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
            services.AddSingleton<DirectiveTreeCategoryCollection>();
            services.AddSingleton<CharacterDirectiveCollection>();
            services.AddSingleton<CharacterDirectiveTreeCollection>();
            services.AddSingleton<CharacterDirectiveTierCollection>();
            services.AddSingleton<CharacterDirectiveObjectiveCollection>();

            services.AddSingleton<OutfitCollection, OutfitCollection>();

            // Static collections
            services.AddSingleton<MapCollection>();
            services.AddSingleton<FacilityCollection>();
            services.AddSingleton<IStaticCollection<PsItem>, ItemCollection>();
            services.AddSingleton<ItemCollection>();
            services.AddSingleton<IStaticCollection<ItemCategory>, ItemCategoryCollection>();
            services.AddSingleton<ItemCategoryCollection>();
            services.AddSingleton<IStaticCollection<ItemType>, ItemTypeCollection>();
            services.AddSingleton<ItemTypeCollection>();
            services.AddSingleton<IStaticCollection<PsDirective>, DirectiveCollection>();
            services.AddSingleton<DirectiveCollection>();
            services.AddSingleton<IStaticCollection<DirectiveTree>, DirectiveTreeCollection>();
            services.AddSingleton<DirectiveTreeCollection>();
            services.AddSingleton<IStaticCollection<DirectiveTier>, DirectiveTierCollection>();
            services.AddSingleton<DirectiveTierCollection>();
            services.AddSingleton<IStaticCollection<DirectiveTreeCategory>, DirectiveTreeCategoryCollection>();
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
            services.AddSingleton<IStaticCollection<ExperienceType>, ExperienceTypeCollection>();
            services.AddSingleton<ExperienceTypeCollection>();
            services.AddSingleton<IStaticCollection<FireGroupToFireMode>, FireGroupToFireModeCollection>();
            services.AddSingleton<FireGroupToFireModeCollection>();

            services.AddSingleton<RealtimeMapStateCollection>();
        }

    }

}