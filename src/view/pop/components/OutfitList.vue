<template>
    <table class="table">
        <thead>
            <tr>
                <th>Outfit</th>
                <th>Online</th>
            </tr>
        </thead>

        <tbody v-if="outfits.state == 'loaded'">
            <tr v-for="outfit in entries">
                <td>
                    [{{outfit.outfitTag}}]
                    {{outfit.outfitName}}
                </td>

                <td>
                    {{outfit.count}}
                </td>
            </tr>
        </tbody>
    </table>

</template>

<script lang="ts">
    import Vue from "vue";

    export const OutfitList = Vue.extend({
        props: {
            outfits: { type: Object, required: true },
            FactionId: { type: Number, required: true }
        },

        computed: {
            entries: function() {
                if ((this.outfits as any).state != "loaded") {
                    return [];
                }

                return [...this.outfits.data.filter((iter: any) => iter.factionID == this.FactionId)]
                    .sort((a, b) => b.count - a.count);
            }
        }
    });
    export default OutfitList;
</script>