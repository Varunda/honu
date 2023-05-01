import { WrappedEntry } from "api/WrappedApi";
import LoadoutUtils from "util/Loadout";

export class WrappedFilters {
    public class = {
        infil: true as boolean,
        lightAssault: true as boolean,
        medic: true as boolean,
        engi: true as boolean,
        heavy: true as boolean,
        max: true as boolean
    };

    public characters: string[] = [];

    public periodStart: Date = new Date();
    public periodEnd: Date = new Date();
}

export interface Entity {
    id: string;
    type: "outfit" | "character" | "unset";
    displayName: string;

    factionID: number;
    worldID: number;
}

export class EntityFought {
    public id: string = "";
    public type: "outfit" | "character" | "unset" = "unset";
    public displayName: string = "";
    public factionID: number = 0;
    public worldID: number = 0;

    public kills: number = 0;
    public deaths: number = 0;
    public kd: number = 0;

    public teamkills: number = 0;
    public teamdeaths: number = 0;

    public headshotKills: number = 0;
    public headshotKillRatio: number = 0;
    public headshotDeaths: number = 0;
    public headshotDeathRatio: number = 0;

    public hipKills: number = 0;
    public hipKillRatio: number = 0;
    public adsKills: number = 0;
    public adsKillRatio: number = 0;

    public hipDeaths: number = 0;
    public hipDeathRatio: number = 0;
    public adsDeaths: number = 0;
    public adsDeathRatio: number = 0;

    /**
     * Mutate the passed parameter to update the ratio values in the instance
     * @param elem
     */
    public static updateRatios(elem: EntityFought): EntityFought {
        elem.kd = elem.kills / Math.max(1, elem.deaths);
        elem.headshotKillRatio = elem.headshotKills / Math.max(1, elem.kills);
        elem.headshotDeathRatio = elem.headshotDeaths / Math.max(1, elem.deaths);
        elem.hipKillRatio = elem.hipKills / Math.max(1, elem.kills);
        elem.adsKillRatio = elem.adsKills / Math.max(1, elem.kills);
        elem.hipDeathRatio = elem.hipDeaths / Math.max(1, elem.deaths);
        elem.adsDeathRatio = elem.adsDeaths / Math.max(1, elem.deaths);

        return elem;
    }

}

export class EntitySupported {
    public id: string = "";
    public type: "outfit" | "character" | "unset" = "unset";
    public displayName: string = "";
    public factionID: number = 0;
    public worldID: number = 0;

    public heals: number = 0;
    public healthHealed: number = 0;
    public revives: number = 0;
    public resupplies: number = 0;
    public maxRepairs: number = 0;
    public maxHealthRepairs: number = 0;
    public assists: number = 0;
    public shieldRepairs: number = 0;

}

type DateEvent = {
    action: "event";
    timestamp: Date;
    loadoutID: number;
}

type SessionStartEvent = {
    action: "start";
    timestamp: Date;
}

type SessionEndEvent = {
    action: "end";
    timestamp: Date;
}

type TimestampedEvent = DateEvent | SessionStartEvent | SessionEndEvent;

export class WrappedClassStats {

    constructor(name?: string) {
        this.name = name ?? "";
    }

    public name: string = "";
    public kills: number = 0;
    public deaths: number = 0;
    public exp: number = 0;
    public timeAs: number = 0;

    public static generate(wrapped: WrappedEntry): WrappedClassStats[] {
        const infil: WrappedClassStats = new WrappedClassStats("Infiltrator");
        const lightAssault: WrappedClassStats = new WrappedClassStats("Light Assault");
        const medic: WrappedClassStats = new WrappedClassStats("Combat Medic");
        const engi: WrappedClassStats = new WrappedClassStats("Engineer");
        const heavy: WrappedClassStats = new WrappedClassStats("Heavy Assault");
        const max: WrappedClassStats = new WrappedClassStats("MAX");

        const getClassStats = (loadoutID: number): WrappedClassStats => {
            if (LoadoutUtils.isInfiltrator(loadoutID) == true) {
                return infil;
            } else if (LoadoutUtils.isLightAssault(loadoutID) == true) {
                return lightAssault;
            } else if (LoadoutUtils.isMedic(loadoutID) == true) {
                return medic;
            } else if (LoadoutUtils.isEngineer(loadoutID) == true) {
                return engi;
            } else if (LoadoutUtils.isHeavy(loadoutID) == true) {
                return heavy;
            } else if (LoadoutUtils.isMax(loadoutID) == true) {
                return max;
            }

            throw `unchecked loadoutID ${loadoutID}`;
        }

        const timestampEvents: TimestampedEvent[] = [];

        for (const ev of wrapped.kills) {
            const classStats: WrappedClassStats = getClassStats(ev.attackerLoadoutID);
            ++classStats.kills;

            timestampEvents.push({
                action: "event",
                timestamp: ev.timestamp,
                loadoutID: ev.attackerLoadoutID
            });
        }

        for (const ev of wrapped.deaths) {
            const classStats: WrappedClassStats = getClassStats(ev.killedLoadoutID);
            ++classStats.deaths;

            timestampEvents.push({
                action: "event",
                timestamp: ev.timestamp,
                loadoutID: ev.killedLoadoutID
            });
        }

        for (const ev of wrapped.exp) {
            const classStats: WrappedClassStats = getClassStats(ev.loadoutID);
            classStats.exp += ev.amount;

            timestampEvents.push({
                action: "event",
                timestamp: ev.timestamp,
                loadoutID: ev.loadoutID
            });
        }

        for (const session of wrapped.sessions) {
            if (session.end == null) {
                continue;
            }

            timestampEvents.push({
                action: "start",
                timestamp: session.start
            });

            timestampEvents.push({
                action: "end",
                timestamp: session.end
            });
        }

        if (timestampEvents.length <= 0) {
            return [];
        }

        timestampEvents.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

        let inSession: boolean = false;
        let lastLoadoutID: number = 0;
        let lastTimestamp: Date = timestampEvents[0].timestamp;
        for (const ev of timestampEvents) {
            if (ev.action == "start") {
                if (inSession == true) {
                    console.warn(`already started a session`);
                }

                inSession = true;
                lastTimestamp = ev.timestamp;
            } else if (ev.action == "end") {
                if (inSession == false) {
                    console.warn(`not in a session at ${ev.timestamp.toISOString()}`);
                }

                inSession = false;
                lastTimestamp = ev.timestamp;
            } else if (ev.action == "event") {
                if (inSession == false) {
                    console.warn(`event at ${ev.timestamp.toISOString()} occured not within a session`);
                }

                if (lastLoadoutID == ev.loadoutID) {
                    continue;
                }

                const timespan: number = ev.timestamp.getTime() - lastTimestamp.getTime();

                const classStats: WrappedClassStats = getClassStats(ev.loadoutID);
                classStats.timeAs += timespan;

                lastTimestamp = ev.timestamp;
            }
        }

        return [
            infil, lightAssault, medic, engi, heavy, max
        ];
    }

}