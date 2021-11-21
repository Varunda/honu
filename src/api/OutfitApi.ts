import * as axios from "axios";

import { PsCharacter, CharacterApi } from "api/CharacterApi";
import { CharacterHistoryStat, CharacterHistoryStatApi } from "api/CharacterHistoryStatApi";

export class PsOutfit {
	public id: string = "";
	public name: string = "";
	public tag: string | null = null;
	public factionID: number = 0;
	public timestamp: Date = new Date();
	public dateCreated: Date = new Date();
	public leaderID: string = "";
	public memberCount: number = 0;
}

export class OutfitMember {
	public characterID: string = "";
	public outfitID: string = "";
	public memberSince: Date = new Date();
	public rank: string = "";
	public rankOrder: number = 0;
	public worldID: number | null = null;
}

export class ExpandedOutfitMember {
	public member: OutfitMember = new OutfitMember();
	public character: PsCharacter | null = null;
	public stats: CharacterHistoryStat[] = [];
}

export class FlatExpandedOutfitMember {
	public characterID: string = "";
	public outfitID: string = "";
	public memberSince: Date = new Date();
	public rank: string = "";
	public rankOrder: number = 0;
	public worldID: number | null = null;

	public outfitTag: string | null = null;
	public outfitName: string | null = null;
	public prestige: number | null = null;
	public battleRank: number | null = null;
	public prestigeRank: string | null = null;
	public name: string | null = null;
	public lastLogin: Date | null = null;

	public recentKD: number | null = null;
	public recentKPM: number | null = null;
	public recentSPM: number | null = null;
}

export class OutfitApi {
	private static _instance: OutfitApi = new OutfitApi();
	public static get(): OutfitApi { return OutfitApi._instance; }

	public static parse(elem: any): PsOutfit {
		return {
			...elem,
			timestamp: new Date(elem.lastUpdated),
			dateCreated: new Date(elem.dateCreated)
		};
	}

	public static parseOutfitMember(elem: any): OutfitMember {
		return {
			...elem,
			memberSince: new Date(elem.memberSince)
		};
	};

	public static parseExpandedOutfitMember(elem: any): ExpandedOutfitMember {
		return {
			member: OutfitApi.parseOutfitMember(elem.member),
			character: elem.character == null ? null : CharacterApi.parse(elem.character),
			stats: elem.stats == null ? null : elem.stats.map((iter: any) => CharacterHistoryStatApi.parse(iter))
		};
	}

	public static flattenExpandedOutfitMember(entry: ExpandedOutfitMember): FlatExpandedOutfitMember {
		const flat: FlatExpandedOutfitMember = new FlatExpandedOutfitMember();

		flat.characterID = entry.member.characterID;
		flat.outfitID = entry.member.outfitID;
		flat.memberSince = entry.member.memberSince;
		flat.rank = entry.member.rank;
		flat.rankOrder = entry.member.rankOrder;
		flat.worldID = entry.member.worldID;

		if (entry.character != null) {
			flat.outfitTag = entry.character.outfitTag;
			flat.outfitName = entry.character.outfitName;
			flat.prestige = entry.character.prestige;
			flat.battleRank = entry.character.battleRank;
			flat.name = entry.character.name;
			flat.prestigeRank = `${entry.character.prestige}~${entry.character.battleRank}`;
			flat.lastLogin = entry.character.dateLastLogin;
		}

		if (entry.stats != null && entry.stats.length > 0) {
			const kills: CharacterHistoryStat | null = entry.stats.find(iter => iter.type == "kills") || null;
			const deaths: CharacterHistoryStat | null = entry.stats.find(iter => iter.type == "deaths") || null;
			const score: CharacterHistoryStat | null = entry.stats.find(iter => iter.type == "score") || null;
			const time: CharacterHistoryStat | null = entry.stats.find(iter => iter.type == "time") || null;

			function reduce(arr: number[]): number {
				return arr.reduce((acc, iter) => acc += iter, 0);
			};

			if (kills != null && deaths != null) {
				flat.recentKD = reduce(kills.days) / Math.max(1, reduce(deaths.days));
			}
			if (kills != null && time != null) {
				flat.recentKPM = reduce(kills.days) / Math.max(1, reduce(time.days)) * 60;
			}
			if (score != null && time != null) {
				flat.recentSPM = reduce(score.days) / Math.max(1, reduce(time.days)) * 60;
			}
		}

		return flat;
	}

	public static async getByID(outfitID: string): Promise<PsOutfit | null> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/outfit/${outfitID}`);

		if (response.status != 200) {
			return null;
		}

		return OutfitApi.parse(response.data);
	}

	public static async getMembers(outfitID: string): Promise<ExpandedOutfitMember[]> {
		const response: axios.AxiosResponse = await axios.default.get(`/api/outfit/${outfitID}/members`);

		if (response.status != 200) {
			throw response.data;
		}

		return response.data.map((iter: any) => OutfitApi.parseExpandedOutfitMember(iter));
	}

	public static async getMembersFlat(outfitID: string): Promise<FlatExpandedOutfitMember[]> {
		return (await OutfitApi.getMembers(outfitID)).map(iter => OutfitApi.flattenExpandedOutfitMember(iter));
	}

}