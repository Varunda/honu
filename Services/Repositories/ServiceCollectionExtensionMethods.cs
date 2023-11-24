using System;
using Microsoft.Extensions.DependencyInjection;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.PSB;
using watchtower.Models.Queues;
using watchtower.Models.Report;
using watchtower.Services.Db.Implementations;
using watchtower.Services.Hosted;
using watchtower.Services.Repositories.PSB;
using watchtower.Services.Repositories.Readers;

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
            services.AddSingleton<CharacterDataRepository>();

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
            services.AddSingleton<IStaticRepository<ExperienceType>, ExperienceTypeRepository>();
            services.AddSingleton<ExperienceTypeRepository>();
            services.AddSingleton<IStaticRepository<PsDirective>, DirectiveRepository>();
            services.AddSingleton<DirectiveRepository>();
            services.AddSingleton<IStaticRepository<DirectiveTree>, DirectiveTreeRepository>();
            services.AddSingleton<DirectiveTreeRepository>();
            //services.AddSingleton<IStaticRepository<DirectiveTier>, DirectiveTierRepository>(); // Directive tier doesn't have a primary key
            services.AddSingleton<DirectiveTierRepository>();
            services.AddSingleton<IStaticRepository<DirectiveTreeCategory>, DirectiveTreeCategoryRepository>();
            services.AddSingleton<DirectiveTreeCategoryRepository>();
            services.AddSingleton<IStaticRepository<FireGroupToFireMode>, FireGroupToFireModeRepository>();
            services.AddSingleton<FireGroupToFireModeRepository>();
            services.AddSingleton<IStaticRepository<PsMetagameEvent>, MetagameEventRepository>();
            services.AddSingleton<MetagameEventRepository>();
            services.AddSingleton<IStaticRepository<ItemType>, ItemTypeRepository>();
            services.AddSingleton<ItemTypeRepository>();

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

            services.AddSingleton<RealtimeAlertRepository>();
            services.AddSingleton<HonuAccountPermissionRepository>();
            services.AddSingleton<PsbDriveRepository>();
            services.AddSingleton<PsbContactSheetRepository>();
            services.AddSingleton<PsbCalendarRepository>();
            services.AddSingleton<ISheetsReader<PsbPracticeContact>, PsbPracticeContactReader>();
            services.AddSingleton<ISheetsReader<PsbOvOContact>, PsbOvOContactReader>();
            services.AddSingleton<ISheetsReader<PsbCalendarEntry>, PsbCalendarEntryReader>();
            services.AddSingleton<PsbReservationRepository>();
            services.AddSingleton<PsbOvOSheetRepository>();
            services.AddSingleton<GDriveRepository>();
            services.AddSingleton<PsbOvOAccountRepository>();
            services.AddSingleton<WrappedSavedCharacterDataFileRepository>();
            services.AddSingleton<HostedWrappedGenerationProcess>();
            services.AddSingleton<RealtimeMapStateRepository>();
        }

    }

}