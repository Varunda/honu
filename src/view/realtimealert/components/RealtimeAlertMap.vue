<template>
    <div>
        <nexus-image name="nexus_base"></nexus-image>

        <nexus-image :name="getImage(AlphaWarpgate)"></nexus-image>
        <nexus-image :name="getImage(OmegaWarpgate)"></nexus-image>

        <nexus-image :name="getImage(Arazek)"></nexus-image>
        <nexus-image :name="getImage(Argent)"></nexus-image>
        <nexus-image :name="getImage(Bitter)"></nexus-image>
        <nexus-image :name="getImage(Granite)"></nexus-image>
        <nexus-image :name="getImage(Secure)"></nexus-image>
        <nexus-image :name="getImage(Slate)"></nexus-image>
        <nexus-image :name="getImage(Hydro)"></nexus-image>

        <nexus-image :name="getImage(Alpha)"></nexus-image>
        <nexus-image :name="getImage(Omega)"></nexus-image>

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

    /*
    // Actual Nexus data
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
    */

    // Test Amerish data
    const NEXUS_WARPGATE_ALPHA: number = 222180; // chimney
    const NEXUS_ARAZEK: number = 206002; // heyoka chem
    const NEXUS_ARGENT: number = 206000; // heyoka proper
    const NEXUS_BITTER_GORGE: number = 400129; // cobalt geo
    const NEXUS_GRANITEHEAD: number = 222340; // scarfield
    const NEXUS_SECURE: number = 210001; // wokuk eco
    const NEXUS_HYDRO: number = 210000; // wokuk proper
    const NEXUS_SLATE: number = 210002; // wokuk shipping
    const NEXUS_WARPGATE_OMEGA: number = 220000; // west pass
    const NEXUS_ALPHA: number = 210003; // wokuk watchtower
    const NEXUS_OMEGA: number = 222220; // torremar

    const NexusImage = Vue.extend({
        props: {
            name: { type: String },
        },

        template: `<img v-if="name != null" width=600 height=600 :src="'/img/nexus/' + name + '_1000x1000.png'" :style="'position: absolute;'" />`
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

    export const RealtimeAlertMap = Vue.extend({
        props: {
            bases: { type: Array as PropType<PsFacilityOwner[]>, required: true },
        },

        data: function() {
            return {
                nexus: new Map([
                    [NEXUS_WARPGATE_ALPHA, makeBase(NEXUS_WARPGATE_ALPHA, "nexus_north", [])],
                    [NEXUS_ARAZEK, makeBase(NEXUS_ARAZEK, "nexus_arazek", [])],
                    [NEXUS_ARGENT, makeBase(NEXUS_ARGENT, "nexus_argent", [])],
                    [NEXUS_BITTER_GORGE, makeBase(NEXUS_BITTER_GORGE, "nexus_bitter", [])],
                    [NEXUS_GRANITEHEAD, makeBase(NEXUS_GRANITEHEAD, "nexus_granite", [])],
                    [NEXUS_SECURE, makeBase(NEXUS_SECURE, "nexus_secure", [])],
                    [NEXUS_HYDRO, makeBase(NEXUS_HYDRO, "nexus_hydro", [])],
                    [NEXUS_SLATE, makeBase(NEXUS_SLATE, "nexus_slate", [])],
                    [NEXUS_WARPGATE_OMEGA, makeBase(NEXUS_WARPGATE_OMEGA, "nexus_south", [])],
                    [NEXUS_ALPHA, makeBase(NEXUS_ALPHA, "nexus_alpha", [])],
                    [NEXUS_OMEGA, makeBase(NEXUS_OMEGA, "nexus_omega", [])],
                ]) as Map<number, NexusBase>
            }
        },

        methods: {
            getImage: function(baseID: number): string | null {
                const b: NexusBase = this.nexus.get(baseID)!;

                if (b.owner != 2 && b.owner != 3) {
                    return null;
                }

                const color: string = b.owner == 2 ? "blue" : "red";

                return `${b.imageBase}_${color}`;
            }

        },

        watch: {
            bases: function(): void {
                for (const base of this.bases) {
                    const b: NexusBase | undefined = this.nexus.get(base.facilityID);
                    if (b != undefined) {
                        b.owner = base.owner;
                        this.nexus.set(base.facilityID, b);
                    }
                }
                this.$forceUpdate();
            }
        },

        computed: {
            AlphaWarpgate: function(): number { return NEXUS_WARPGATE_ALPHA; },
            OmegaWarpgate: function(): number { return NEXUS_WARPGATE_OMEGA; },
            Arazek: function(): number { return NEXUS_ARAZEK; },
            Argent: function(): number { return NEXUS_ARGENT; },
            Bitter: function(): number { return NEXUS_BITTER_GORGE; },
            Granite: function(): number { return NEXUS_GRANITEHEAD; },
            Secure: function(): number { return NEXUS_SECURE; },
            Hydro: function(): number { return NEXUS_HYDRO; },
            Slate: function(): number { return NEXUS_SLATE; },
            Alpha: function(): number { return NEXUS_ALPHA; },
            Omega: function(): number { return NEXUS_OMEGA; },
        },

        components: {
            NexusImage
        }
    });
    export default RealtimeAlertMap;
</script>