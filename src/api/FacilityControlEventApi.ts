import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class FacilityControlEvent {
	public id: number = 0;
	public facilityID: number = 0;
	public timestamp: Date = new Date();
	public players: number = 0;
	public newFactionID: number = 0;
	public oldFactionID: number = 0;
	public outfitID: string | null = null;
	public worldID: number = 0;
	public zoneID: number = 0;
}

export class FacilityControlEventApi extends ApiWrapper<FacilityControlEvent> {

	public static parse(elem: any): FacilityControlEvent {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	}

}
