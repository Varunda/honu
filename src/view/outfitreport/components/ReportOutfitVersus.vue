<template>
    <div>
        <h2 class="wt-header">
            Outfit versus
        </h2>

        <table class="table table-sm table-striped">
            <tr class="table-secondary">
                <td>Outfit</td>
                <td>Kills</td>
                <td>Deaths</td>
                <td>KD</td>
            </tr>

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
                <td>{{outfit.kills}}</td>
                <td>{{outfit.deaths}}</td>
                <td>{{outfit.kills / Math.max(1, outfit.deaths) | locale}}</td>
            </tr>
        </table>

        template
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import "filters/LocaleFilter";

    import { PsOutfit } from "api/OutfitApi";
    import { PsCharacter } from "api/CharacterApi";

    class OutfitVersus {
        public id: string = "";
        public name: string = "";
        public tag: string | null = null;

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

                this.versus = [noOutfitVS, noOutfitNC, noOutfitTR, noOutfitNS, ...Array.from(this.map.values())]
                    .filter(iter => iter.kills + iter.deaths > 10)
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

                        this.map.set(character.outfitID, n);
                    }

                    return this.map.get(character.outfitID)!;
                }

                return null;
            }
        }
    });

    export default ReportOutfitVersus;
</script>