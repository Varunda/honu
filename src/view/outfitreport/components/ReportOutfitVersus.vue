<template>
    <collapsible header-text="Outfit Versus">

        <a-table 
            :entries="entries" default-sort-field="kills" default-sort-order="desc"
            :paginate="false" :striped="true" :filter="false" :show-filters="false"
            display-type="table" row-padding="compact">

            <a-col sort-field="name">
                <a-header>
                    <b>Outfit</b>
                </a-header>

                <a-body v-slot="outfit">
                    <span v-if="outfit.id">
                        <a :href="'/o/' + outfit.id">
                            [{{outfit.tag}}]
                            {{outfit.name}}

                            ({{outfit.factionID | faction}})
                        </a>
                    </span>
                    <span v-else>
                        {{outfit.name}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Report link</b>
                    <info-hover text="Get a report for this outfit, at the same time"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    <span v-if="outfit.id">
                        <a :href="'/report/' + outfit.generator">
                            View report
                        </a>
                    </span>
                    <span v-else>
                        --
                    </span>
                </a-body>
            </a-col>
            
            <a-col sort-field="kills">
                <a-header>
                    <b>Kills</b>
                    <info-hover text="How many kills the tracked players got against players in this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.kills}}
                    ({{outfit.kills / Math.max(1, kills) * 100 | locale(2)}}%)
                </a-body>
            </a-col>

            <a-col sort-field="deaths">
                <a-header>
                    <b>Deaths</b>
                    <info-hover text="How many deaths the tracked players had against players in this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.deaths}}
                    ({{outfit.deaths / Math.max(1, deaths) * 100 | locale(2)}}%)
                </a-body>
            </a-col>

            <a-col sort-field="assists">
                <a-header>
                    <b>Assists</b>
                    <info-hover text="How many assists the tracked players got against players in this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.assists}}
                    ({{outfit.assists / Math.max(1, assists) * 100 | locale(2)}}%)
                </a-body>
            </a-col>

            <a-col sort-field="uniqueCount">
                <a-header>
                    <b>Players</b>
                    <info-hover text="How many unique players tracked members encountered against this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.uniquePlayers.length}}
                </a-body>
            </a-col>

            <a-col sort-field="kd">
                <a-header>
                    <b>K/D</b>
                    <info-hover text="How many deaths the tracked players had against players in this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.kd | locale(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="kda">
                <a-header>
                    <b>K+A/D</b>
                    <info-hover text="Kills + Assists / Deaths"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.kda | locale(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="killHsr">
                <a-header>
                    HSR% kills
                    <info-hover text="What percent of kills came from headshots against this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.killHsr | locale(2)}}%
                </a-body>
            </a-col>

            <a-col sort-field="deathHsr">
                <a-header>
                    HSR% deaths
                    <info-hover text="What percent of kills came from headshots against this outfit"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    {{outfit.deathHsr | locale(2)}}%
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Most used weapon</b>
                    <info-hover text="What weapon in this outfit killed the most players in this report"></info-hover>
                </a-header>

                <a-body v-slot="outfit">
                    <a :href="'/i/' + outfit.mostUsedWeaponID">
                        <span v-if="outfit.mostUsedWeapon != null">
                            {{outfit.mostUsedWeapon.name}}
                        </span>
                        <span v-else>
                            &lt;missing {{outfit.mostUsedWeaponID}}&gt;
                        </span>
                    </a>
                </a-body>
            </a-col>
        </a-table>

    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { ReportParameters } from "../Report";

    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";

    import { PsOutfit } from "api/OutfitApi";
    import { PsCharacter } from "api/CharacterApi";
    import { Experience } from "api/ExpStatApi";
    import { PsItem } from "api/ItemApi";

    import { Loading, Loadable } from "Loading";

    class OutfitVersus {
        public id: string = "";
        public name: string = "";
        public tag: string | null = null;
        public factionID: number = 0;
        public uniquePlayers: string[] = [];
        public uniqueCount: number = 0;

        public generator: string = "";

        public kills: number = 0;
        public deaths: number = 0;
        public assists: number = 0;
        public headshotKills: number = 0;
        public headshotDeaths: number = 0;

        public kd: number = 0;
        public kda: number = 0;
        public killHsr: number = 0;
        public deathHsr: number = 0;

        public weapons: Map<number, number> = new Map();
        public mostUsedWeaponID: number = 0;
        public mostUsedWeapon: PsItem | null = null;
    }

    const noOutfitVS: OutfitVersus = new OutfitVersus();
    noOutfitVS.name = "<no outfit VS>";

    const noOutfitNC: OutfitVersus = new OutfitVersus();
    noOutfitNC.name = "<no outfit NC>";

    const noOutfitTR: OutfitVersus = new OutfitVersus();
    noOutfitTR.name = "<no outfit TR>";

    const noOutfitNS: OutfitVersus = new OutfitVersus();
    noOutfitNS.name = "<no outfit NS>";

    export const ReportOutfitVersus = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        created: function(): void {
            this.make();
        },

        data: function() {
            return {
                versus: [] as OutfitVersus[],
                map: new Map() as Map<string, OutfitVersus>,

                kills: 0 as number,
                deaths: 0 as number,
                assists: 0 as number,
            }
        },

        methods: {
            make: function(): void {
                for (const kill of this.report.kills) {
                    const v: OutfitVersus | null = this.getOutfit(kill.killedCharacterID);
                    if (v != null) {
                        ++v.kills;
                        if (kill.isHeadshot == true) {
                            ++v.headshotKills;
                        }

                        if (v.uniquePlayers.indexOf(kill.killedCharacterID) == -1) {
                            v.uniquePlayers.push(kill.killedCharacterID);
                        }
                    }
                }

                for (const death of this.report.deaths) {
                    const v: OutfitVersus | null = this.getOutfit(death.attackerCharacterID);
                    if (v != null) {
                        ++v.deaths;
                        if (death.isHeadshot == true) {
                            ++v.headshotDeaths;
                        }
                        v.weapons.set(death.weaponID, (v.weapons.get(death.weaponID) ?? 0) + 1);

                        if (v.uniquePlayers.indexOf(death.attackerCharacterID) == -1) {
                            v.uniquePlayers.push(death.attackerCharacterID);
                        }
                    }
                }

                for (const exp of this.report.experience) {
                    if (Experience.isAssist(exp.experienceID) == false) {
                        continue;
                    }

                    const v: OutfitVersus | null = this.getOutfit(exp.otherID);
                    if (v != null) {
                        ++v.assists;
                    }
                }

                this.kills = this.report.kills.length;
                this.deaths = this.report.deaths.length;
                this.assists = this.report.experience.filter(iter => Experience.isAssist(iter.experienceID)).length;

                this.versus = [noOutfitVS, noOutfitNC, noOutfitTR, noOutfitNS, ...Array.from(this.map.values())]
                    .filter(iter => ((iter.kills + iter.deaths) / (this.kills + this.deaths))  > 0.01)
                    .sort((a, b) => (b.kills + a.deaths) - (a.kills + a.deaths));

                for (const value of this.versus) {
                    const weapons: number[] = Array.from(value.weapons.entries())
                        .sort((a, b) => b[1] - a[1])
                        .map(iter => iter[0]);

                    value.mostUsedWeaponID = weapons[0];
                    value.mostUsedWeapon = this.report.items.get(weapons[0]) || null;

                    value.uniqueCount = value.uniquePlayers.length;

                    value.kd = value.kills / Math.max(1, value.deaths);
                    value.kda = (value.kills + value.assists) / Math.max(1, value.deaths);
                    value.killHsr = value.headshotKills / Math.max(1, value.kills) * 100;
                    value.deathHsr = value.headshotDeaths / Math.max(1, value.deaths) * 100;
                }

            },

            getOutfit: function(charID: string) : OutfitVersus | null {
                if (this.report.characters.has(charID)) {
                    const character: PsCharacter = this.report.characters.get(charID)!;

                    if (character.outfitID == null || character.outfitID == "" || character.outfitID == "0") {
                        if (character.factionID == 1) {
                            return noOutfitVS;
                        } else if (character.factionID == 2) {
                            return noOutfitNC;
                        } else if (character.factionID == 3) {
                            return noOutfitTR;
                        } else if (character.factionID == 4) {
                            return noOutfitNS;
                        }
                        throw `Unchecked factionID ${character.factionID}`;
                    }

                    if (this.map.has(character.outfitID) == false) {
                        const n: OutfitVersus = new OutfitVersus();

                        const outfit: PsOutfit | null = this.report.outfits.get(character.outfitID) || null;

                        n.id = character.outfitID;
                        n.name = character.outfitName!;
                        n.tag = character.outfitTag;
                        n.factionID = outfit?.factionID ?? -1;

                        const start: number = Math.floor(this.parameters.periodStart.getTime() / 1000);
                        const end: number = Math.floor(this.parameters.periodEnd.getTime() / 1000);

                        const gen: string = btoa(`${start},${end},${n.factionID};o${n.id};`);
                        n.generator = gen;

                        this.map.set(character.outfitID, n);
                    }

                    return this.map.get(character.outfitID)!;
                }

                return null;
            }
        },

        computed: {
            entries: function(): Loading<OutfitVersus[]> {
                return Loadable.loaded(this.versus);
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            Collapsible
        }
    });

    export default ReportOutfitVersus;
</script>