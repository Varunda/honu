<template>
    <div>
        <h2 class="wt-header">
            Outfit versus
            <info-hover text="An outfit is only included if it is at least 1% of all kills & deaths"></info-hover>
        </h2>

        <table class="table table-sm table-hover">
            <thead>
                <tr class="table-secondary font-weight-bold">
                    <td>Outfit</td>
                    <td>
                        Report link
                        <info-hover text="Get a report for this outfit, at the same time"></info-hover>
                    </td>
                    <td>Kills</td>
                    <td>Deaths</td>
                    <td>Assists</td>
                    <td>KD</td>
                    <td>KDA</td>
                    <td>
                        HSR% kills
                        <info-hover text="What percent of kills came from headshots against this outfit"></info-hover>
                    </td>
                    <td>
                        HSR% deaths
                        <info-hover text="What percent of deaths came from headshots from this outfit"></info-hover>
                    </td>
                    <td>
                        Most used weapon
                        <info-hover text="What weapon in this outfit killed the most players in this report"></info-hover>
                    </td>
                </tr>
            </thead>

            <tbody>
                <tr v-for="outfit in versus">
                    <td>
                        <span v-if="outfit.id">
                            <a :href="'/o/' + outfit.id">
                                [{{outfit.tag}}]
                                {{outfit.name}}
                            </a>
                        </span>
                        <span v-else>
                            {{outfit.name}}
                        </span>
                    </td>

                    <td>
                        <span v-if="outfit.id">
                            <a :href="'/report/' + outfit.generator">
                                View report
                            </a>
                        </span>
                        <span v-else>
                            --
                        </span>
                    </td>

                    <td>
                        {{outfit.kills}}
                        ({{outfit.kills / Math.max(1, kills) * 100 | locale(2)}}%)
                    </td>
                    <td>
                        {{outfit.deaths}}
                        ({{outfit.deaths / Math.max(1, deaths) * 100 | locale(2)}}%)
                    </td>
                    <td>
                        {{outfit.assists}}
                        ({{outfit.assists / Math.max(1, assists) * 100 | locale(2)}}%)
                    </td>

                    <td>
                        {{outfit.kills / Math.max(1, outfit.deaths) | locale(2)}}
                    </td>

                    <td>
                        {{(outfit.kills + outfit.assists) / Math.max(1, outfit.deaths) | locale(2)}}
                    </td>

                    <td>
                        {{outfit.headshotKills / outfit.kills * 100 | locale(2)}}%
                    </td>

                    <td>
                        {{outfit.headshotDeaths / Math.max(1, outfit.deaths) * 100 | locale(2)}}%
                    </td>

                    <td>
                        <a :href="'/i/' + outfit.mostUsedWeaponID">
                            <span v-if="outfit.mostUsedWeapon != null">
                                {{outfit.mostUsedWeapon.name}}
                            </span>
                            <span v-else>
                                &lt;missing {{outfit.mostUsedWeaponID}}&gt;
                            </span>
                        </a>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import InfoHover from "components/InfoHover.vue";

    import "filters/LocaleFilter";

    import { PsOutfit } from "api/OutfitApi";
    import { PsCharacter } from "api/CharacterApi";
    import { Experience } from "api/ExpStatApi";
    import { PsItem } from "api/ItemApi";

    class OutfitVersus {
        public id: string = "";
        public name: string = "";
        public tag: string | null = null;

        public generator: string = "";

        public kills: number = 0;
        public deaths: number = 0;
        public assists: number = 0;
        public headshotKills: number = 0;
        public headshotDeaths: number = 0;

        public weapons: Map<string, number> = new Map();
        public mostUsedWeaponID: string = "";
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
            report: { type: Object as PropType<Report>, required: true }
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
                    const weapons: string[] = Array.from(value.weapons.entries())
                        .sort((a, b) => b[1] - a[1])
                        .map(iter => iter[0]);

                    value.mostUsedWeaponID = weapons[0];
                    value.mostUsedWeapon = this.report.items.get(Number.parseInt(weapons[0])) || null;
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
                        n.id = character.outfitID;
                        n.name = character.outfitName!;
                        n.tag = character.outfitTag;

                        const gen: string = btoa(`${this.report.generator.split(';')[0]};o${n.id};`);
                        n.generator = gen;

                        this.map.set(character.outfitID, n);
                    }

                    return this.map.get(character.outfitID)!;
                }

                return null;
            }
        },

        components: {
            InfoHover
        }
    });

    export default ReportOutfitVersus;
</script>