import { Loading } from "../Loading";
import ApiWrapper from "./ApiWrapper";
import { CharacterApi, PsCharacter } from "api/CharacterApi";
import { CharacterHistoryStat, CharacterHistoryStatApi } from "api/CharacterHistoryStatApi";

export class KillboardEntry {
    public sourceCharacterID: string = "";
    public otherCharacterID: string = "";
    public kills: number = 0;
    public deaths: number = 0;
}

export class ExpandedKillboardEntry {
    public entry: KillboardEntry = new KillboardEntry();
    public character: PsCharacter | null = null;
    public stats: CharacterHistoryStat[] | null = null;
}

export class KillboardApi extends ApiWrapper<KillboardEntry> {

    private static _instance: KillboardApi = new KillboardApi();
    public static get(): KillboardApi { return KillboardApi._instance; }

    public static readEntry(elem: any): KillboardEntry {
        return {
            ...elem
        };
    }

    public static readExpanded(elem: any): ExpandedKillboardEntry {
        return {
            entry: KillboardApi.readEntry(elem.entry),
            character: (elem.character == null) ? null : CharacterApi.parse(elem.character),
            stats: (elem.stats == null) ? null : elem.stats.map((iter: any) => CharacterHistoryStatApi.parse(iter))
        }
    }

    public static getExpandedByCharacterID(charID: string, includeStats: boolean = false): Promise<Loading<ExpandedKillboardEntry[]>> {
        return KillboardApi.get().readList(`/api/character/${charID}/killboard/expanded?includeStats=${includeStats}`, KillboardApi.readExpanded);
    }

}
