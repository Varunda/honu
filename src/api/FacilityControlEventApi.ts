import ApiWrapper from "api/ApiWrapper";

import { OutfitApi, PsOutfit } from "api/OutfitApi";
import { MapApi, PsFacility } from "api/MapApi";
import { Loading } from "../Loading";

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

export class ExpandedFacilityControlEvent {
	public event: FacilityControlEvent = new FacilityControlEvent();
	public outfit: PsOutfit | null = null;
	public facility: PsFacility | null = null;
}

export class FacilityControlEventApi extends ApiWrapper<FacilityControlEvent> {

	private static _instance: FacilityControlEventApi = new FacilityControlEventApi();
	public static get(): FacilityControlEventApi { return FacilityControlEventApi._instance; }

	public static parse(elem: any): FacilityControlEvent {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		};
	}

	public static parseExpanded(elem: any): ExpandedFacilityControlEvent {
		return {
			event: FacilityControlEventApi.parse(elem.event),
			outfit: (elem.outfit) == null ? null : OutfitApi.parse(elem.outfit),
			facility: (elem.facility) == null ? null : MapApi.parseFacility(elem.facility)
		};
    }

	public static getByAlertID(alertID: number): Promise<Loading<ExpandedFacilityControlEvent[]>> {
		return FacilityControlEventApi.get().readList(`/api/alerts/${alertID}/control`, FacilityControlEventApi.parseExpanded);
    }

}
