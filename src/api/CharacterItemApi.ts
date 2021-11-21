﻿import * as axios from "axios";

import { ItemApi, PsItem } from "api/ItemApi";

export class CharacterItem {
	public characterID: string = "";
	public itemID: string = "";
	public accountLevel: boolean = false;
	public stackCount: number | null = null;
}

export class ExpandedCharacterItem {
	public entry: CharacterItem = new CharacterItem();
	public item: PsItem | null = null;

	public itemID: string = "";
	public itemName: string = "";
	public accountLevel: boolean = false;
}

export class CharacterItemApi {
	private static _instance: CharacterItemApi = new CharacterItemApi();
	public static get(): CharacterItemApi { return CharacterItemApi._instance; }

	public static parseCharacterItem(elem: any): CharacterItem {
		return {
			...elem
		};
	}

	public static parseExpandedCharacterItem(elem: any): ExpandedCharacterItem {
		return {
			entry: CharacterItemApi.parseCharacterItem(elem.entry),
			item: (elem.item == null) ? null : ItemApi.parse(elem.item),
			itemID: elem.entry.itemID,
			itemName: (elem.item == null) ? `<missing ${elem.entry.itemID}>` : elem.item.name,
			accountLevel: elem.entry.accountLevel,
		};
	}

	public static async getByID(charID: string): Promise<ExpandedCharacterItem[]> {
		const response: axios.AxiosResponse = await axios.default.get(`/api/character/${charID}/items`);

		if (response.status != 200) {
			throw response.data;
		}

		return response.data.map((iter: any) => CharacterItemApi.parseExpandedCharacterItem(iter));
	}

}