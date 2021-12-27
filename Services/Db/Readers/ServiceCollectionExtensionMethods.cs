using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Models.Report;
using watchtower.Services.Census.Readers;
using watchtower.Services.Db.Implementations;

namespace watchtower.Services.Db.Readers {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuDatabaseReadersServices(this IServiceCollection services) {
            services.AddSingleton<IDataReader<KillDbEntry>, KillDbEntryReader>();
            services.AddSingleton<IDataReader<KillDbOutfitEntry>, KillDbOutfitEntryReader>();
            services.AddSingleton<IDataReader<KillEvent>, KillEventReader>();
            services.AddSingleton<IDataReader<ExpEvent>, ExpEventReader>();
            services.AddSingleton<IDataReader<KillItemEntry>, KillItemEntryReader>();
            services.AddSingleton<IDataReader<FacilityControlDbEntry>, FacilityControlDbEntryReader>();

            services.AddSingleton<IDataReader<PsFacilityLink>, PsFacilityLinkReader>();
            services.AddSingleton<IDataReader<PsMapHex>, PsMapHexReader>();
            services.AddSingleton<IDataReader<OutfitPopulation>, OutfitPopulationReader>();
            services.AddSingleton<IDataReader<PsItem>, ItemDbStore>();
            services.AddSingleton<IDataReader<PsCharacter>, CharacterDbStore>();
            services.AddSingleton<IDataReader<CharacterMetadata>, CharacterMetadataReader>();

            services.AddSingleton<IDataReader<LogoutBufferEntry>, LogoutBufferEntryReader>();
            services.AddSingleton<IDataReader<PlayerControlEvent>, PlayerControlEventReader>();
            services.AddSingleton<IDataReader<FacilityControlEvent>, FacilityControlEventReader>();

            services.AddSingleton<IDataReader<CharacterFriend>, CharacterFriendReader>();

            services.AddSingleton<IDataReader<OutfitReport>, OutfitReportReader>();

            services.AddSingleton<IDataReader<PsDirective>, DirectiveReader>();
            services.AddSingleton<IDataReader<DirectiveTree>, DirectiveTreeReader>();
            services.AddSingleton<IDataReader<DirectiveTier>, DirectiveTierReader>();
            services.AddSingleton<IDataReader<DirectiveTreeCategory>, DirectiveTreeCategoryReader>();

            services.AddSingleton<IDataReader<CharacterDirective>, CharacterDirectiveReader>();
            services.AddSingleton<IDataReader<CharacterDirectiveTree>, CharacterDirectiveTreeReader>();
            services.AddSingleton<IDataReader<CharacterDirectiveTier>, CharacterDirectiveTierReader>();
            services.AddSingleton<IDataReader<CharacterDirectiveObjective>, CharacterDirectiveObjectiveReader>();

            services.AddSingleton<IDataReader<PsObjective>, ObjectiveReader>();
            services.AddSingleton<IDataReader<ObjectiveType>, ObjectiveTypeReader>();
            services.AddSingleton<IDataReader<ObjectiveSet>, ObjectiveSetReader>();
        }

    }

}