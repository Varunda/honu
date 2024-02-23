using watchtower.Models.Census;

namespace watchtower.Code.Constants {

    /// <summary>
    ///     pulled from: https://census.daybreakgames.com/s:example/get/ps2:v2/experience_award_type?c:limit=1000
    /// </summary>
    public sealed class ExperienceAwardTypes {

        public const int KILL = 1;

        public const int KILL_ASSIST = 2;

        public const int SPAWN_KILL_ASSIST = 3;

        public const int HEAL = 4;

        public const int HEAL_ASSIST = 5;

        /// <summary>
        ///     this is any kinda repair, MAX, vehicle, terminal, containment site gate shield
        /// </summary>
        public const int REPAIR = 6;

        public const int REVIVE = 9;

        public const int KILL_STREAK = 10;

        /// <summary>
        ///     this is by far the most useful ID, as it contains all the <see cref="ExperienceType"/>
        /// </summary>
        public const int GUNNER_KILL = 35;

        public const int DEPLOY_KILL = 36;

        public const int ROAD_KILL = 37;

        /// <summary>
        ///     this is any kinda repair, MAX, vehicle, terminal, containment site gate shield
        /// </summary>
        public const int SQUAD_REPAIR = 40;

        /// <summary>
        ///     these types of <see cref="ExperienceType"/>s occur when a gunner of a vehicle gets a kill share. but it doesn't tell us what vehicle they're in
        /// </summary>
        public const int VEHICLE_DAMAGE = 62;

        public const int VEHICLE_RADAR_KILL = 69;

        public const int SQUAD_VEHICLE_RADAR_KILL = 70;

        /// <summary>
        ///     these types of <see cref="ExperienceType"/>s occur when a gunner of a vehicle gets a kill share. but it doesn't tell us what vehicle they're in
        /// </summary>
        public const int VEHICLE_SHARE = 73;

    }
}
