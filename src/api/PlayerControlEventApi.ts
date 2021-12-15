import { Loading } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class PlayerControlEvent {
	public controlID: number = 0;
	public isCapture: boolean = false;
	public characterID: string = "";
	public outfitID: string | null = null;
	public facilityID: number = 0;
	public timestamp: Date = new Date();
	public worldID: number = 0;
	public zoneID: number = 0;
}

export class PlayerControlEventApi extends ApiWrapper<PlayerControlEvent> {

	public static parse(elem: any): PlayerControlEvent {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		}
	}

}
