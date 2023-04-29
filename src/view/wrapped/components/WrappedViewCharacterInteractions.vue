<template>
    <div>
        <collapsible header-text="Character interactions">
            <div>
                <h3 class="wt-header" style="background-color: var(--blue)">
                    Players fought
                </h3>

                <a-table :entries="characterFight"
                         :paginate="true"
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc">

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <a :href="'/c/' + entry.id">
                                {{entry.displayName}}
                            </a>
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
                </a-table>
            </div>

            <div>
                <h3 class="wt-header" style="background-color: var(--green)">
                    Players supported
                </h3>

                <a-table 
                    :entries="characterSupport"
                    :paginate="true"
                    :show-filters="true"
                    :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                    default-sort-field="heals" default-sort-order="desc"
                >

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                            :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <a :href="'/c/' + entry.id">
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
                            {{entry.healthHealed | locale}}
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
                <h3 class="wt-header" style="background-color: var(--purple)">
                    Outfits fought
                </h3>

                <a-table :entries="outfitFight"
                         :paginate="true"
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc">

                    <a-col sort-field="displayName">
                        <a-header>
                            <b>Outfit</b>
                        </a-header>

                        <a-filter field="displayName" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <span v-if="entry.id.startsWith('0')">
                                {{entry.displayName}}
                            </span>
                            <span v-else>
                                <a :href="'/o/' + entry.id">
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
                </a-table>
            </div>

            <h3 class="wt-header" style="background-color: var(--indigo)">
                Outfits supported
            </h3>

        </collapsible>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // util
    import CharacterUtils from "util/Character";
    import FactionUtils from "util/Faction";

    // models
    import { EntityFought, EntitySupported } from "../common";
    import { PsCharacter } from "api/CharacterApi";
    import { Experience } from "api/ExpStatApi";
    import { PsOutfit } from "api/OutfitApi";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";

    export const WrappedViewCharacterInteractions = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                characterFight: Loadable.idle() as Loading<EntityFought[]>,
                characterSupport: Loadable.idle() as Loading<EntitySupported[]>,

                outfitFight: Loadable.idle() as Loading<EntityFought[]>,
                outfitSupport: Loadable.idle() as Loading<EntitySupported[]>
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

            makeCharacterFight: function(): void {
                const charMap: Map<string, EntityFought> = new Map();
                const outfitMap: Map<string, EntityFought> = new Map();

                for (const kill of this.wrapped.kills) {
                    const character: PsCharacter | undefined = this.wrapped.characters.get(kill.killedCharacterID);

                    let f: EntityFought | undefined = charMap.get(kill.killedCharacterID);
                    if (f == undefined) {
                        f = new EntityFought();
                        f.id = kill.killedCharacterID;
                        f.type = "character";

                        if (character != undefined) {
                            f.displayName = CharacterUtils.getDisplay(character);
                            f.worldID = character.worldID;
                            f.factionID = character.factionID;
                        } else {
                            f.displayName = `<missing ${kill.killedCharacterID}>`;
                        }
                    }

                    ++f.kills;

                    if (kill.isHeadshot == true) {
                        ++f.headshotKills;
                    }

                    const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(this.wrapped, kill.attackerFireModeID);
                    if (fireModeIndex == 0) {
                        ++f.hipKills;
                    } else if (fireModeIndex == 1) {
                        ++f.adsKills;
                    }

                    charMap.set(kill.killedCharacterID, f);

                    if (character == undefined) {
                        continue;
                    }

                    const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
                    let o: EntityFought | undefined = outfitMap.get(outfitID);
                    if (o == undefined) {
                        o = new EntityFought();
                        o.id = outfitID;
                        o.type = "outfit";

                        if (character.outfitID == null) {
                            o.displayName = `<no outfit ${FactionUtils.getName(character.factionID)}>`;
                        } else {
                            const outfit: PsOutfit | undefined = this.wrapped.outfits.get(character.outfitID);
                            if (outfit == undefined) {
                                o.displayName = `<missing ${character.outfitID}>`;
                            } else {
                                o.displayName = `[${outfit.tag}] ${outfit.name}`;
                            }
                        }
                    }

                    ++o.kills;

                    if (kill.isHeadshot == true) {
                        ++o.headshotKills;
                    }

                    if (fireModeIndex == 0) {
                        ++o.hipKills;
                    } else if (fireModeIndex == 1) {
                        ++o.adsKills;
                    }

                    outfitMap.set(outfitID, o);
                }

                for (const death of this.wrapped.deaths) {
                    const character: PsCharacter | undefined = this.wrapped.characters.get(death.attackerCharacterID);

                    let f: EntityFought | undefined = charMap.get(death.attackerCharacterID);
                    if (f == undefined) {
                        f = new EntityFought();
                        f.id = death.attackerCharacterID;
                        f.type = "character";

                        if (character != undefined) {
                            f.displayName = CharacterUtils.getDisplay(character);
                            f.worldID = character.worldID;
                            f.factionID = character.factionID;
                        } else {
                            f.displayName = `<missing ${death.attackerCharacterID}>`;
                        }
                    }

                    ++f.deaths;

                    if (death.isHeadshot == true) {
                        ++f.headshotDeaths;
                    }

                    charMap.set(death.killedCharacterID, f);

                    const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(this.wrapped, death.attackerFireModeID);
                    if (fireModeIndex == 0) {
                        ++f.hipDeaths;
                    } else if (fireModeIndex == 1) {
                        ++f.adsDeaths;
                    }

                    if (character == undefined) {
                        continue;
                    }

                    const outfitID: string = character.outfitID ?? `0-${character.factionID}`;
                    let o: EntityFought | undefined = outfitMap.get(outfitID);
                    if (o == undefined) {
                        o = new EntityFought();
                        o.id = outfitID;
                        o.type = "outfit";

                        if (character.outfitID == null) {
                            o.displayName = `<no outfit ${FactionUtils.getName(character.factionID)}>`;
                        } else {
                            const outfit: PsOutfit | undefined = this.wrapped.outfits.get(character.outfitID);
                            if (outfit == undefined) {
                                o.displayName = `<missing ${character.outfitID}>`;
                            } else {
                                o.displayName = `[${outfit.tag}] ${outfit.name}`;
                            }
                        }
                    }

                    ++o.deaths;

                    if (death.isHeadshot == true) {
                        ++o.headshotDeaths;
                    }

                    if (fireModeIndex == 0) {
                        ++o.hipDeaths;
                    } else if (fireModeIndex == 1) {
                        ++o.adsDeaths;
                    }

                    outfitMap.set(outfitID, o);
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

                    let f: EntitySupported | undefined = charMap.get(id);
                    if (f == undefined) {
                        f = new EntitySupported();
                        f.id = id;
                        f.type = "character";

                        if (character != undefined) {
                            f.displayName = CharacterUtils.getDisplay(character);
                            f.worldID = character.worldID;
                            f.factionID = character.factionID;
                        } else {
                            f.displayName = `<missing ${id}>`;
                        }
                    }

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
                }

                this.characterSupport = Loadable.loaded(Array.from(charMap.values()));
            }

        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewCharacterInteractions;
</script>