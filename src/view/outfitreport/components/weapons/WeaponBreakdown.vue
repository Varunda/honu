<template>
    <div class="d-flex">
        <table class="table table-sm flex-grow-1 flex-basis-0 align-self-start" style="min-width: 0;">
            <tr class="table-secondary th-border-top-0">
                <td>Weapon</td>
                <td>Kills (%)</td>
                <td>HSR %</td>
                <td>Hip %</td>
                <td>ADS %</td>
            </tr>

            <tr v-for="weapon in entries.slice(0, sliceSize)">
                <td>
                    <a v-if="LinkItem == true && weapon.id != 0" :href="'/i/' + weapon.id">
                        {{weapon.name}}
                    </a>

                    <span v-else>
                        {{weapon.name}}
                    </span>
                </td>
                <td>
                    {{weapon.kills}}
                    ({{weapon.kills / Math.max(1, total) * 100 | locale}}%)
                </td>
                <td>
                    {{weapon.headshotKills | locale}}
                    ({{weapon.headshotKills / Math.max(1, weapon.kills) * 100 | locale}}%)
                </td>
                <td>
                    <span v-if="weapon.id == 0">
                        --
                    </span>
                    <span v-else>
                        {{weapon.hipKills | locale}}
                        ({{weapon.hipKills / Math.max(1, weapon.kills) * 100 | locale}}%)
                    </span>
                </td>
                <td>
                    <span v-if="weapon.id == 0">
                        --
                    </span>
                    <span v-else>
                        {{weapon.adsKills}}
                        ({{weapon.adsKills / Math.max(1, weapon.kills) * 100 | locale}}%)
                    </span>
                </td>
            </tr>

            <tr class="table-dark">
                <td colspan="2">
                    {{entries.length}} entries over {{total}}
                </td>

                <td>
                    {{totalHeadshotKills}}
                    ({{totalHeadshotKills / Math.max(1, total) * 100 | locale}}%)
                </td>

                <td>
                    {{totalHipKills}}
                    ({{totalHipKills / Math.max(1, total) * 100 | locale}}%)
                </td>

                <td>
                    {{totalAdsKills}}
                    ({{totalAdsKills / Math.max(1, total) * 100 | locale}}%)
                </td>
            </tr>
        </table>

        <div class="flex-grow-1 flex-basis-0" style="min-width: 0;">
            <chart-block-pie-chart :data="block"
                :show-percent="true" :show-total="true">
            </chart-block-pie-chart>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { WeaponBreakdownEntry } from "./common";
    import { Block, BlockEntry } from "../charts/common";
    import ChartBlockPieChart from "../charts/ChartBlockPieChart.vue";

    export const WeaponBreakdown = Vue.extend({
        props: {
            entries: { type: Array as PropType<WeaponBreakdownEntry[]>, required: true },
            block: { type: Object as PropType<Block>, required: true },
            LinkItem: { type: Boolean, required: true },
            total: { type: Number, required: true },
            HeadshotTotal: { type: Number, required: true }
        },

        data: function() {
            return {
                sliceSize: 10 as number
            }
        },

        methods: {

        },

        computed: {
            totalHeadshotKills: function(): number {
                return this.entries.reduce((acc, iter) => acc += iter.headshotKills, 0);
            },

            totalHipKills: function(): number {
                return this.entries.reduce((acc, iter) => acc += iter.hipKills, 0);
            },

            totalAdsKills: function(): number {
                return this.entries.reduce((acc, iter) => acc += iter.adsKills, 0);
            },
        },

        components: {
            ChartBlockPieChart
        }
    });
    export default WeaponBreakdown;
</script>