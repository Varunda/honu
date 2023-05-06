import { WrappedEntry } from "api/WrappedApi";
import LoadoutUtils from "util/Loadout";
import { Session } from "api/SessionApi";
import { KillEvent } from "../../api/KillStatApi";
import { ExpEvent } from "../../api/ExpStatApi";
import { PsCharacter } from "../../api/CharacterApi";
import { WrappedEntityInteraction } from "./data/interactions";

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

    public static build(wrapped: WrappedEntry): WrappedExtraData {
        const extra: WrappedExtraData = new WrappedExtraData();

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

        const events: PsEvent[] = [];

        for (const ev of wrapped.kills) {
            const classStats: WrappedClassStats = getClassStats(ev.attackerLoadoutID);
            ++classStats.kills;

            events.push({
                type: "kill",
                ...ev
            });
        }

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

        let lastTimestamp: Date = events[0].timestamp;
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

                let eventLoadoutID: number;
                if (ev.type == "kill") {
                    eventLoadoutID = ev.attackerLoadoutID;
                    ++currentSession.kills;
                } else if (ev.type == "death") {
                    eventLoadoutID = ev.killedLoadoutID;
                    ++currentSession.deaths;
                } else if (ev.type == "exp") {
                    eventLoadoutID = ev.loadoutID;
                    currentSession.exp += ev.amount;
                } else {
                    throw `invalid state, event was not kill|death|exp: ${(ev as any).type}`;
                }

                if (lastLoadoutID == eventLoadoutID) {
                    continue;
                }

                const timespan: number = ev.timestamp.getTime() - lastTimestamp.getTime();

                if (timespan >= 60000) {
                    //console.log(`${LoadoutUtils.getLoadoutName(eventLoadoutID)} ${timespan} ${ev.timestamp.toISOString()} ${lastTimestamp.toISOString()}!!`);
                }

                // fail safe, don't include events that are 30 minutes apart
                if (timespan <= 1000 * 60 * 30) {
                    const classStats: WrappedClassStats = getClassStats(eventLoadoutID);
                    classStats.timeAs += timespan;
                }

                lastTimestamp = ev.timestamp;
            }
        }

        extra.classStats = [
            infil, lightAssault, medic, engi, heavy, max
        ];

        const interactions: WrappedEntityInteraction = WrappedEntityInteraction.generate(wrapped);
        extra.characterFight = interactions.characterFight;
        extra.outfitFight = interactions.outfitFight;
        extra.characterSupport = interactions.characterSupport;
        extra.outfitSupport = interactions.outfitSupport;

        return extra;
    }

}

export class WrappedClassStats {
    constructor(name?: string) {
        this.name = name ?? "";
    }

    public name: string = "";
    public kills: number = 0;
    public deaths: number = 0;
    public exp: number = 0;
    public timeAs: number = 0;
}

export class WrappedSession {
    public session: Session = new Session();
    public characterID: string = "";
    public duration: number = 0;
    public start: Date = new Date();
    public character: PsCharacter | null = null;

    public kills: number = 0;
    public deaths: number = 0;
    public exp: number = 0;
}