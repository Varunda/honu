<template>
    <img :src="'/img/' + factionBase + '_' + colorSuffix + '.png'" width="24" height="24" />
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsOutfit } from "api/OutfitApi";

    export const TeamIcon = Vue.extend({
        props: {
            outfit: { type: Object as PropType<PsOutfit>, required: true },
            teamId: { type: Number, required: true }
        },

        computed: {
            factionBase: function(): string {
                if (this.outfit.factionID == 1) {
                    return "vs";
                } else if (this.outfit.factionID == 2) {
                    return "nc";
                } else if (this.outfit.factionID == 3) {
                    return "tr";
                } else if (this.outfit.factionID == 4) {
                    return "ns";
                }
                return "";
            },

            colorSuffix: function(): string {
                if (this.teamId == 2) {
                    return "blue";
                } else if (this.teamId == 3) {
                    return "red";
                }
                return "";
            }
        }
    });
    export default TeamIcon;
</script>