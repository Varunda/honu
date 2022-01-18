using System;

namespace watchtower.Models.Events {

    /// <summary>
    ///     Represents data from the realtime vehicle_destroy event
    /// </summary>
    public class VehicleDestroyEvent {

        /*
            id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,

            attacker_character_id varchar NOT NULL,
            attacker_vehicle_id varchar NOT NULL,
            attacker_weapon_id int NOT NULL,
            attacker_loadout_id smallint NOT NULL,
            attacker_team_id smallint NOT NULL,
            attacker_faction_id smallint NOT NULL,

            killed_character_id varchar NOT NULL,
            killed_faction_id smallint NOT NULL,
            killed_team_id smallint NOT NULL,
            killed_vehicle_id varchar NOT NULL,

            facility_id string NOT NULL,
            world_id smallint NOT NULL,
            zone_id int NOT NULL,
            timestamp timestamptz NOT NULL
        */

        public long ID { get; set; }

        public string AttackerCharacterID { get; set; } = "";

        public string AttackerVehicleID { get; set; } = "";

        public int AttackerWeaponID { get; set; }

        public short AttackerLoadoutID { get; set; }

        public short AttackerFactionID { get; set; }

        public short AttackerTeamID { get; set; }

        public string KilledCharacterID { get; set; } = "";

        public short KilledFactionID { get; set; }

        public short KilledTeamID { get; set; }

        public string KilledVehicleID { get; set; } = "";

        public string FacilityID { get; set; } = "";

        public short WorldID { get; set; }

        public uint ZoneID { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
