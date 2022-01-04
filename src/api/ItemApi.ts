import * as axios from "axios";
import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PsItem {
	public id: number = 0;
	public typeID: number = 0;
	public categoryID: number = 0;
	public name: string = "";
}

export class ItemPercentileStats {
	public itemID: string = "";
	public typeID: number = 0;
	public timestamp: Date = new Date();
	public loaded: boolean = true;
	public q0: number = 0;
	public q5: number = 0;
	public q10: number = 0;
	public q15: number = 0;
	public q20: number = 0;
	public q25: number = 0;
	public q30: number = 0;
	public q35: number = 0;
	public q40: number = 0;
	public q45: number = 0;
	public q50: number = 0;
	public q55: number = 0;
	public q60: number = 0;
	public q65: number = 0;
	public q70: number = 0;
	public q75: number = 0;
	public q80: number = 0;
	public q85: number = 0;
	public q90: number = 0;
	public q95: number = 0;
	public q100: number = 0;
}

export class ItemPercentileAll {
	public itemID: string = "";
	public kd: Bucket[] = [];
	public kpm: Bucket[] = [];
	public accuracy: Bucket[] = [];
	public headshotRatio: Bucket[] = [];
}

export class Bucket {
	public start: number = 0;
	public width: number = 0;
	public count: number = 0;
}

export class ItemApi extends ApiWrapper<PsItem> {
	private static _instance: ItemApi = new ItemApi();
	public static get(): ItemApi { return ItemApi._instance; }

	public static parse(elem: any): PsItem {
		return {
			...elem
		};
	};

	public static parsePercentileStats(elem: any): ItemPercentileStats {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	}

	public static parseBucket(elem: any): Bucket {
		return {
			...elem
		};
	}

	public static parsePercentileAll(elem: any): ItemPercentileAll {
		return {
			itemID: elem.itemID,
			kd: elem.kd.map((iter: any) => ItemApi.parseBucket(iter)),
			kpm: elem.kpm.map((iter: any) => ItemApi.parseBucket(iter)),
			accuracy: elem.accuracy.map((iter: any) => ItemApi.parseBucket(iter)),
			headshotRatio: elem.headshotRatio.map((iter: any) => ItemApi.parseBucket(iter)),
		}
	}

	public static async getByID(itemID: string): Promise<Loading<PsItem>> {
		return ItemApi.get().readSingle(`/api/item/${itemID}`, ItemApi.parse)
	}

	public static async getStatsByID(itemID: string): Promise<Loading<ItemPercentileAll>> {
		return ItemApi.get().readSingle(`/api/item/${itemID}/percentile_stats`, ItemApi.parsePercentileAll)
	}

}
