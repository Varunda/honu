import { Loading } from "Loading";

import ApiWrapper from "api/ApiWrapper";
import { PsCharacter, CharacterApi } from "api/CharacterApi";

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

export class ExpandedPlayerControlEvent {
	public event: PlayerControlEvent = new PlayerControlEvent();
	public character: PsCharacter | null = null;
}

export class PlayerControlEventApi extends ApiWrapper<PlayerControlEvent> {

	private static _instance: PlayerControlEventApi = new PlayerControlEventApi();
	public static get(): PlayerControlEventApi { return PlayerControlEventApi._instance; }

	public static parse(elem: any): PlayerControlEvent {
		return {
			...elem,
			timestamp: new Date(elem.timestamp)
		}
	}

	public static parseExpanded(elem: any): ExpandedPlayerControlEvent {
		return {
			event: PlayerControlEventApi.parse(elem.event),
			character: (elem.character) == null ? null : CharacterApi.parse(elem.character)
        }
    }

	public static getByEventID(controlID: number): Promise<Loading<ExpandedPlayerControlEvent[]>> {
		return PlayerControlEventApi.get().readList(`/api/control/${controlID}/players`, PlayerControlEventApi.parseExpanded);
    }

}
