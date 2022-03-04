import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";
import { CharacterApi, PsCharacter } from "api/CharacterApi";
import { OutfitApi, PsOutfit } from "api/OutfitApi";
import { Session, SessionApi } from "api/SessionApi";
import { Alert } from "bootstrap";

export class AlertParticipantDataEntry {
    public id: number = 0;
    public alertID: number = 0;
    public characterID: string = "";
    public outfitID: string | null = null;
    public secondsOnline: number = 0;

    public kills: number = 0;
    public deaths: number = 0;
    public vehicleKills: number = 0;

    public heals: number = 0;
    public revives: number = 0;
    public shieldRepairs: number = 0;
    public resupplies: number = 0;
    public spawns: number = 0;
    public repairs: number = 0;
}

export class ExpandedAlertParticipants {
    public entries: AlertParticipantDataEntry[] = [];
    public characters: Map<string, PsCharacter> = new Map();
    public outfits: Map<string, PsOutfit> = new Map();
}

export class FlattendParticipantDataEntry {
    public entry: AlertParticipantDataEntry = new AlertParticipantDataEntry();
    public characterID: string = "";
    public characterName: string = "";
    public factionID: number = 0;

    public outfitID: string | null = null;
    public outfitTag: string | null = null;
    public secondsOnline: number = 0;

    public kills: number = 0;
    public deaths: number = 0;
    public vehicleKills: number = 0;

    public heals: number = 0;
    public healsPerMinute: number = 0;
    public revives: number = 0;
    public revivesPerMinute: number = 0;
    public shieldRepairs: number = 0;
    public shieldRepairsPerMinute: number = 0;
    public resupplies: number = 0;
    public resuppliesPerMinute: number = 0;
    public repairs: number = 0;
    public repairsPerMinute: number = 0;
    public spawns: number = 0;
    public spawnsPerMinute: number = 0;

    public kpm: number = 0;
    public kd: number = 0;
}

export class AlertParticipantApi extends ApiWrapper<AlertParticipantDataEntry> {
    private static _instance: AlertParticipantApi = new AlertParticipantApi();
    public static get(): AlertParticipantApi { return AlertParticipantApi._instance; }

    public static readEntry(elem: any): AlertParticipantDataEntry {
        return {
            ...elem
        };
    }

    public static readExpanded(elem: any): ExpandedAlertParticipants {
        const expanded: ExpandedAlertParticipants = new ExpandedAlertParticipants();

        expanded.entries = elem.entries.map((iter: any) => AlertParticipantApi.readEntry(iter));

        const characters: PsCharacter[] = elem.characters.map((iter: any) => CharacterApi.parse(iter));
        for (const c of characters) {
            expanded.characters.set(c.id, c);
        }

        const outfits: PsOutfit[] = elem.outfits.map((iter: any) => OutfitApi.parse(iter));
        for (const o of outfits) {
            expanded.outfits.set(o.id, o);
        }

        return expanded;
    }

    public static async getByAlertID(alertID: number): Promise<Loading<FlattendParticipantDataEntry[]>> {
        const expanded: Loading<ExpandedAlertParticipants> = await AlertParticipantApi.get().readSingle(`/api/alerts/${alertID}/participants`, AlertParticipantApi.readExpanded);

        if (expanded.state == "loaded") {
            const entries: FlattendParticipantDataEntry[] = [];

            for (const entry of expanded.data.entries) {
                if (entry.characterID == "0") {
                    continue;
                }

                const character: PsCharacter | null = expanded.data.characters.get(entry.characterID) || null;
                const outfit: PsOutfit | null = (entry.outfitID == null) ? null : expanded.data.outfits.get(entry.outfitID) || null;

                const flat: FlattendParticipantDataEntry = {
                    ...entry,
                    entry: entry,
                    characterName: character?.name ?? `<missing ${entry.characterID}>`,
                    factionID: character?.factionID ?? -1,
                    outfitTag: outfit?.tag ?? null,

                    kpm: entry.kills / Math.max(1, entry.secondsOnline) * 60,
                    kd: entry.kills / Math.max(1, entry.deaths),
                    healsPerMinute: entry.heals / Math.max(1, entry.secondsOnline) * 60,
                    revivesPerMinute: entry.revives / Math.max(1, entry.secondsOnline) * 60,
                    shieldRepairsPerMinute: entry.shieldRepairs / Math.max(1, entry.secondsOnline) * 60,
                    repairsPerMinute: entry.repairs / Math.max(1, entry.secondsOnline) * 60,
                    resuppliesPerMinute: entry.resupplies / Math.max(1, entry.secondsOnline) * 60,
                    spawnsPerMinute: entry.spawns / Math.max(1, entry.secondsOnline) * 60,
                };

                entries.push(flat);
            }

            return Loadable.loaded(entries);
        } else {
            throw `aasdflakjdf`;
            return expanded as any;
        }
    }

}