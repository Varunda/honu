using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Census.Readers {

    public static class ServiceCollectionExtensionMethods {

        /// <summary>
        ///     Add the <see cref="ICensusReader{T}"/>s used for the collection services
        /// </summary>
        /// <param name="services">Extension instance</param>
        public static void AddHonuCensusReadersServices(this IServiceCollection services) {
            services.AddSingleton<ICensusReader<CharacterItem>, CensusCharacterItemReader>();
            services.AddSingleton<ICensusReader<PsCharacterStat>, CensusCharacterStatReader>();
            services.AddSingleton<ICensusReader<OutfitMember>, CensusOutfitMemberReader>();
            services.AddSingleton<ICensusReader<PsDirective>, CensusDirectiveReader>();
            services.AddSingleton<ICensusReader<DirectiveTree>, CensusDirectiveTreeReader>();
            services.AddSingleton<ICensusReader<DirectiveTier>, CensusDirectiveTierReader>();
            services.AddSingleton<ICensusReader<DirectiveTreeCategory>, CensusDirectiveTreeCategoryReader>();
            services.AddSingleton<ICensusReader<CharacterDirective>, CensusCharacterDirectiveReader>();
            services.AddSingleton<ICensusReader<CharacterDirectiveTree>, CensusCharacterDirectiveTreeReader>();
            services.AddSingleton<ICensusReader<CharacterDirectiveTier>, CensusCharacterDirectiveTierReader>();
            services.AddSingleton<ICensusReader<CharacterDirectiveObjective>, CensusCharacterDirectiveObjectiveReader>();
            services.AddSingleton<ICensusReader<ObjectiveType>, CensusObjectiveTypeReader>();
            services.AddSingleton<ICensusReader<ObjectiveSet>, CensusObjectiveSetReader>();
            services.AddSingleton<ICensusReader<PsObjective>, CensusObjectiveReader>();
            services.AddSingleton<ICensusReader<Achievement>, CensusAchievementReader>();
            services.AddSingleton<ICensusReader<PsItem>, CensusItemReader>();
            services.AddSingleton<ICensusReader<CharacterAchievement>, CensusCharacterAchievementReader>();
            services.AddSingleton<ICensusReader<PsOutfit>, CensusOutfitReader>();
            services.AddSingleton<ICensusReader<PsCharacter>, CensusCharacterReader>();
            services.AddSingleton<ICensusReader<PsVehicle>, CensusVehicleReader>();
            services.AddSingleton<ICensusReader<ItemCategory>, CensusItemCategoryReader>();
            services.AddSingleton<ICensusReader<ItemType>, CensusItemTypeReader>();
            services.AddSingleton<ICensusReader<ExperienceType>, CensusExperienceTypeReader>();
            services.AddSingleton<ICensusReader<FireGroupToFireMode>, CensusFireGroupToFireModeReader>();
            services.AddSingleton<ICensusReader<RealtimeMapState>, CensusRealtimeMapStateReader>();
            services.AddSingleton<ICensusReader<PsCharacterStatByFaction>, CensusCharacterStatByFactionReader>();
        }

    }

}