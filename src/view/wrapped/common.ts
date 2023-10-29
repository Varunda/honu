import { WrappedEntry } from "api/WrappedApi";
import LoadoutUtils from "util/Loadout";
import { Session } from "api/SessionApi";
import { KillEvent } from "api/KillStatApi";
import { Experience, ExpEvent } from "api/ExpStatApi";
import { PsCharacter } from "api/CharacterApi";
import { WrappedEntityInteraction } from "./data/interactions";
import { WrappedVehicleUsage, WrappedVehicleData } from "./data/vehicles";
import { PsVehicle } from "api/VehicleApi";
import { PsItem } from "api/ItemApi";
import ZoneUtils from "../../util/Zone";

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
    public characters: Set<string> = new Set();

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

export class VehicleUsage {
    public vehicleID: number = 0;
    public vehicleName: string = "";
    public vehicle: PsVehicle | null = null;

    public kills: number = 0;
    public deaths: number = 0;
    public driverAssists: number = 0;
    public gunnerAssists: number = 0;

}

type PsEvent = KillEvent & { type: "kill" }
    | KillEvent & { type: "death" }
    | ExpEvent & { type: "exp" }
    | Session & { type: "session_start", timestamp: Date }
    | Session & { type: "session_end", timestamp: Date };

export class WrappedExtraData {

    public classStats: WrappedClassStats[] = [];
    public sessions: WrappedSession[] = [];
    public characterFight: EntityFought[] = [];
    public outfitFight: EntityFought[] = [];
    public characterSupport: EntitySupported[] = [];
    public outfitSupport: EntitySupported[] = [];
    public vehicleUsage: WrappedVehicleUsage[] = [];
    public weaponStats: WrappedWeaponStats[] = [];
    public zoneStats: WrappedZoneStats[] = [];

    public totalPlaytime: number = 0;

    public focusedCharacter: PsCharacter = new PsCharacter();

    public static build(wrapped: WrappedEntry): WrappedExtraData {
        const extra: WrappedExtraData = new WrappedExtraData();

        const infil: WrappedClassStats = new WrappedClassStats("Infiltrator", "infil");
        const lightAssault: WrappedClassStats = new WrappedClassStats("Light Assault", "light");
        const medic: WrappedClassStats = new WrappedClassStats("Combat Medic", "medic");
        const engi: WrappedClassStats = new WrappedClassStats("Engineer", "engi");
        const heavy: WrappedClassStats = new WrappedClassStats("Heavy Assault", "heavy");
        const max: WrappedClassStats = new WrappedClassStats("MAX", "max");
        const other: WrappedClassStats = new WrappedClassStats("Other");

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

            return other;
        }

        const weapons: Map<number, WrappedWeaponStats> = new Map();

        const events: PsEvent[] = [];

        for (const ev of wrapped.kills) {
            const classStats: WrappedClassStats = getClassStats(ev.attackerLoadoutID);
            ++classStats.kills;

            if (weapons.has(ev.weaponID) == false) {
                weapons.set(ev.weaponID, {
                    kills: 0,
                    headshots: 0,
                    item: wrapped.items.get(ev.weaponID) ?? null,
                    itemId: ev.weaponID,
                    name: wrapped.items.get(ev.weaponID)?.name ?? `<missing ${ev.weaponID}>`
                });
            }

            const w: WrappedWeaponStats = weapons.get(ev.weaponID)!;
            w.kills += 1;
            if (ev.isHeadshot == true) {
                w.headshots += 1;
            }

            weapons.set(ev.weaponID, w);

            events.push({
                type: "kill",
                ...ev
            });
        }

        extra.weaponStats = Array.from(weapons.values());

        for (const ev of wrapped.deaths) {
            const classStats: WrappedClassStats = getClassStats(ev.killedLoadoutID);
            ++classStats.deaths;

            events.push({
                type: "death",
                ...ev
            });
        }

        for (const ev of wrapped.exp) {
            const classStats: WrappedClassStats = getClassStats(ev.loadoutID);
            classStats.exp += ev.amount;

            events.push({
                type: "exp",
                ...ev
            });
        }

        for (const session of wrapped.sessions) {
            if (session.end == null) {
                continue;
            }

            extra.totalPlaytime += (session.end.getTime() - session.start.getTime()) / 1000;

            // in cases where an event starts a session, not a login, ensure the session start comes first
            events.push({
                type: "session_start",
                timestamp: new Date(session.start.getTime() - 50),
                ...session
            });

            events.push({
                type: "session_end",
                timestamp: session.end,
                ...session,
            });
        }

        if (events.length <= 0) {
            return extra;
        }

        events.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

        let currentSession: WrappedSession = new WrappedSession();
        let sessionID: number | null = null;
        let lastLoadoutID: number = 0;

        const zoneInfo: Map<number, WrappedZoneStats> = new Map();

        let lastTimestamp: Date = events[0].timestamp;
        let lastZoneTimestamp: Date = events[0].timestamp;

        for (const ev of events) {
            if (ev.type == "session_start") {
                if (sessionID != null) {
                    console.warn(`already in session ${sessionID}, closing it`);
                    extra.sessions.push(currentSession);
                    currentSession.duration = (ev.timestamp.getTime() - currentSession.session.start.getTime()) / 1000;
                    currentSession = new WrappedSession();
                }

                currentSession.session = ev;
                currentSession.start = ev.timestamp;
                currentSession.characterID = ev.characterID;
                currentSession.character = wrapped.characters.get(ev.characterID) ?? null;

                sessionID = ev.id;
                lastTimestamp = ev.timestamp;
            } else if (ev.type == "session_end") {
                if (sessionID == null) {
                    console.warn(`not in a session at ${ev.timestamp.toISOString()}`);
                } else {
                    currentSession.duration = (ev.timestamp.getTime() - currentSession.session.start.getTime()) / 1000;
                    extra.sessions.push(currentSession);
                    currentSession = new WrappedSession();
                }

                sessionID = null;
                lastTimestamp = ev.timestamp;
            } else if (ev.type == "kill" || ev.type == "death" || ev.type == "exp") {
                if (sessionID == null) {
                    console.warn(`event at ${ev.timestamp.toISOString()} occured not within a session`);
                    continue;
                }

                let eventDefID: number = (ev.zoneID & 0xFFFF);

                let zoneStats: WrappedZoneStats | undefined = zoneInfo.get(eventDefID);
                if (zoneStats == undefined) {
                    zoneStats = {
                        definitionID: eventDefID,
                        kills: 0,
                        deaths: 0,
                        exp: 0,
                        timeMs: 0,
                        name: ZoneUtils.getZoneName(eventDefID)
                    };
                }

                let eventLoadoutID: number;
                if (ev.type == "kill") {
                    eventLoadoutID = ev.attackerLoadoutID;
                    ++currentSession.kills;
                    ++zoneStats.kills;

                    if (ev.isHeadshot == true) {
                        ++currentSession.headshots;
                    }

                    if (LoadoutUtils.isMax(ev.killedLoadoutID)) {
                        ++currentSession.maxKills;
                    }

                    if (ev.attackerVehicleID == 0) {
                        ++currentSession.infantryKills;
                    }

                } else if (ev.type == "death") {
                    eventLoadoutID = ev.killedLoadoutID;
                    ++currentSession.deaths;
                    ++zoneStats.deaths;
                } else if (ev.type == "exp") {
                    eventLoadoutID = ev.loadoutID;
                    currentSession.exp += ev.amount;
                    zoneStats.exp += ev.amount;

                    if (Experience.isHeal(ev.experienceID)) {
                        ++currentSession.heals;
                    } else if (Experience.isRevive(ev.experienceID)) {
                        ++currentSession.revives;
                    } else if (Experience.isMaxRepair(ev.experienceID)) {
                        ++currentSession.maxRepair;
                    } else if (Experience.isVehicleRepair(ev.experienceID)) {
                        ++currentSession.vehicleRepair;
                    } else if (Experience.isAssist(ev.experienceID)) {
                        ++currentSession.assists;
                    } else if (Experience.isResupply(ev.experienceID)) {
                        ++currentSession.resupplies;
                    } else if (Experience.isShieldRepair(ev.experienceID)) {
                        ++currentSession.shieldRepairs;
                    }

                } else {
                    throw `invalid state, event was not kill|death|exp: ${(ev as any).type}`;
                }

                const zoneTimespan: number = ev.timestamp.getTime() - lastZoneTimestamp.getTime();
                if (zoneTimespan <= 1000 * 60 * 30) {
                    zoneStats.timeMs += ev.timestamp.getTime() - lastZoneTimestamp.getTime();
                }
                lastZoneTimestamp = ev.timestamp;
                zoneInfo.set(eventDefID, zoneStats);

                if (lastLoadoutID != eventLoadoutID) {
                    const timespan: number = ev.timestamp.getTime() - lastTimestamp.getTime();
                    // fail safe, don't include events that are 30 minutes apart
                    if (timespan <= 1000 * 60 * 30) {
                        const classStats: WrappedClassStats = getClassStats(eventLoadoutID);
                        classStats.timeAs += timespan;
                    }

                    lastTimestamp = ev.timestamp;
                }
            }
        }

        extra.classStats = [
            infil, lightAssault, medic, engi, heavy, max
        ];

        extra.zoneStats = Array.from(zoneInfo.values());

        extra.classStats = extra.classStats.map((iter: WrappedClassStats) => {
            iter.kd = iter.kills / Math.max(1, iter.deaths);
            iter.kpm = iter.kills / Math.max(1, iter.timeAs / 1000) * 60;
            iter.spm = iter.exp / Math.max(1, iter.timeAs / 1000) * 60;

            return iter;
        });

        const interactions: WrappedEntityInteraction = WrappedEntityInteraction.generate(wrapped);
        extra.characterFight = interactions.characterFight;
        extra.outfitFight = interactions.outfitFight;
        extra.characterSupport = interactions.characterSupport;
        extra.outfitSupport = interactions.outfitSupport;

        extra.vehicleUsage = WrappedVehicleData.generate(wrapped);

        const timePerCharacter: Map<string, number> = new Map();
        for (const session of extra.sessions) {
            timePerCharacter.set(session.characterID, (timePerCharacter.get(session.characterID) ?? 0) + session.duration);
        }

        const mostPlayedCharacterID: string = Array.from(timePerCharacter.entries()).sort((a, b) => {
            return b[1] - a[1];
        })[0][0];

        if (wrapped.characters.has(mostPlayedCharacterID)) {
            extra.focusedCharacter = wrapped.characters.get(mostPlayedCharacterID)!;
        }

        return extra;
    }

}

export class WrappedClassStats {
    constructor(name?: string, icon?: string) {
        this.name = name ?? "";
        this.icon = icon ?? "";
    }

    public name: string = "";
    public icon: string = "";
    public kills: number = 0;
    public deaths: number = 0;
    public exp: number = 0;
    public timeAs: number = 0;

    public kd: number = 0;
    public kpm: number = 0;
    public spm: number = 0;
}

export class WrappedWeaponStats {

    public name: string = "";
    public itemId: number = 0;

    public kills: number = 0;
    public headshots: number = 0;

    public item: PsItem | null = null;
}

export class WrappedSession {
    public session: Session = new Session();
    public characterID: string = "";
    public duration: number = 0;
    public start: Date = new Date();
    public character: PsCharacter | null = null;

    public kills: number = 0;
    public infantryKills: number = 0;
    public headshots: number = 0;
    public deaths: number = 0;
    public exp: number = 0;

    public maxKills: number = 0;

    public assists: number = 0;
    public revives: number = 0;
    public heals: number = 0;
    public shieldRepairs: number = 0;
    public resupplies: number = 0;
    public maxRepair: number = 0;
    public vehicleRepair: number = 0;

}

export class WrappedZoneStats {
    public definitionID: number = 0;
    public name: string = "";
    public kills: number = 0;
    public deaths: number = 0;
    public exp: number = 0;
    public timeMs: number = 0;
}

