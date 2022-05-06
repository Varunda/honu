import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { CharacterApi, PsCharacter } from "api/CharacterApi";

export class CharacterFriend {
    public characterID: string = "";
    public friendID: string = "";
}

export class ExpandedCharacterFriend {
    public entry: CharacterFriend = new CharacterFriend();
    public friend: PsCharacter | null = null;
}

export class FlatExpandedCharacterFriend {
    public characterID: string = "";
    public friendID: string = "";
    public friendName: string | null = null;
    public friendOutfitID: string | null = null;
    public friendOutfitTag: string | null = null;
    public friendOutfitName: string | null = null;
    public friendFactionID: number | null = null;
    public friendWorldID: number | null = null;
    public friendDateLastLogin: Date | null = null;
}

export class CharacterFriendApi extends ApiWrapper<CharacterFriend> {
    private static _instance: CharacterFriendApi = new CharacterFriendApi();
    public static get(): CharacterFriendApi { return CharacterFriendApi._instance; }

    public static parse(elem: any): CharacterFriend  {
        return {
            characterID: elem.characterID,
            friendID: elem.friendID
        };
    }

    public static parseExpanded(elem: any): ExpandedCharacterFriend {
        return {
            entry: CharacterFriendApi.parse(elem.entry),
            friend: (elem.friend != null) ? CharacterApi.parse(elem.friend) : null
        };
    }

    public static parseFlat(elem: any): FlatExpandedCharacterFriend {
        const ex: ExpandedCharacterFriend = CharacterFriendApi.parseExpanded(elem);

        return {
            characterID: ex.entry.characterID,
            friendID: ex.entry.friendID,
            friendName: ex.friend?.name ?? null,
            friendOutfitID: ex.friend?.outfitID ?? null,
            friendOutfitTag: ex.friend?.outfitTag ?? null,
            friendOutfitName: ex.friend?.outfitName ?? null,
            friendFactionID: ex.friend?.factionID ?? null,
            friendWorldID: ex.friend?.worldID ?? null,
            friendDateLastLogin: ex.friend?.dateLastLogin ?? null
        };
    }

    public static getByCharacterID(charID: string, fast: boolean = false): Promise<Loading<FlatExpandedCharacterFriend[]>> {
        return CharacterFriendApi.get().readList(`/api/character/${charID}/friends?fast=${fast}`, CharacterFriendApi.parseFlat);
    }

}