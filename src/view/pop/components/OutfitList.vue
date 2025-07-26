<template>
    <div>
        <h3 class="text-center">
            <faction :faction-id="FactionId" style="height: 48px"></faction>
            Outfits on 
            {{ FactionId | faction }}
        </h3>

        <table class="table">
            <thead>
                <tr>
                    <th>Outfit</th>
                    <th>Online ({{ sum }})</th>
                </tr>
            </thead>

            <tbody v-if="outfits.state == 'loaded'">
                <tr v-for="outfit in entries" :key="outfit.outfitID">
                    <td>
                        <span v-if="outfit.outfitID == '0'">
                            No outfit
                        </span>
                        <span v-else>
                            <a :href="'/o/' + outfit.outfitID">
                                [{{outfit.outfitTag}}]
                                {{outfit.outfitName}}
                            </a>
                        </span>
                    </td>

                    <td>
                        {{outfit.count}}
                        ({{ outfit.count / sum * 100 | locale(2) }}%)
                    </td>
                </tr>
            </tbody>

            <tbody v-else-if="outfits.state == 'loading'">
                <tr colspan="2">
                    <td>
                        Loading...
                    </td>
                </tr>
            </tbody>
        </table>
    </div>

</template>

<script lang="ts">
    import Vue from "vue";

    import { OutfitPopulation } from "api/OutfitPopulationApi";
    import Faction from "components/FactionImage";

    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";

    export const OutfitList = Vue.extend({
        props: {
            outfits: { type: Object, required: true },
            FactionId: { type: Number, required: true }
        },

        computed: {
            entries: function(): OutfitPopulation[] {
                if ((this.outfits as any).state != "loaded") {
                    return [];
                }

                return [...this.outfits.data.filter((iter: any) => iter.factionID == this.FactionId)]
                    .sort((a, b) => b.count - a.count);
            },

            sum: function(): number {
                return this.entries.reduce((acc: number, iter: OutfitPopulation) => acc += iter.count, 0);
            }
        },

        components: {
            Faction
        }
    });
    export default OutfitList;
</script>