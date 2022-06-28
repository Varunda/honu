<template>
    <div class="d-flex">
        <table class="table table-sm flex-grow-1 flex-basis-0">
            <tr class="table-secondary th-border-top-0">
                <td>Weapon</td>
                <td>Kills</td>
                <td>%</td>
                <td>HSR%</td>
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
                <td>{{weapon.kills}}</td>
                <td>{{weapon.kills / Math.max(1, total) * 100 | locale}}%</td>
                <td>{{weapon.headshotKills / Math.max(1, weapon.kills) * 100 | locale}}%</td>
            </tr>

            <tr class="table-dark">
                <td colspan="4">
                    {{entries.length}} entries over {{total}}
                    ({{HeadshotTotal / Math.max(1, total) * 100 | locale}}% HSR)
                </td>
            </tr>
        </table>

        <div class="flex-grow-1 flex-basis-0">
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

        components: {
            ChartBlockPieChart
        }
    });
    export default WeaponBreakdown;
</script>