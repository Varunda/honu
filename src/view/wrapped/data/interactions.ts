import { WrappedEntry } from "api/WrappedApi";

// models
import { KillEvent } from "api/KillStatApi";
import { Experience } from "api/ExpStatApi";
import { PsCharacter } from "api/CharacterApi";
import { PsOutfit } from "api/OutfitApi";
import { EntityFought, EntitySupported, Entity } from "../common";

// util
import CharacterUtils from "util/Character";
import FactionUtils from "util/Faction";

export class WrappedEntityInteraction {

    public characterFight: EntityFought[] = [];
    public outfitFight: EntityFought[] = [];

    public characterSupport: EntitySupported[] = [];
    public outfitSupport: EntitySupported[] = [];

    public static generate(wrapped: WrappedEntry): WrappedEntityInteraction {
        const interactions: WrappedEntityInteraction = new WrappedEntityInteraction();

        this.generateFight(wrapped, interactions);
        this.generateSupport(wrapped, interactions);

        return interactions;
    }

    private static generateFight(wrapped: WrappedEntry, interactions: WrappedEntityInteraction): void {
        const charMap: Map<string, EntityFought> = new Map();
        const outfitMap: Map<string, EntityFought> = new Map();

        const processKill = (kill: KillEvent): void => {
            const character: PsCharacter | undefined = wrapped.characters.get(kill.killedCharacterID);
            const f: EntityFought = this.getCharacterEntity(wrapped, kill.killedCharacterID, charMap, () => new EntityFought());
            processKillSingle(f, kill);

            charMap.set(kill.killedCharacterID, f);

            if (character == undefined) {
                return;
            }

            const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
            const o: EntityFought = this.getOutfitEntity(wrapped, outfitID, character, outfitMap, () => new EntityFought());
            processKillSingle(o, kill);

            outfitMap.set(outfitID, o);
        };

        const processDeath = (death: KillEvent): void => {
            const character: PsCharacter | undefined = wrapped.characters.get(death.attackerCharacterID);
            const f: EntityFought = this.getCharacterEntity(wrapped, death.attackerCharacterID, charMap, () => new EntityFought());
            processDeathSingle(f, death);

            if (character == undefined) {
                return;
            }

            const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
            const o: EntityFought = this.getOutfitEntity(wrapped, outfitID, character, outfitMap, () => new EntityFought());

            processDeathSingle(o, death);
        }

        const processKillSingle = (entity: EntityFought, ev: KillEvent): void => {
            if (ev.attackerTeamID != ev.killedTeamID) {
                ++entity.kills;

                entity.characters.add(ev.killedCharacterID);

                if (ev.isHeadshot == true) {
                    ++entity.headshotKills;
                }

                const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(wrapped, ev.attackerFireModeID);
                if (fireModeIndex == 0) {
                    ++entity.hipKills;
                } else if (fireModeIndex == 1) {
                    ++entity.adsKills;
                }
            } else {
                ++entity.teamkills;
            }
        };

        const processDeathSingle = (entity: EntityFought, ev: KillEvent): void => {
            if (ev.attackerTeamID != ev.killedTeamID) {
                ++entity.deaths;

                entity.characters.add(ev.attackerCharacterID);

                if (ev.isHeadshot == true) {
                    ++entity.headshotDeaths;
                }

                const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(wrapped, ev.attackerFireModeID);
                if (fireModeIndex == 0) {
                    ++entity.hipDeaths;
                } else if (fireModeIndex == 1) {
                    ++entity.adsDeaths;
                }
            } else {
                ++entity.teamdeaths;
            }
        };

        for (const kill of wrapped.kills) {
            processKill(kill);
        }

        for (const death of wrapped.deaths) {
            processDeath(death);
        }

        for (const kill of wrapped.teamkills) {
            processKill(kill);
        }

        for (const death of wrapped.teamdeaths) {
            processDeath(death);
        }

        const characterData: EntityFought[] = Array.from(charMap.values()).map((iter: EntityFought) => {
            return EntityFought.updateRatios(iter);
        });

        const outfitData: EntityFought[] = Array.from(outfitMap.values()).map((iter: EntityFought) => {
            return EntityFought.updateRatios(iter);
        });

        interactions.characterFight = characterData;
        interactions.outfitFight = outfitData;
    }

    private static generateSupport(wrapped: WrappedEntry, interactions: WrappedEntityInteraction): void {
        const charMap: Map<string, EntitySupported> = new Map();
        const outfitMap: Map<string, EntitySupported> = new Map();

        for (const exp of wrapped.exp) {
            if (exp.otherID.length != 19) {
                continue;
            }

            const id: string = exp.otherID;
            const character: PsCharacter | undefined = wrapped.characters.get(id);
            const f: EntitySupported = this.getCharacterEntity(wrapped, id, charMap, () => new EntitySupported());

            if (Experience.isHeal(exp.experienceID)) {
                ++f.heals;
                f.healthHealed += exp.amount * 25;
            } else if (Experience.isRevive(exp.experienceID)) {
                ++f.revives;
            } else if (Experience.isResupply(exp.experienceID)) {
                ++f.resupplies;
            } else if (Experience.isMaxRepair(exp.experienceID)) {
                ++f.maxRepairs;
                f.maxHealthRepairs += exp.amount * 10;
            } else if (Experience.isShieldRepair(exp.experienceID)) {
                ++f.shieldRepairs;
            }

            charMap.set(id, f);

            if (character == undefined) {
                continue;
            }

            const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
            const o: EntitySupported = this.getOutfitEntity(wrapped, outfitID, character, outfitMap, () => new EntitySupported());

            if (Experience.isHeal(exp.experienceID)) {
                ++o.heals;
                o.healthHealed += exp.amount * 25;
            } else if (Experience.isRevive(exp.experienceID)) {
                ++o.revives;
            } else if (Experience.isResupply(exp.experienceID)) {
                ++o.resupplies;
            } else if (Experience.isMaxRepair(exp.experienceID)) {
                ++o.maxRepairs;
                o.maxHealthRepairs += exp.amount * 10;
            } else if (Experience.isShieldRepair(exp.experienceID)) {
                ++o.shieldRepairs;
            }

            outfitMap.set(outfitID, o);
        }

        interactions.characterSupport = Array.from(charMap.values());
        interactions.outfitSupport = Array.from(outfitMap.values());
    }

    private static getCharacterEntity<T extends Entity>(wrapped: WrappedEntry, id: string, map: Map<string, T>, create: () => T): T {
        let e: T | undefined = map.get(id);

        if (e == undefined) {
            const character: PsCharacter | undefined = wrapped.characters.get(id);

            e = create();
            e.id = id
            e.type = "character";
            e.factionID = character?.factionID ?? 0;
            e.worldID = character?.worldID ?? 0;

            if (character != undefined) {
                e.displayName = CharacterUtils.getDisplay(character);
            } else {
                e.displayName = `<missing ${id}>`;
            }
        }

        return e;
    }

    private static getOutfitEntity<T extends Entity>(wrapped: WrappedEntry, id: string, character: PsCharacter, map: Map<string, T>, create: () => T): T {
        let e: T | undefined = map.get(id);

        if (e == undefined) {
            e = create();
            e.id = id
            e.type = "outfit";

            if (character.outfitID == null) {
                e.displayName = `<no outfit ${FactionUtils.getName(character.factionID)}>`;
                e.worldID = 0;
                e.factionID = character.factionID;
            } else {
                const outfit: PsOutfit | undefined = wrapped.outfits.get(character.outfitID);
                if (outfit == undefined) {
                    e.displayName = `<missing ${character.outfitID}>`;
                    e.factionID = 0;
                    e.worldID = 0;
                } else {
                    e.displayName = `[${outfit.tag}] ${outfit.name}`;

                    const leader: PsCharacter | undefined = wrapped.characters.get(outfit.leaderID);
                    if (leader == undefined) {
                        console.warn(`missing outfit leader ID ${outfit.leaderID}`);
                    }
                    e.factionID = leader?.factionID ?? 0;
                    e.worldID = leader?.worldID ?? 0;
                }
            }
        }

        return e;
    }

}
