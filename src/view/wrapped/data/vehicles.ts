import { WrappedEntry } from "api/WrappedApi";

import { PsVehicle } from "api/VehicleApi";

export class WrappedVehicleUsage {
    public vehicleID: number = 0;
    public vehicleName: string = "";
    public vehicle: PsVehicle | null = null;

    // kill with this vehicle
    public killsAs: number = 0;

    // how many of this vehicle killed
    public killed: number = 0;

    // how many times you died in this vehicle
    public deathsAs: number = 0;

    // how many times this vehicle killed you
    public deathsFrom: number = 0;

    public suicides: number = 0;
    public teamkills: number = 0;
    public teamdeaths: number = 0;
    public roadkills: number = 0;

    public driverAssists: number = 0;
    public gunnerAssists: number = 0;

}

export class WrappedVehicleData {

    private static makeUsage(wrapped: WrappedEntry, vehicleID: string): WrappedVehicleUsage {
        const veh: WrappedVehicleUsage = new WrappedVehicleUsage();

        veh.vehicleID = Number.parseInt(vehicleID);
        veh.vehicle = wrapped.vehicles.get(veh.vehicleID) ?? null;
        veh.vehicleName = veh.vehicle?.name ?? `<missing ${veh.vehicleID}>`;

        return veh;
    }

    public static generate(wrapped: WrappedEntry): WrappedVehicleUsage[] {
        const map: Map<string, WrappedVehicleUsage> = new Map();

        const processed: Set<number> = new Set();

        for (const ev of wrapped.vehicleKill) {
            if (ev.attackerVehicleID != "0") {
                let veh: WrappedVehicleUsage | undefined = map.get(ev.attackerVehicleID);

                if (veh == undefined) {
                    veh = this.makeUsage(wrapped, ev.attackerVehicleID);
                }

                if (ev.attackerCharacterID == ev.killedCharacterID) {
                    ++veh.suicides;
                } else if (ev.attackerTeamID == ev.killedTeamID) {
                    ++veh.teamkills;
                } else {
                    ++veh.killsAs;
                }

                map.set(ev.attackerVehicleID, veh);
            }

            if (ev.killedVehicleID != "0") {
                let veh: WrappedVehicleUsage | undefined = map.get(ev.killedVehicleID);

                if (veh == undefined) {
                    veh = this.makeUsage(wrapped, ev.killedVehicleID);
                }

                if (ev.attackerCharacterID != ev.killedCharacterID && ev.attackerTeamID != ev.killedTeamID) {
                    ++veh.killed;
                }

                map.set(ev.killedVehicleID, veh);
            }

            processed.add(ev.id);

        }

        for (const ev of wrapped.vehicleDeath) {
            // skip duplicate events (can happen in a suicide event)
            if (processed.has(ev.id)) {
                continue;
            }

            if (ev.attackerVehicleID != "0") {
                let veh: WrappedVehicleUsage | undefined = map.get(ev.attackerVehicleID);

                if (veh == undefined) {
                    veh = new WrappedVehicleUsage();
                    veh.vehicleID = Number.parseInt(ev.attackerVehicleID);
                    veh.vehicle = wrapped.vehicles.get(veh.vehicleID) ?? null;
                    veh.vehicleName = veh.vehicle?.name ?? `<missing ${veh.vehicleID}>`;
                }

                if (ev.attackerTeamID == ev.killedTeamID) {
                    ++veh.teamdeaths;
                } else {
                    ++veh.deathsFrom;
                }

                map.set(ev.attackerVehicleID, veh);
            }

            if (ev.killedVehicleID != "0") {
                let veh: WrappedVehicleUsage | undefined = map.get(ev.killedVehicleID);

                if (veh == undefined) {
                    veh = new WrappedVehicleUsage();
                    veh.vehicleID = Number.parseInt(ev.killedVehicleID);
                    veh.vehicle = wrapped.vehicles.get(veh.vehicleID) ?? null;
                    veh.vehicleName = veh.vehicle?.name ?? `<missing ${veh.vehicleID}>`;
                }

                if (ev.attackerCharacterID != ev.killedCharacterID && ev.attackerTeamID != ev.killedTeamID) {
                    ++veh.deathsAs;
                }
            }
        }

        return Array.from(map.values());
    }

}
