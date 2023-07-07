import { ZoneState, ZoneStateApi } from "api/ZoneStateApi";

export class WorldOverview {
    public worldName: string = "";
    public worldID: number = 0;
    public playersOnline: number = 0;
    public zones: ZoneState[] = [];
}