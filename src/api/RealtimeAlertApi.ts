import { ExpEvent, ExpStatApi } from "./ExpStatApi";
import { KillEvent, KillStatApi } from "./KillStatApi";
import { VehicleDestroyEvent, VehicleDestroyEventApi } from "./VehicleDestroyEventApi";

export class RealtimeAlert {
    public timestamp: Date = new Date();
    public worldID: number = 0;
    public zoneID: number = 0;
    public vs: RealtimeAlertTeam = new RealtimeAlertTeam();
    public nc: RealtimeAlertTeam = new RealtimeAlertTeam();
    public tr: RealtimeAlertTeam = new RealtimeAlertTeam();
}

export class RealtimeAlertTeam {
    public teamID: number = 0;
    public kills: number = 0;
    public deaths: number = 0;
    public experience: Map<number, number> = new Map();
    public vehicleKills: number = 0;
    public vehicleDeaths: number = 0;

    public killDeathEvents: KillEvent[] = [];
    public expEvents: ExpEvent[] = [];
    public vehicleDestroyEvents: VehicleDestroyEvent[] = [];
}

export class RealtimeAlertApi {

    public static parse(elem: any): RealtimeAlert {
        return {
            timestamp: new Date(elem.timestamp),
            ...elem,
            vs: RealtimeAlertTeamApi.parse(elem.vs),
            nc: RealtimeAlertTeamApi.parse(elem.nc),
            tr: RealtimeAlertTeamApi.parse(elem.tr),
        };
    }

}

export class RealtimeAlertTeamApi {

    public static parse(elem: any): RealtimeAlertTeam {
        return {
            ...elem,
            experience: new Map(Object.entries(elem.experience).map(iter => [Number.parseInt(iter[0]), iter[1]])),
            killDeathEvents: elem.killDeathEvents.map((iter: any) => KillStatApi.parseKillEvent(iter)),
            expEvents: elem.killDeathEvents.map((iter: any) => ExpStatApi.parseExpEvent(iter)),
            vehicleDestroyEvents: elem.killDeathEvents.map((iter: any) => VehicleDestroyEventApi.parse(iter)),
        };
    }

}