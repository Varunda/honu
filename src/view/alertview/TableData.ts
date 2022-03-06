import { FlattendParticipantDataEntry, AlertPlayerProfileData } from "api/AlertParticipantApi";

export class BaseTableData {
    public id: string = "";
    public name: string = "";
    public outfitID: string | null = null;
    public outfitTag: string | null = null;
    public factionID: number = 0;

    public kills: number = 0;
    public deaths: number = 0;
    public timeAs: number = 0;

    public kd: number = 0;
    public kpm: number = 0;
}

export class MedicTableData extends BaseTableData {
    public heals: number = 0;
    public revives: number = 0;

    public healsPerMinute: number = 0;
    public revivesPerMinute: number = 0;
    public krd: number = 0;
}

export class EngineerTableData extends BaseTableData {
    public resupplies: number = 0;
    public repairs: number = 0;
    public resuppliesPerMinute: number = 0;
    public repairsPerMinute: number = 0;
}

export class TableData {

    public static getMedicData(data: FlattendParticipantDataEntry[]): MedicTableData[] {
        return data.filter(iter => {
            const medicProfile: AlertPlayerProfileData | undefined = iter.profiles.find(iter => iter.profileID == 4);
            return medicProfile != undefined && medicProfile.timeAs >= 60;
        }).map(iter => {
            const medicProfile: AlertPlayerProfileData = iter.profiles.find(iter => iter.profileID == 4)!; // must exist cause of check above

            return {
                id: iter.characterID,
                name: iter.characterName,
                outfitID: iter.outfitID,
                outfitTag: iter.outfitTag,
                factionID: iter.factionID,

                kills: medicProfile.kills,
                deaths: medicProfile.deaths,
                timeAs: medicProfile.timeAs,
                vehicleKills: medicProfile.vehicleKills,

                heals: iter.heals,
                revives: iter.revives,
                kd: medicProfile.kills / Math.max(1, medicProfile.deaths),
                kpm: medicProfile.kills / Math.max(1, medicProfile.timeAs) * 60,
                healsPerMinute: iter.heals / Math.max(1, medicProfile.timeAs) * 60,
                revivesPerMinute: iter.revives / Math.max(1, medicProfile.timeAs) * 60,
                krd: (medicProfile.kills + iter.revives) / Math.max(1, medicProfile.deaths)
            };
        });
    }

    public static getEngineerData(data: FlattendParticipantDataEntry[]): EngineerTableData[] {
        return data.filter(iter => {
            const profile: AlertPlayerProfileData | undefined = iter.profiles.find(iter => iter.profileID == 4);
            return profile != undefined && profile.timeAs >= 60;
        }).map(iter => {
            const profile: AlertPlayerProfileData = iter.profiles.find(iter => iter.profileID == 4)!; // must exist cause of check above

            return {
                id: iter.characterID,
                name: iter.characterName,
                outfitID: iter.outfitID,
                outfitTag: iter.outfitTag,
                factionID: iter.factionID,

                kills: profile.kills,
                deaths: profile.deaths,
                timeAs: profile.timeAs,
                vehicleKills: profile.vehicleKills,
                kd: profile.kills / Math.max(1, profile.deaths),
                kpm: profile.kills / Math.max(1, profile.timeAs) * 60,

                resupplies: iter.resupplies,
                repairs: iter.repairs,
                resuppliesPerMinute: iter.resupplies / Math.max(1, profile.timeAs) * 60,
                repairsPerMinute: iter.repairs / Math.max(1, profile.timeAs) * 60
            };
        });
    }

}
