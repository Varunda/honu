import Report, { PlayerMetadata } from "./Report";

import { Experience, ExpEvent } from "api/ExpStatApi";
import { KillEvent } from "api/KillStatApi";

import LoadoutUtils from "util/Loadout";

/**
 * Represents how much damage a single character did
 */
export class InfantryDamageEntry {
    public characterID: string = "";

    public kills: KillEvent[] = [];
    public assists: ExpEvent[] = [];

    /**
     * Damage done in total
     */
    public totalDamage: number = 0;

    /**
     * Damage done as infil
     */
    public infilDamage: number = 0;

    /**
     * Damage done as light assault
     */
    public lightAssaultDamage: number = 0;

    /**
     * Damage done as medic
     */
    public medicDamage: number = 0;

    /**
     * Damage done as engi
     */
    public engiDamage: number = 0;

    /**
     * Damage done as heavy assault
     */
    public heavyDamage: number = 0;

    /**
     * Damage done as max
     */
    public maxDamage: number = 0;

}

export class InfantryDamage {

    /**
     * Generate the infantry damage entries for a report
     * @param report
     */
    public static get(report: Report): InfantryDamageEntry[] {
        const map: Map<number, InfantryDamageInput> = this._createInput(report);

        const missingExp: Map<number, number> = new Map(); // <exp ID, times missed>
        const missingMetadata: Set<string> = new Set();
        const chars: Map<string, InfantryDamageEntry> = new Map();

        const getEntry = function(charID: string): InfantryDamageEntry {
            let entry: InfantryDamageEntry | undefined = chars.get(charID);
            if (entry == undefined) {
                entry = new InfantryDamageEntry();
                entry.characterID = charID;
                chars.set(charID, entry);
            }

            return entry;
        }

        for (const kvp of map.entries()) {
            const kills: KillEvent[] = kvp[1].kills;
            const assists: ExpEvent[] = kvp[1].assists;

            let playersKilled: string[] = kills.map(iter => iter.killedCharacterID);
            playersKilled.push(...assists.map(iter => iter.otherID));
            playersKilled = playersKilled.filter((value, index, arr) => arr.indexOf(value) == index);

            //console.log(`InfantryDamage> have ${playersKilled.length} players killed`);

            for (const killedID of playersKilled) {
                let assistPercent: number = 0;

                for (const assist of assists) {
                    if (assist.otherID != killedID) {
                        continue;
                    }

                    const metadata: PlayerMetadata | undefined = report.playerMetadata.get(assist.sourceID);
                    if (metadata == undefined) {
                        missingMetadata.add(assist.sourceID);
                        continue;
                    }

                    // Normalize the amount of XP gained in case this player had a score multiplier
                    const normalAmount = assist.amount / Math.max(1, metadata.scoreMultiplier);

                    // Get the max amount of xp that can be earned from that event, defaulting to 100 if unknown
                    let baseAmount: number | null = null;
                    switch (assist.experienceID) {
                        case Experience.ASSIST: baseAmount = 100; break;
                        case Experience.PRIORITY_ASSIST: baseAmount = 150; break;
                        case Experience.HIGH_PRIORITY_ASSIST: baseAmount = 300; break;
                    }

                    if (baseAmount == null) {
                        missingExp.set(assist.experienceID, (missingExp.get(assist.experienceID) || 0) + 1);
                        baseAmount = 100;
                    }

                    // The percent of damage this assist dealt
                    let damagePercent: number = normalAmount / baseAmount;

                    if (damagePercent > 1) {
                        console.log(`InfantryDamage> KILLED: ${killedID}, percent is above 1, assuming double xp`);
                        metadata.scoreMultiplier = 2;
                        damagePercent /= 2;
                    }

                    //console.log(`InfantryDamage> KILLED: ${killedID}, ASSIST ${assist.sourceID} did ${damagePercent} (${normalAmount} / ${baseAmount}) of damage`);
                    assistPercent += damagePercent;

                    const assistDamage: number = 1000 * damagePercent;
                    const entry: InfantryDamageEntry = getEntry(assist.sourceID);
                    entry.assists.push(assist);

                    entry.totalDamage += assistDamage;
                    if (LoadoutUtils.isInfiltrator(assist.loadoutID)) { entry.infilDamage += assistDamage; }
                    else if (LoadoutUtils.isLightAssault(assist.loadoutID)) { entry.lightAssaultDamage += assistDamage; }
                    else if (LoadoutUtils.isMedic(assist.loadoutID)) { entry.medicDamage += assistDamage; }
                    else if (LoadoutUtils.isEngineer(assist.loadoutID)) { entry.engiDamage += assistDamage; }
                    else if (LoadoutUtils.isHeavy(assist.loadoutID)) { entry.heavyDamage += assistDamage; }
                    else if (LoadoutUtils.isMax(assist.loadoutID)) { entry.maxDamage += assistDamage; }
                    else {
                        console.error(`Unchecked loadout ID ${assist.loadoutID}`);
                    }
                }

                if (assistPercent > 1) {
                    console.warn(`InfantryDamage> Had more than 100% assist damage? percent=${assistPercent} killed=${killedID}, timestamp=${kvp[0]}`);
                }

                //console.log(`InfantryDamage> KILLED: ${killedID}, PERCENT ${assistPercent}/${1000 * assistPercent}`);

                // If there's a tracked killed, take away all the damage dealt by assists
                const kill: KillEvent | undefined = kills.find(iter => iter.killedCharacterID == killedID);
                if (kill != undefined) {
                    const killPercent: number = Math.max(0, 1 - assistPercent);

                    const entry: InfantryDamageEntry = getEntry(kill.attackerCharacterID);
                    entry.totalDamage += 1000 * killPercent;
                    entry.kills.push(kill);
                }
            }
        }

        if (missingExp.size > 0) {
            console.error(`Missing the following exp IDs for base amounts:`);
            for (const kvp in missingExp.entries()) {
                console.error(`\t${kvp[0]}, missed ${kvp[1]} times`);
            }
        }

        if (missingMetadata.size > 0) {
            console.error(`Missing the following player metadatas: [${Array.from(missingMetadata.values())}]`);
        }

        return Array.from(chars.values());
    }


    /**
     * Create the input that's used to calculate damage done, which has a timestamp for each kill/assist that took place,
     *      along with each kill/assist that occured at the same time
     */
    private static _createInput(report: Report): Map<number, InfantryDamageInput> {
        const map: Map<number, InfantryDamageInput> = new Map();

        for (const ev of report.kills) {
            const ts: number = ev.timestamp.getTime();

            if (map.has(ts) == false) {
                map.set(ts, {
                    timestamp: ev.timestamp,
                    kills: [],
                    assists: []
                });
            }

            const input: InfantryDamageInput = map.get(ts)!; // safe

            input.kills.push({ ...ev });
        }

        for (const ev of report.experience) {
            if (Experience.isAssist(ev.experienceID) == false) {
                continue;
            }

            const ts: number = ev.timestamp.getTime();

            if (map.has(ts) == false) {
                map.set(ts, {
                    timestamp: ev.timestamp,
                    kills: [],
                    assists: []
                });
            }

            const input: InfantryDamageInput = map.get(ts)!; // safe
            input.assists.push({ ...ev });
        }

        return map;
    }
}

class InfantryDamageInput {
    public timestamp: Date = new Date();
    public kills: KillEvent[] = [];
    public assists: ExpEvent[] = [];
}
