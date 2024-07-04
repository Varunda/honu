using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Queues;
using watchtower.Models.Report;
using watchtower.Services.Db.Implementations;

namespace watchtower.Services.Db {

    public static class ServiceCollectionExtensionMethods {

        public static void AddHonuDatabasesServices(this IServiceCollection services) {
            services.AddSingleton<CharacterDbStore>();
            services.AddSingleton<OutfitDbStore>();

            services.AddSingleton<KillEventDbStore>();
            services.AddSingleton<ExpEventDbStore, ExpEventDbStore>();
            services.AddSingleton<VehicleDestroyDbStore>();
            services.AddSingleton<IWorldTotalDbStore, WorldTotalDbStore>();
            services.AddSingleton<ItemAddedDbStore>();
            services.AddSingleton<AchievementEarnedDbStore>();

            services.AddSingleton<SessionDbStore, SessionDbStore>();
            services.AddSingleton<FacilityControlDbStore>();
            services.AddSingleton<IFacilityDbStore, FacilityDbStore>();
            services.AddSingleton<IMapDbStore, MapDbStore>();
            services.AddSingleton<CharacterWeaponStatDbStore>();
            services.AddSingleton<IWeaponStatPercentileCacheDbStore, WeaponStatPercentileCacheDbStore>();
            services.AddSingleton<CharacterHistoryStatDbStore, CharacterHistoryStatDbStore>();
            services.AddSingleton<CharacterItemDbStore>();
            services.AddSingleton<CharacterStatDbStore>();
            services.AddSingleton<IBattleRankDbStore, BattleRankDbStore>();
            services.AddSingleton<OutfitReportParameterDbStore>();
            services.AddSingleton<CharacterMetadataDbStore>();
            services.AddSingleton<LogoutBufferDbStore>();
            services.AddSingleton<FacilityPlayerControlDbStore>();
            services.AddSingleton<CharacterFriendDbStore>();
            services.AddSingleton<CharacterDirectiveDbStore>();
            services.AddSingleton<CharacterDirectiveTreeDbStore>();
            services.AddSingleton<CharacterDirectiveTierDbStore>();
            services.AddSingleton<CharacterDirectiveObjectiveDbStore>();
            services.AddSingleton<CharacterAchievementDbStore>();
            services.AddSingleton<CharacterNameChangeDbStore>();

            // static data
            services.AddSingleton<IStaticDbStore<PsItem>, ItemDbStore>();
            services.AddSingleton<ItemDbStore>();
            services.AddSingleton<IStaticDbStore<PsObjective>, ObjectiveDbStore>();
            services.AddSingleton<ObjectiveDbStore>();
            services.AddSingleton<IStaticDbStore<ObjectiveType>, ObjectiveTypeDbStore>();
            services.AddSingleton<ObjectiveTypeDbStore>();
            services.AddSingleton<IStaticDbStore<ObjectiveSet>, ObjectiveSetDbStore>();
            services.AddSingleton<ObjectiveSetDbStore>();
            services.AddSingleton<IStaticDbStore<Achievement>, AchievementDbStore>();
            services.AddSingleton<AchievementDbStore>();
            services.AddSingleton<IStaticDbStore<PsVehicle>, VehicleDbStore>();
            services.AddSingleton<VehicleDbStore>();
            services.AddSingleton<IStaticDbStore<ItemType>, ItemTypeDbStore>();
            services.AddSingleton<ItemTypeDbStore>();
            services.AddSingleton<IStaticDbStore<ItemCategory>, ItemCategoryDbStore>();
            services.AddSingleton<ItemCategoryDbStore>();
            services.AddSingleton<IStaticDbStore<ExperienceType>, ExperienceTypeDbStore>();
            services.AddSingleton<ExperienceTypeDbStore>();
            services.AddSingleton<IStaticDbStore<FireGroupToFireMode>, FireGroupToFireModeDbStore>();
            services.AddSingleton<FireGroupToFireModeDbStore>();
            services.AddSingleton<IStaticDbStore<PsDirective>, DirectiveDbStore>();
            services.AddSingleton<DirectiveDbStore>();
            services.AddSingleton<IStaticDbStore<DirectiveTree>, DirectiveTreeDbStore>();
            services.AddSingleton<DirectiveTreeDbStore>();
            services.AddSingleton<IStaticDbStore<DirectiveTier>, DirectiveTierDbStore>();
            services.AddSingleton<DirectiveTierDbStore>();
            services.AddSingleton<IStaticDbStore<DirectiveTreeCategory>, DirectiveTreeCategoryDbStore>();
            services.AddSingleton<DirectiveTreeCategoryDbStore>();
            services.AddSingleton<IStaticDbStore<PsMetagameEvent>, MetagameEventDbStore>();
            services.AddSingleton<MetagameEventDbStore>();
            services.AddSingleton<IStaticDbStore<ExperienceAwardType>, ExperienceAwardTypeDbStore>();
            services.AddSingleton<ExperienceAwardTypeDbStore>();

            services.AddSingleton<PsbAccountDbStore>();
            services.AddSingleton<PsbAccountNoteDbStore>();
            services.AddSingleton<HonuAccountDbStore>();
            services.AddSingleton<HonuAccountAccessLogDbStore>();

            services.AddSingleton<AlertDbStore>();
            services.AddSingleton<AlertPlayerDataDbStore>();
            services.AddSingleton<AlertPlayerProfileDataDbStore>();
            services.AddSingleton<AlertPopulationDbStore>();

            services.AddSingleton<WorldTagDbStore>();
            services.AddSingleton<RealtimeReconnectDbStore>();

            services.AddSingleton<WeaponStatSnapshotDbStore>();
            services.AddSingleton<HonuAccountPermissionDbStore>();
            services.AddSingleton<WeaponStatBucketDbStore>();
            services.AddSingleton<WeaponStatTopDbStore>();
            services.AddSingleton<PopulationDbStore>();
            services.AddSingleton<SessionEndSubscriptionDbStore>();
            services.AddSingleton<AlertEndSubscriptionDbStore>();
            services.AddSingleton<ContinentLockDbStore>();
            services.AddSingleton<PsbParsedReservationDbStore>();
            services.AddSingleton<WrappedDbStore>();
            services.AddSingleton<RealtimeMapStateDbStore>();
            services.AddSingleton<WorldZonePopulationDbStore>();
            services.AddSingleton<HonuMetadataDbStore>();
            services.AddSingleton<VehicleUsageDbStore>();
        }

    }

}