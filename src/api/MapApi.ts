import * as axios from "axios";

export class PsMapHex {
	public regionID: number = 0;
	public zoneID: number = 0;
	public x: number = 0;
	public y: number = 0;
	public hexType: number = 0;
}

export class PsFacility {
	public facilityID: number = 0;
	public zoneID: number = 0;
	public regionID: number = 0;
	public name: string = "";
	public typeID: number = 0;
	public typeName: string = "";
	public locationX: number | null = 0;
	public locationY: number | null = 0;
	public locationZ: number | null = 0;
}

export class PsFacilityLink {
	public zoneID: number = 0;
	public facilityA: number = 0;
	public facilityB: number = 0;
	public description: string | null = null;
}

export class ZoneMap {
	public hexes: PsMapHex[] = [];
	public facilities: PsFacility[] = [];
	public links: PsFacilityLink[] = [];
}

export class MapApi {
	private static _instance: MapApi = new MapApi();
	public static get(): MapApi { return MapApi._instance; }

	public static async getZone(zoneID: number): Promise<ZoneMap | null> {
        const response: axios.AxiosResponse<any> = await axios.default.get(`/api/map/${zoneID}`);

        if (response.status != 200) {
            return null;
        }

		return response.data as ZoneMap;
	}

}
