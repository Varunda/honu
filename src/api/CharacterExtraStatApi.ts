import ApiWrapper from "api/ApiWrapper";
import { Loading, Loadable } from "Loading";

export class CharacterStatBase {
    public name: string = "";
    public description: string | null = null;
    public figuredOutBy: string | null = null;
    public figuredOutOn: Date | null = null;
    public value: number = 0;
}

export class ExtraStatSet {
    public name: string = "";
    public description: string = "";
    public stats: CharacterStatBase[] = [];
}

export class CharacterExtraStatApi extends ApiWrapper<ExtraStatSet> {
    private static _instance: CharacterExtraStatApi = new CharacterExtraStatApi();
    public static get(): CharacterExtraStatApi { return CharacterExtraStatApi._instance; }

    public static parseSet(elem: any): ExtraStatSet {
        return {
            name: elem.name,
            description: elem.description,
            stats: elem.stats.map((iter: any) => CharacterExtraStatApi.parseStat(iter))
        };
    }

    public static parseStat(elem: any): CharacterStatBase {
        return {
            ...elem,
            figuredOutOn: elem.figuredOutOn == null ? null : new Date(elem.figuredOutOn)
        };
    }

    public static getByCharacterID(charID: string): Promise<Loading<ExtraStatSet[]>> {
        return CharacterExtraStatApi.get().readList(`/api/character/${charID}/extra`, CharacterExtraStatApi.parseSet);
    }

}
