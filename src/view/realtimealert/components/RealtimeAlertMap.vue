<template>
    <div>
        <nexus-image name="nexus_base"></nexus-image>
        <nexus-image name="nexus_shading"></nexus-image>
        <nexus-image name="nexus_lattices"></nexus-image>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    type PsFacilityOwner = {
        facilityID: number;
        owner: number;
    }

    type NexusBase = {
        facilityID: number;
        imageBase: string;
        owner: number;
        cutoff: boolean;
        adjacent: number[];
    };

    const NEXUS_WARPGATE_ALPHA: number = 310570;
    const NEXUS_ARAZEK: number = 310500;
    const NEXUS_ARGENT: number = 310540;
    const NEXUS_BITTER_GORGE: number = 310590;
    const NEXUS_GRANITEHEAD: number = 310530;
    const NEXUS_SECURE: number = 310510;
    const NEXUS_HYDRO: number = 310520;
    const NEXUS_SLATE: number = 310550;
    const NEXUS_WARPGATE_OMEGA: number = 310560;
    const NEXUS_ALPHA: number = 310600;
    const NEXUS_OMEGA: number = 310610;

    const NexusImage = Vue.extend({
        props: {
            name: { type: String, required: true },
        },

        template: `<img width=600 height=600 :src="'/img/nexus/' + name + '_1000x1000.png'" :style="'position: absolute;'" />`
    });

    function makeBase(facilityID: number, imageBase: string, adjacent: number[]): NexusBase {
        return {
            facilityID: facilityID,
            imageBase: imageBase,
            owner: 0,
            cutoff: false,
            adjacent: adjacent
        };
    }

    const nexus: Map<number, NexusBase> = new Map([
        [NEXUS_WARPGATE_ALPHA, makeBase(NEXUS_WARPGATE_ALPHA, "nexus_north", [])]
    ]);

    const facilityId: Map<number, string> = new Map([
        [NEXUS_WARPGATE_ALPHA, "nexus_north"],
        [NEXUS_ARAZEK, "nexus_arazek"],
        [NEXUS_ARGENT, "nexus_argent"],
        [NEXUS_BITTER_GORGE, "nexus_bitter"],
        [NEXUS_GRANITEHEAD, "nexus_granite"],
        [NEXUS_SECURE, "nexus_secure"],
        [NEXUS_HYDRO, "nexus_hydro"],
        [NEXUS_SLATE, "nexus_slate"],
        [NEXUS_WARPGATE_OMEGA, "nexus_south"],
        [NEXUS_ALPHA, "nexus_alpha"],
        [NEXUS_OMEGA, "nexus_omega"]
    ]);

    export const RealtimeAlertMap = Vue.extend({
        props: {
            bases: { type: Array as PropType<PsFacilityOwner[]>, required: true },
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            imgAlphaWarpgate: function(): string {
                return "";
            },

        },

        components: {
            NexusImage
        }
    });
    export default RealtimeAlertMap;
</script>