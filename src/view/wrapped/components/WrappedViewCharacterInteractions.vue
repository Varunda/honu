<template>
    <collapsible header-text="Character interactions">
        <div class="ml-2">
            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-1)">
                    Players fought
                </h3>

                <a-table :entries="characterFight"
                         :paginate="true"
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc"
                         class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 1.5rem;"></faction-image>

                            <a :href="'/c/' + entry.id" style="color: var(--p-red-1)" class="bright-150">
                                {{entry.displayName}}
                            </a>
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            <b>Server</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.worldID | world}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            <b>Kills</b>
                            <info-hover text="How many times an input character killed this character."></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                            ({{entry.kills / wrapped.kills.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="deaths">
                        <a-header>
                            <b>Deaths</b>
                            <info-hover text="How many times an input character was killed by this character. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                            ({{entry.deaths / wrapped.deaths.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="kd">
                        <a-header>
                            K/D
                            <info-hover text="Kills / Deaths. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kd | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotKills">
                        <a-header>
                            Headshot kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotKills | locale(0)}}
                            ({{entry.headshotKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotDeaths">
                        <a-header>
                            Headshot deaths (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotDeaths | locale(0)}}
                            ({{entry.headshotDeathRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKills">
                        <a-header>
                            Hipfire kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKills | locale(0)}}
                            ({{entry.hipKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKills">
                        <a-header>
                            ADS kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKills | locale(0)}}
                            ({{entry.adsKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamkills">
                        <a-header>
                            Teamkills
                            <info-hover text="How many times you teamkilled this character"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamkills | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamdeaths">
                        <a-header>
                            Teamdeaths
                            <info-hover text="How many times you were teamkilled by this character"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamdeaths | locale(0)}}
                        </a-body>
                    </a-col>
                </a-table>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-2)">
                    Players supported
                </h3>

                <a-table 
                    :entries="characterSupport"
                    :paginate="true"
                    :show-filters="true"
                    :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                    default-sort-field="heals" default-sort-order="desc"
                    class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                            :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 2rem;"></faction-image>
                            <a :href="'/c/' + entry.id" style="color: var(--p-red-2)" class="bright-150">
                                {{entry.displayName}}
                            </a>
                        </a-body>
                    </a-col>

                    <a-col sort-field="heals">
                        <a-header>
                            <b>Healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.heals | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="healthHealed">
                        <a-header>
                            <b>Health healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            <span :title="entry.healthHealed | locale(0)">
                                {{entry.healthHealed | compact}}
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="revives">
                        <a-header>
                            <b>Revived</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.revives | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="resupplies">
                        <a-header>
                            <b>Resupplied</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.resupplies | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="maxRepairs">
                        <a-header>
                            <b>MAX repaired</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.maxRepairs | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="shieldRepairs">
                        <a-header>
                            <b>Shield repairs</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.shieldRepairs | locale}}
                        </a-body>
                    </a-col>

                </a-table>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-3)">
                    Outfits fought
                </h3>

                <a-table :entries="outfitFight"
                         :paginate="true"
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc"
                         class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Outfit</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 2rem;"></faction-image>
                            <span v-if="entry.id.startsWith('0')" style="color: var(--p-red-3)" class="bright-150">
                                {{entry.displayName}}
                            </span>
                            <span v-else>
                                <a :href="'/o/' + entry.id" style="color: var(--p-red-3)" class="bright-150">
                                    {{entry.displayName}}
                                </a>
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            <b>Kills</b>
                            <info-hover text="How many times an input character killed this character."></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                            ({{entry.kills / wrapped.kills.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="deaths">
                        <a-header>
                            <b>Deaths</b>
                            <info-hover text="How many times an input character was killed by this character. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                            ({{entry.deaths / wrapped.deaths.length * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="kd">
                        <a-header>
                            K/D
                            <info-hover text="Kills / Deaths. Revives remove a death"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kd | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotKills">
                        <a-header>
                            Headshot kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotKills | locale(0)}}
                            ({{entry.headshotKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotDeaths">
                        <a-header>
                            Headshot deaths (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotDeaths | locale(0)}}
                            ({{entry.headshotDeathRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKills">
                        <a-header>
                            Hipfire kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKills | locale(0)}}
                            ({{entry.hipKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKills">
                        <a-header>
                            ADS kills (%)
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKills | locale(0)}}
                            ({{entry.adsKillRatio * 100 | locale(2)}}%)
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamkills">
                        <a-header>
                            Teamkills
                            <info-hover text="How many times you teamkilled members of this outfit"></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamkills | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="teamdeaths">
                        <a-header>
                            Teamdeaths
                            <info-hover text="How many times you were teamkilled by members of this outfit "></info-hover>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.teamdeaths | locale(0)}}
                        </a-body>
                    </a-col>
                </a-table>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--p-red-4)">
                    Outfits supported
                </h3>

                <a-table 
                    :entries="outfitSupport"
                    :paginate="true"
                    :show-filters="true"
                    :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                    default-sort-field="heals" default-sort-order="desc"
                    class="border-top-0"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Outfit</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                            :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <faction-image :faction-id="entry.factionID" style="width: 2rem;"></faction-image>
                            <span v-if="entry.id.startsWith('0')" style="color: var(--p-red-4)" class="bright-150">
                                {{entry.displayName}}
                            </span>
                            <span v-else>
                                <a :href="'/o/' + entry.id" style="color: var(--p-red-4)" class="bright-150">
                                    {{entry.displayName}}
                                </a>
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="heals">
                        <a-header>
                            <b>Healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.heals | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="healthHealed">
                        <a-header>
                            <b>Health healed</b>
                        </a-header>

                        <a-body v-slot="entry">
                            <span :title="entry.healthHealed | locale(0)">
                                {{entry.healthHealed | compact}}
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="revives">
                        <a-header>
                            <b>Revived</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.revives | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="resupplies">
                        <a-header>
                            <b>Resupplied</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.resupplies | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="maxRepairs">
                        <a-header>
                            <b>MAX repaired</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.maxRepairs | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="shieldRepairs">
                        <a-header>
                            <b>Shield repairs</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.shieldRepairs | locale}}
                        </a-body>
                    </a-col>

                </a-table>
            </div>
        </div>
    </collapsible>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import FactionImage from "components/FactionImage";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // util
    import CharacterUtils from "util/Character";
    import FactionUtils from "util/Faction";
    import ColorUtils, { RGB } from "util/Color";

    // models
    import { EntityFought, EntitySupported, Entity } from "../common";
    import { PsCharacter } from "api/CharacterApi";
    import { Experience } from "api/ExpStatApi";
    import { PsOutfit } from "api/OutfitApi";
    import { KillEvent } from "api/KillStatApi";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/CompactFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";

    // #E74C3C (231, 76, 60)
    // #F39C12 (243, 158, 18)

    const red: RGB = {
        red: 231,
        green: 76,
        blue: 60
    };

    const green: RGB = {
        red: 243,
        green: 158,
        blue: 18
    };

    export const WrappedViewCharacterInteractions = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                characterFight: Loadable.idle() as Loading<EntityFought[]>,
                characterSupport: Loadable.idle() as Loading<EntitySupported[]>,

                outfitFight: Loadable.idle() as Loading<EntityFought[]>,
                outfitSupport: Loadable.idle() as Loading<EntitySupported[]>,

                colors: [
                    ColorUtils.rgbToString(red),
                    ColorUtils.rgbToString(ColorUtils.colorGradient(0.33, red, green)),
                    ColorUtils.rgbToString(ColorUtils.colorGradient(0.66, red, green)),
                    ColorUtils.rgbToString(green),
                ] as string[]
            }
        },

        created: function(): void {
            this.makeAll();
        },

        methods: {
            makeAll: function(): void {
                this.characterFight = Loadable.loading();
                this.characterSupport = Loadable.loading();
                this.outfitFight = Loadable.loading();
                this.outfitSupport = Loadable.loading();

                // next tick to allow a the loading render to start on the <a-table>s that display this data
                this.$nextTick(() => {
                    this.makeCharacterFight();
                    this.makeSupported();
                });
            },

            getCharacterEntity: function<T extends Entity>(id: string, map: Map<string, T>, create: () => T): T {
                let e: T | undefined = map.get(id);

                if (e == undefined) {
                    const character: PsCharacter | undefined = this.wrapped.characters.get(id);

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
            },

            getOutfitEntity: function<T extends Entity>(id: string, character: PsCharacter, map: Map<string, T>, create: () => T): T {
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
                        const outfit: PsOutfit | undefined = this.wrapped.outfits.get(character.outfitID);
                        if (outfit == undefined) {
                            e.displayName = `<missing ${character.outfitID}>`;
                            e.factionID = 0;
                            e.worldID = 0;
                        } else {
                            e.displayName = `[${outfit.tag}] ${outfit.name}`;

                            const leader: PsCharacter | undefined = this.wrapped.characters.get(outfit.leaderID);
                            if (leader == undefined) {
                                console.warn(`missing outfit leader ID ${outfit.leaderID}`);
                            }
                            e.factionID = leader?.factionID ?? 0;
                            e.worldID = leader?.worldID ?? 0;
                        }
                    }
                }

                return e;
            },

            makeCharacterFight: function(): void {
                const charMap: Map<string, EntityFought> = new Map();
                const outfitMap: Map<string, EntityFought> = new Map();

                const processKill = (kill: KillEvent): void => {
                    const character: PsCharacter | undefined = this.wrapped.characters.get(kill.killedCharacterID);
                    const f: EntityFought = this.getCharacterEntity(kill.killedCharacterID, charMap, () => new EntityFought());
                    processKillSingle(f, kill);

                    charMap.set(kill.killedCharacterID, f);

                    if (character == undefined) {
                        return;
                    }

                    const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
                    const o: EntityFought = this.getOutfitEntity(outfitID, character, outfitMap, () => new EntityFought());
                    processKillSingle(o, kill);

                    outfitMap.set(outfitID, o);
                };

                const processDeath = (death: KillEvent): void => {
                    const character: PsCharacter | undefined = this.wrapped.characters.get(death.attackerCharacterID);
                    const f: EntityFought = this.getCharacterEntity(death.attackerCharacterID, charMap, () => new EntityFought());
                    processDeathSingle(f, death);

                    if (character == undefined) {
                        return;
                    }

                    const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
                    const o: EntityFought = this.getOutfitEntity(outfitID, character, outfitMap, () => new EntityFought());

                    processDeathSingle(o, death);
                }

                const processKillSingle = (entity: EntityFought, ev: KillEvent): void => {
                    if (ev.attackerTeamID != ev.killedTeamID) {
                        ++entity.kills;

                        if (ev.isHeadshot == true) {
                            ++entity.headshotKills;
                        }

                        const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(this.wrapped, ev.attackerFireModeID);
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

                        if (ev.isHeadshot == true) {
                            ++entity.headshotDeaths;
                        }

                        const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(this.wrapped, ev.attackerFireModeID);
                        if (fireModeIndex == 0) {
                            ++entity.hipDeaths;
                        } else if (fireModeIndex == 1) {
                            ++entity.adsDeaths;
                        }
                    } else {
                        ++entity.teamdeaths;
                    }
                };

                for (const kill of this.wrapped.kills) {
                    processKill(kill);
                }

                for (const death of this.wrapped.deaths) {
                    processDeath(death);
                }

                for (const kill of this.wrapped.teamkills) {
                    processKill(kill);
                }

                for (const death of this.wrapped.teamdeaths) {
                    processDeath(death);
                }

                const characterData: EntityFought[] = Array.from(charMap.values()).map((iter: EntityFought) => {
                    return EntityFought.updateRatios(iter);
                });

                const outfitData: EntityFought[] = Array.from(outfitMap.values()).map((iter: EntityFought) => {
                    return EntityFought.updateRatios(iter);
                });

                this.characterFight = Loadable.loaded(characterData);
                this.outfitFight = Loadable.loaded(outfitData);
            },

            makeSupported: function(): void {
                const charMap: Map<string, EntitySupported> = new Map();
                const outfitMap: Map<string, EntitySupported> = new Map();

                for (const exp of this.wrapped.exp) {
                    if (exp.otherID.length != 19) {
                        continue;
                    }

                    const id: string = exp.otherID;
                    const character: PsCharacter | undefined = this.wrapped.characters.get(id);
                    const f: EntitySupported = this.getCharacterEntity(id, charMap, () => new EntitySupported());

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
                    const o: EntitySupported = this.getOutfitEntity(outfitID, character, outfitMap, () => new EntitySupported());

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

                this.characterSupport = Loadable.loaded(Array.from(charMap.values()));
                this.outfitSupport = Loadable.loaded(Array.from(outfitMap.values()));
            }

        },

        components: {
            Collapsible,
            InfoHover,
            FactionImage,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewCharacterInteractions;
</script>