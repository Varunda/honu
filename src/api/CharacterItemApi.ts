import * as axios from "axios";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

import { ItemApi, PsItem } from "api/ItemApi";
import { ItemCategory, ItemCategoryApi } from "api/ItemCategoryApi";
import { ItemType, ItemTypeApi } from "api/ItemTypeApi";

export class CharacterItem {
	public characterID: string = "";
	public itemID: string = "";
	public accountLevel: boolean = false;
	public stackCount: number | null = null;
}

export class ExpandedCharacterItem {
	public entry: CharacterItem = new CharacterItem();
	public item: PsItem | null = null;

	public category: ItemCategory | null = null;
	public type: ItemType | null = null;

	public categoryName: string = "";
	public typeName: string = "";

	public itemID: string = "";
	public itemName: string = "";
	public accountLevel: boolean = false;
}

export class CharacterItemApi extends ApiWrapper<CharacterItem> {
	private static _instance: CharacterItemApi = new CharacterItemApi();
	public static get(): CharacterItemApi { return CharacterItemApi._instance; }

	public static parseCharacterItem(elem: any): CharacterItem {
		return {
			...elem
		};
	}

	public static parseExpandedCharacterItem(elem: any): ExpandedCharacterItem {
		const entry: ExpandedCharacterItem = {
			entry: CharacterItemApi.parseCharacterItem(elem.entry),
			item: (elem.item == null) ? null : ItemApi.parse(elem.item),

			category: elem.category == null ? null : ItemCategoryApi.parse(elem.category),
			categoryName: "",
			type: elem.type == null ? null : ItemTypeApi.parse(elem.type),
			typeName: "",

			itemID: elem.entry.itemID,
			itemName: (elem.item == null) ? `<missing ${elem.entry.itemID}>` : elem.item.name,
			accountLevel: elem.entry.accountLevel
		};

		entry.categoryName = entry.category?.name ?? `<unknown>`;
		entry.typeName = entry.type?.name ?? `<unknown>`;

		return entry;
	}

	public static async getByID(charID: string): Promise<Loading<ExpandedCharacterItem[]>> {
		return CharacterItemApi.get().readList(`/api/character/${charID}/items`, CharacterItemApi.parseExpandedCharacterItem);
	}

}