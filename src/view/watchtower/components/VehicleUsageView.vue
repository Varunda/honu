
<template>
    <div>
        <h5 class="text-center">
            Vehicle usage estimates
            <span class="h6">
                (not the number of vehicles)
            </span>
        </h5>

        <div class="mb-2 mx-2">
            <div class="mb-1">
                Honu estimates that <b>{{data.totalVehicles}}</b> of <b>{{data.total}}</b> players are in a vehicle of some kinda.
            </div>
            <div class="mb-1">
                Honu this data based on kills (vehicle weapons), and experience events (gunner assists, repairing vehicles, etc.)
            </div>
            <div class="text-warning">not 100% accurate!</div>
        </div>

        <table class="table table-sm">
            <tr>
                <td>
                    <census-image :image-id="getImageId(0)" style="height: 2rem;"></census-image>
                    Infantry:
                </td>
                <td class="vu-count">
                    {{infantryCount}}
                </td>
            </tr>

            <tr>
                <td>
                    <census-image :image-id="getImageId(-1)" style="height: 2rem;"></census-image>
                    Unknown
                    <info-hover text="when Honu knows someone is in a vehicle, but not sure which one, for example prox repairs can be a sunderer, galaxy or corsair"></info-hover>
                    :
                </td>

                <td class="vu-count">
                    {{unknownCount}}
                </td>
            </tr>

            <tr v-for="other in otherCounts">
                <td>
                    <census-image :image-id="getImageId(other.vehicleID)" style="height: 2rem;"></census-image>
                    {{other.vehicleName}}
                </td>

                <td class="vu-count">
                    {{other.count}}
                </td>
            </tr>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import CensusImage from "components/CensusImage";
    import InfoHover from "components/InfoHover.vue";

    import { VehicleUsageEntry, VehicleUsageFaction } from "api/VehicleUsageApi";

    export const VehicleUsageView = Vue.extend({
        props: {
            data: { type: Object as PropType<VehicleUsageFaction>, required: true }
        },

        data: function() {
            return {
                vehicleIdImageMapping: new Map([
                    [0, 31], // 0 => infantry
                    [-1, 4], // -1 => unknown
                    [1, 260], // 1 => flash
                    [2, 264], // 2 => sunderer
                    [3, 258], // 3 => lightning
                    [4, 259], // 4 => magrider
                    [5, 265], // 5 => vanguard
                    [6, 261], // 6 => prowler
                    [7, 266], // 7 => scythe
                    [8, 263], // 8 => reaver
                    [9, 260], // 9 => mosquito
                    [10, 257], // 10 => liberator
                    [11, 256], // 11 => galaxy
                    [12, 8852], // 12 => harasser
                    [13, 264], // 13 => drop pod
                    [14, 79711], // 14 => valkyrie
                    [15, 84726], // 15 => ant
                    [163, 264], // 163 => glaive
                    [1001, 260], // 1001 => flash
                    [1010, 258], // 1010 => liberator
                    [1011, 256], // 1011 => galaxy
                    [2007, 92801], // 2007 => colossus
                    [2019, 92392], // 2019 => bastion
                    [2033, 92332], // 2033 => javelin
                    [2130, 92332], // 2130 => another javelin
                    [2136, 93607], // 2136 => dervish
                    [2137, 93604], // 2137 => chimera
                    [2142, 95012], // 2142 => corsair
                ]) as Map<number, number>
            }
        },

        methods: {

            getImageId: function(vehicleID: number): number {
                return this.vehicleIdImageMapping.get(vehicleID) ?? 4;
            }
        },

        computed: {

            infantryCount: function(): number {
                return this.data.usage.get(0)?.count ?? 0;
            },

            unknownCount: function(): number {
                return this.data.usage.get(-1)?.count ?? 0;
            },

            otherCounts: function(): VehicleUsageEntry[] {
                // 0 is not in a vehicle
                // -1 is in a vehicle, but not known which one
                return Array.from(this.data.usage.values())
                    .filter(iter => iter.vehicleID != 0 && iter.vehicleID != -1)
                    .sort((a, b) => b.count - a.count);
            }

        },

        components: {
            CensusImage,
            InfoHover
        }

    });
    export default VehicleUsageView;

</script>