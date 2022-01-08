import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { CharacterApi, PsCharacter } from "api/CharacterApi";

export class PsbNamedAccount {
    public tag: string | null = null;
    public name: string = "";
    public vsID: string | null = null;
    public ncID: string | null = null;
    public trID: string | null = null;
    public nsID: string | null = null;
    public notes: string | null = null;
}

export class ExpandedPsbNamedAccount {
    public account: PsbNamedAccount = new PsbNamedAccount();
    public vsCharacter: PsCharacter | null = null;
    public ncCharacter: PsCharacter | null = null;
    public trCharacter: PsCharacter | null = null;
    public nsCharacter: PsCharacter | null = null;
}

export class FlatPsbNamedAccount {
    public tag: string | null = null;
    public name: string = "";
    public lastUsed: Date | null = null;
    public missingCharacter: boolean = false;

    public vsID: string | null = null;
    public vsCharacter: PsCharacter | null = null;
    public vsName: string | null = null;
    public vsOutfitID: string | null = null;
    public vsOutfitTag: string | null = null;
    public vsOutfitName: string | null = null;
    public vsBattleRank: number | null = null;
    public vsPrestige: number | null = null;
    public vsLastLogin: Date | null = null;

    public ncID: string | null = null;
    public ncCharacter: PsCharacter | null = null;
    public ncName: string | null = null;
    public ncOutfitID: string | null = null;
    public ncOutfitTag: string | null = null;
    public ncOutfitName: string | null = null;
    public ncBattleRank: number | null = null;
    public ncPrestige: number | null = null;
    public ncLastLogin: Date | null = null;

    public trID: string | null = null;
    public trCharacter: PsCharacter | null = null;
    public trName: string | null = null;
    public trOutfitID: string | null = null;
    public trOutfitTag: string | null = null;
    public trOutfitName: string | null = null;
    public trBattleRank: number | null = null;
    public trPrestige: number | null = null;
    public trLastLogin: Date | null = null;

    public nsID: string | null = null;
    public nsCharacter: PsCharacter | null = null;
    public nsName: string | null = null;
    public nsOutfitID: string | null = null;
    public nsOutfitTag: string | null = null;
    public nsOutfitName: string | null = null;
    public nsBattleRank: number | null = null;
    public nsPrestige: number | null = null;
    public nsLastLogin: Date | null = null;

}

export class PsbNamedAccountApi extends ApiWrapper<PsbNamedAccount> {
    private static _instance: PsbNamedAccountApi = new PsbNamedAccountApi();
    public static get(): PsbNamedAccountApi { return PsbNamedAccountApi._instance; }

    public static parse(entry: any): PsbNamedAccount {
        return {
            tag: entry.tag,
            name: entry.name,
            vsID: entry.vsID,
            ncID: entry.ncID,
            trID: entry.trID,
            nsID: entry.nsID,
            notes: entry.notes,
        };
    }

    public static parseExpanded(elem: any): ExpandedPsbNamedAccount {
        return {
            account: PsbNamedAccountApi.parse(elem.account),
            vsCharacter: (elem.vsCharacter != null) ? CharacterApi.parse(elem.vsCharacter) : null,
            ncCharacter: (elem.ncCharacter != null) ? CharacterApi.parse(elem.ncCharacter) : null,
            trCharacter: (elem.trCharacter != null) ? CharacterApi.parse(elem.trCharacter) : null,
            nsCharacter: (elem.nsCharacter != null) ? CharacterApi.parse(elem.nsCharacter) : null,
        };
    }

    public static parseFlat(elem: any): FlatPsbNamedAccount {

        const expanded: ExpandedPsbNamedAccount = PsbNamedAccountApi.parseExpanded(elem);

        const flat: FlatPsbNamedAccount = {
            tag: expanded.account.tag,
            name: expanded.account.name,
            lastUsed: null, 
            missingCharacter: expanded.account.vsID == null || expanded.vsCharacter == null
                || expanded.account.ncID == null || expanded.ncCharacter == null
                || expanded.account.trID == null || expanded.trCharacter == null
                || expanded.account.nsID == null || expanded.nsCharacter == null,

            vsID: expanded.account.vsID,
            vsCharacter: expanded.vsCharacter ?? null,
            vsName: expanded.vsCharacter?.name ?? null,
            vsOutfitID: expanded.vsCharacter?.outfitID ?? null,
            vsOutfitTag: expanded.vsCharacter?.outfitTag ?? null,
            vsOutfitName: expanded.vsCharacter?.outfitName ?? null,
            vsBattleRank: expanded.vsCharacter?.battleRank ?? null,
            vsPrestige: expanded.vsCharacter?.prestige ?? null,
            vsLastLogin: expanded.vsCharacter?.dateLastLogin ?? null,

            ncID: expanded.account.ncID,
            ncCharacter: expanded.ncCharacter ?? null,
            ncName: expanded.ncCharacter?.name ?? null,
            ncOutfitID: expanded.ncCharacter?.outfitID ?? null,
            ncOutfitTag: expanded.ncCharacter?.outfitTag ?? null,
            ncOutfitName: expanded.ncCharacter?.outfitName ?? null,
            ncBattleRank: expanded.ncCharacter?.battleRank ?? null,
            ncPrestige: expanded.ncCharacter?.prestige ?? null,
            ncLastLogin: expanded.ncCharacter?.dateLastLogin ?? null,

            trID: expanded.account.trID,
            trCharacter: expanded.trCharacter ?? null,
            trName: expanded.trCharacter?.name ?? null,
            trOutfitID: expanded.trCharacter?.outfitID ?? null,
            trOutfitTag: expanded.trCharacter?.outfitTag ?? null,
            trOutfitName: expanded.trCharacter?.outfitName ?? null,
            trBattleRank: expanded.trCharacter?.battleRank ?? null,
            trPrestige: expanded.trCharacter?.prestige ?? null,
            trLastLogin: expanded.trCharacter?.dateLastLogin ?? null,

            nsID: expanded.account.nsID,
            nsCharacter: expanded.nsCharacter ?? null,
            nsName: expanded.nsCharacter?.name ?? null,
            nsOutfitID: expanded.nsCharacter?.outfitID ?? null,
            nsOutfitTag: expanded.nsCharacter?.outfitTag ?? null,
            nsOutfitName: expanded.nsCharacter?.outfitName ?? null,
            nsBattleRank: expanded.nsCharacter?.battleRank ?? null,
            nsPrestige: expanded.nsCharacter?.prestige ?? null,
            nsLastLogin: expanded.nsCharacter?.dateLastLogin ?? null,
        };

        // expanded.vsCharacter?.dateLastLogin ?? expanded.ncCharacter?.dateLastLogin ?? expanded.trCharacter?.dateLastLogin ?? null,

        if ((expanded.vsCharacter?.dateLastLogin ?? null) == null && (expanded.ncCharacter?.dateLastLogin ?? null) == null && (expanded.trCharacter?.dateLastLogin ?? null) == null) {

        } else {
            const dates: number[] = [
                (expanded.vsCharacter?.dateLastLogin.getTime() ?? 0),
                (expanded.ncCharacter?.dateLastLogin.getTime() ?? 0),
                (expanded.trCharacter?.dateLastLogin.getTime() ?? 0),
            ];

            const mostRecent: Date = new Date(Math.max(...dates));
            flat.lastUsed = mostRecent;
        }

        return flat;
    }

    public static getAll(): Promise<Loading<FlatPsbNamedAccount[]>> {
        return PsbNamedAccountApi.get().readList(`/api/psb-named/`, PsbNamedAccountApi.parseFlat);
    }

}
