import { PsAlert, AlertApi } from "./AlertApi";

export class PlayerCount {
    public all: number = 0;
    public tr: number = 0;
    public nc: number = 0;
    public vs: number = 0;
    public unknown: number = 0;
}

export class TerritoryControl {
    public tr: number = 0;
    public nc: number = 0;
    public vs: number = 0;
    public total: number = 0;
}

export class ZoneState {
    public zoneID: number = 0;
    public worldID: number = 0;
    public isOpened: boolean = false;
    public unstableState: number = 0;
    public lastLocked: Date | null = null;
    public alert: PsAlert | null = null;
    public alertStart: Date | null = null;
    public alertEnd: Date | null = null;
    public playerCount: number = 0;
    public players: PlayerCount = new PlayerCount();
    public territoryControl: TerritoryControl = new TerritoryControl();
}

export class ZoneStateApi {

    public static parse(elem: any): ZoneState {
        return {
            zoneID: elem.zoneID,
            worldID: elem.worldID,
            isOpened: elem.isOpened,
            unstableState: elem.unstableState,
            lastLocked: (elem.lastLocked == null) ? null : new Date(elem.lastLocked),
            alert: (elem.alert == null) ? null : AlertApi.readEntry(elem.alert),
            alertStart: (elem.alertStart == null) ? null : new Date(elem.alertStart),
            alertEnd: (elem.alertEnd == null) ? null : new Date(elem.alertEnd),
            playerCount: elem.playerCount,
            players: {
                all: elem.players.all,
                tr: elem.players.tr,
                nc: elem.players.nc,
                vs: elem.players.vs,
                unknown: elem.players.unknown,
            },
            territoryControl: {
                tr: elem.territoryControl.tr,
                nc: elem.territoryControl.nc,
                vs: elem.territoryControl.vs,
                total: elem.territoryControl.total
            }
        };
    }

}