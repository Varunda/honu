import * as axios from "axios";

export class FacilityControlEntry {
	public facilityID: number = 0;
	public facilityName: string = "";
	public typeID: number = 0;
	public typeName: string = "";
	public zoneID: number = 0;
	public captured: number = 0;
	public captureAverage: number = 0;
	public defended: number = 0;
	public defenseAverage: number = 0;
	public totalAverage: number = 0;

	public ratio: number = 0;
	public total: number = 0;
}

export class LedgerOptions {
	public zoneID: number | null = null;
	public worldID: number[] = [];
	public playerThreshold: number | null = null;
}

export class LedgerApi {
	private static _instance: LedgerApi = new LedgerApi();
	public static get(): LedgerApi { return LedgerApi._instance; }

	private static parseControlEntry(elem: any): FacilityControlEntry {
		return {
			...elem,
			ratio: (elem.defended || 1) / (elem.captured || 1),
			total: elem.captured + elem.defended
		};
	}

	public static async getLedger(options?: LedgerOptions): Promise<FacilityControlEntry[]> {
		const param: URLSearchParams = new URLSearchParams();

		if (options) {
			if (options.worldID.length > 0) {
				for (const worldID of options.worldID) {
					param.append("worldID", worldID);
				}
			}

			if (options.playerThreshold) {
				param.set("playerThreshold", options.playerThreshold.toString());
			}
		}

        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/ledger/?${param.toString()}`);

        if (response.status != 200) {
            return [];
        }

        if (Array.isArray(response.data) == false) {
            console.warn(`response data is not an array: ${response.data}`);
            return [];
        }

		const ret: FacilityControlEntry[] = [];
		for (const datum of response.data) {
			const elem: FacilityControlEntry = LedgerApi.parseControlEntry(datum);
            ret.push(elem);
        }

        return ret;
	}
}
