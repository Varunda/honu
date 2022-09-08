import * as axios from "axios";
import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

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
	public online: boolean = false;
}

export class FlatExpandedOutfitMember {
	public characterID: string = "";
	public outfitID: string = "";
	public memberSince: Date = new Date();
	public rank: string = "";
	public rankOrder: number = 0;
	public worldID: number | null = null;
	public online: boolean = false;

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

export class OutfitActivity {
	public outfitID: string = "";
	public timestamp: Date = new Date();
	public count: number = 0;
}

export class OutfitApi extends ApiWrapper<PsOutfit> {
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
			stats: elem.stats == null ? null : elem.stats.map((iter: any) => CharacterHistoryStatApi.parse(iter)),
			online: elem.online
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
		flat.online = entry.online;

		if (entry.character != null) {
			flat.outfitTag = entry.character.outfitTag;
			flat.outfitName = entry.character.outfitName;
			flat.prestige = entry.character.prestige;
			flat.battleRank = entry.character.battleRank;
			flat.name = entry.character.name;
			flat.prestigeRank = `${entry.character.prestige}~${entry.character.battleRank}`;
			flat.lastLogin = entry.character.dateLastLogin;
		} else {
			flat.name = `<not yet loaded>`;
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

	public static parseActivity(elem: any): OutfitActivity {
		return {
			outfitID: elem.outfitID,
			timestamp: new Date(elem.timestamp),
			count: elem.count
		};
    }

	public static async searchByName(name: string): Promise<Loading<PsOutfit[]>> {
		return OutfitApi.get().readList(`/api/outfit/search/${name}`, OutfitApi.parse);
    }

	public static async getByID(outfitID: string): Promise<Loading<PsOutfit>> {
		return OutfitApi.get().readSingle(`/api/outfit/${outfitID}`, OutfitApi.parse);
	}

	public static async getByIDs(outfitIDs: string[]): Promise<Loading<PsOutfit[]>> {
		const params: URLSearchParams = new URLSearchParams();
		for (const id of outfitIDs) {
			params.append("IDs", id);
		}

		return OutfitApi.get().readList(`/api/outfit/many?${params.toString()}`, OutfitApi.parse);
	}

	public static async getByTag(tag: string): Promise<Loading<PsOutfit[]>> {
		return OutfitApi.get().readList(`/api/outfit/tag/${tag}`, OutfitApi.parse);
	}

	public static async getMembers(outfitID: string, includeStats: boolean = true): Promise<Loading<ExpandedOutfitMember[]>> {
		return OutfitApi.get().readList(`/api/outfit/${outfitID}/members?includeStats=${includeStats}`, OutfitApi.parseExpandedOutfitMember);
	}

	public static async getMembersFlat(outfitID: string, includeStats: boolean = true): Promise<Loading<FlatExpandedOutfitMember[]>> {
		const members: Loading<ExpandedOutfitMember[]> = await OutfitApi.getMembers(outfitID, includeStats);
		if (members.state != "loaded") {
			return Loadable.rewrap(members);
		}

		return Loadable.loaded(members.data.map(iter => OutfitApi.flattenExpandedOutfitMember(iter)));
	}

	public static async getActivity(outfitID: string, start: Date, end: Date): Promise<Loading<OutfitActivity[]>> {
		return OutfitApi.get().readList(`/api/outfit/${outfitID}/activity?start=${start.toISOString()}&finish=${end.toISOString()}`, OutfitApi.parseActivity);
    }

}