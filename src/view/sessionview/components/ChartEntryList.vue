<template>
    <table class="table table-sm">
        <tr class="table-secondary th-border-top-0">
            <th>{{LeftColumnTitle}}</th>
            <th>{{MiddleColumnTitle}}</th>
            <th>%</th>
        </tr>

        <tr v-for="entry in shown">
            <td>
                <a v-if="entry.link != null" :href="entry.link">
                    {{entry.display}}
                </a>
                <span v-else>
                    {{entry.display}}
                </span>
            </td>
            <td>{{entry.count | locale}}</td>
            <td>{{entry.count / total * 100 | locale(2)}}%</td>
        </tr>

        <tr v-if="data.length > MaxEntries">
            <td>Other</td>
            <td>{{hiddenTotal | locale}}</td>
            <td>{{hiddenTotal / total * 100 | locale(2)}}%</td>
        </tr>

        <tr class="table-secondary th-border-top-0">
            <td>Total</td>
            <td colspan="2">{{total | locale}}</td>
        </tr>
    </table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    interface Entry {
        display: string;
        count: number;
        link: string | null;
    }

    export const ChartEntryPie = Vue.extend({
        props: {
            data: { type: Array as PropType<Entry[]>, required: true },
            MaxEntries: { type: Number, required: false, default: 8 },
            LeftColumnTitle: { type: String, required: false, default: "Target" },
            MiddleColumnTitle: { type: String, required: false, default: "Amount" }
        },

        data: function() {
            return {
                ID: Math.floor(Math.random() * 100000) as number,
                chart: null as Chart | null
            }
        },

        mounted: function(): void {

        },

        computed: {
            total: function(): number {
                return this.data.reduce((acc, iter) => acc += iter.count, 0);
            },

            shown: function(): Entry[] {
                return this.data.slice(0, this.MaxEntries);
            },

            hiddenTotal: function(): number {
                return this.data.slice(this.MaxEntries).reduce((acc, iter) => acc += iter.count, 0);
            }

        },

        methods: {

        }

    });
    export default ChartEntryPie;
</script>
