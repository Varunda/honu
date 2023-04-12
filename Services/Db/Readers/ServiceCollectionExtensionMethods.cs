using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models;
using watchtower.Models.Alert;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Health;
using watchtower.Models.PSB;
using watchtower.Models.Queues;
using watchtower.Models.Report;
using watchtower.Models.Wrapped;
using watchtower.Services.Census.Readers;
using watchtower.Services.Db.Implementations;
using watchtower.Services.Db.Readers.Alert;
using watchtower.Services.Db.Readers.PSB;

namespace watchtower.Services.Db.Readers {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuDatabaseReadersServices(this IServiceCollection services) {
            services.AddSingleton<IDataReader<KillDbEntry>, KillDbEntryReader>();
            services.AddSingleton<IDataReader<KillDbOutfitEntry>, KillDbOutfitEntryReader>();
            services.AddSingleton<IDataReader<KillEvent>, KillEventReader>();
            services.AddSingleton<IDataReader<ExpEvent>, ExpEventReader>();
            services.AddSingleton<IDataReader<KillItemEntry>, KillItemEntryReader>();
            services.AddSingleton<IDataReader<FacilityControlDbEntry>, FacilityControlDbEntryReader>();
            services.AddSingleton<IDataReader<PsItem>, ItemReader>();
            services.AddSingleton<IDataReader<CharacterAchievement>, CharacterAchievementReader>();
            services.AddSingleton<IDataReader<AchievementEarnedEvent>, AchievementEarnedReader>();
            services.AddSingleton<IDataReader<ExperienceType>, ExperienceTypeReader>();

            services.AddSingleton<IDataReader<PsFacilityLink>, PsFacilityLinkReader>();
            services.AddSingleton<IDataReader<PsMapHex>, PsMapHexReader>();
            services.AddSingleton<IDataReader<OutfitPopulation>, OutfitPopulationReader>();
            services.AddSingleton<IDataReader<PsCharacter>, CharacterDbStore>();
            services.AddSingleton<IDataReader<CharacterMetadata>, CharacterMetadataReader>();

            services.AddSingleton<IDataReader<LogoutBufferEntry>, LogoutBufferEntryReader>();
            services.AddSingleton<IDataReader<PlayerControlEvent>, PlayerControlEventReader>();
            services.AddSingleton<IDataReader<FacilityControlEvent>, FacilityControlEventReader>();

            services.AddSingleton<IDataReader<CharacterFriend>, CharacterFriendReader>();

            services.AddSingleton<IDataReader<OutfitReportParameters>, OutfitReportReader>();

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
            services.AddSingleton<IDataReader<Achievement>, AchievementReader>();

            services.AddSingleton<IDataReader<PsbAccount>, PsbNamedReader>();
            services.AddSingleton<IDataReader<PsbAccountNote>, PsbAccountNoteReader>();
            services.AddSingleton<IDataReader<HonuAccount>, HonuAccountReader>();
            services.AddSingleton<IDataReader<VehicleDestroyEvent>, VehicleDestroyEventReader>();
            services.AddSingleton<IDataReader<PsVehicle>, VehicleDataReader>();
            services.AddSingleton<IDataReader<ItemCategory>, ItemCategoryReader>();
            services.AddSingleton<IDataReader<ItemType>, ItemTypeReader>();

            services.AddSingleton<IDataReader<PsAlert>, AlertReader>();
            services.AddSingleton<IDataReader<AlertPlayer>, AlertPlayerReader>();
            services.AddSingleton<IDataReader<AlertPlayerDataEntry>, AlertPlayerDataReader>();
            services.AddSingleton<IDataReader<AlertPlayerProfileData>, AlertPlayerProfileDataReader>();
            services.AddSingleton<IDataReader<AlertPopulation>, AlertPopulationDataReader>();

            services.AddSingleton<IDataReader<WorldTagEntry>, WorldTagEntryReader>();

            services.AddSingleton<IDataReader<RealtimeReconnectEntry>, RealtimeReconnectReader>();
            services.AddSingleton<IDataReader<OutfitActivityDbEntry>, OutfitActivityDbEntryReader>();
            services.AddSingleton<IDataReader<WeaponStatSnapshot>, WeaponStatSnapshotReader>();

            services.AddSingleton<IDataReader<HonuAccountPermission>, HonuAccountPermissionReader>();
            services.AddSingleton<IDataReader<WeaponStatBucket>, WeaponStatBucketReader>();
            services.AddSingleton<IDataReader<WeaponStatTop>, WeaponStatTopReader>();
            services.AddSingleton<IDataReader<PopulationEntry>, PopulationReader>();
            services.AddSingleton<IDataReader<PopulationCount>, PopulationCountReader>();

            services.AddSingleton<IDataReader<SessionEndSubscription>, SessionEndSubscriptionReader>();
            services.AddSingleton<IDataReader<ItemAddedEvent>, ItemAddedEventReader>();
            services.AddSingleton<IDataReader<AlertEndSubscription>, AlertEndSubscriptionReader>();
            services.AddSingleton<IDataReader<ContinentLockEntry>, ContinentLockEntryReader>();
            services.AddSingleton<IDataReader<PsbParsedReservationMetadata>, PsbParsedReservationReader>();
            services.AddSingleton<IDataReader<WrappedEntry>, WrappedEntryReader>();
            services.AddSingleton<IDataReader<FireGroupToFireMode>, FireGroupToFireModeReader>();
        }

    }

}