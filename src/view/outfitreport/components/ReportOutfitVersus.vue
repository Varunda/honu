<template>
    <div>
        <h2 class="wt-header">
            Outfit versus
            <info-hover text="An outfit is only included if it is at least 1% of all kills & deaths"></info-hover>
        </h2>

        <table class="table table-sm table-hover">
            <thead>
                <tr class="table-secondary">
                    <td>Outfit</td>
                    <td>
                        Report link
                        <info-hover text="Get a report for this outfit, at the same time"></info-hover>
                    </td>
                    <td>Kills</td>
                    <td>Deaths</td>
                    <td>KD</td>
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
                        ({{outfit.kills / kills * 100 | locale}}%)
                    </td>
                    <td>
                        {{outfit.deaths}}
                        ({{outfit.deaths / deaths * 100 | locale}}%)
                    </td>
                    <td>{{outfit.kills / Math.max(1, outfit.deaths) | locale}}</td>
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

    class OutfitVersus {
        public id: string = "";
        public name: string = "";
        public tag: string | null = null;

        public generator: string = "";

        public kills: number = 0;
        public deaths: number = 0;
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
                deaths: 0 as number
            }
        },

        methods: {
            make: function(): void {
                for (const kill of this.report.kills) {
                    const v: OutfitVersus | null = this.getOutfit(kill.killedCharacterID);
                    if (v != null) {
                        ++v.kills;
                    }
                }

                for (const death of this.report.deaths) {
                    const v: OutfitVersus | null = this.getOutfit(death.attackerCharacterID);
                    if (v != null) {
                        ++v.deaths;
                    }
                }

                this.kills = this.report.kills.length;
                this.deaths = this.report.deaths.length;

                this.versus = [noOutfitVS, noOutfitNC, noOutfitTR, noOutfitNS, ...Array.from(this.map.values())]
                    .filter(iter => ((iter.kills + iter.deaths) / (this.kills + this.deaths))  > 0.01)
                    .sort((a, b) => (b.kills + a.deaths) - (a.kills + a.deaths));
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