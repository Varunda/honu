<template>
    <collapsible header-text="Experience breakdown">
        <div class="row">
            <div class="col-12 col-xl-6">
                <div class="row">
                    <div class="col-12 col-lg-6">
                        <chart-block-pie-chart :data="count" left-title="Experience type" right-title="# detected" :label-value="false" :show-percent="true"></chart-block-pie-chart>
                    </div>
                    <div class="col-12 col-lg-6">
                        <chart-block-list :data="count" left-title="Experience type" right-title="# detected"></chart-block-list>
                    </div>
                </div>
            </div>

            <div class="col-12 col-xl-6">
                <div class="row">
                    <div class="col-12 col-lg-6">
                        <chart-block-pie-chart :data="amount" left-title="Experience type" right-title="Experience earned" :label-value="false" :show-percent="true"></chart-block-pie-chart>
                    </div>
                    <div class="col-12 col-lg-6">
                        <chart-block-list :data="amount" left-title="Experience type" right-title="Experience earned"></chart-block-list>
                    </div>
                </div>
            </div>
        </div>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { ReportParameters } from "../Report";

    import Collapsible from "components/Collapsible.vue";

    import ChartBlockList from "./charts/ChartBlockList.vue";
    import ChartBlockPieChart from "./charts/ChartBlockPieChart.vue";
    import { Block }  from "./charts/common";

    import ExpAlias from "util/ExpAlias";

    interface Entry {
        display: string;
        count: number;
    }

    export const ReportExpBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        data: function() {
            return {
                typeCount: [] as Entry[],
                typeAmount: [] as Entry[],

                count: new Block() as Block,
                amount: new Block() as Block,
            }
        },

        mounted: function(): void {
            this.makeTypeBreakdown();
        },
        
        methods: {
            makeTypeBreakdown: function(): void {
                this.typeCount = [];
                this.typeAmount = [];

                const count: Map<number, number> = new Map();
                const amount: Map<number, number> = new Map();

                for (const ev of this.report.experience) {
                    const aliasID: number = ExpAlias.get(ev.experienceID);
                    count.set(aliasID, (count.get(aliasID) || 0) + 1);
                    amount.set(aliasID, (amount.get(aliasID) || 0) + ev.amount);
                }

                for (const kvp of count) {
                    this.count.entries.push({
                        name: this.report.experienceTypes.get(kvp[0])?.name ?? `unknown type ${kvp[0]}`,
                        count: kvp[1],
                    });
                }
                this.count.entries.sort((a, b) => b.count - a.count);
                this.count.total = this.report.experience.length;

                for (const kvp of amount) {
                    this.amount.entries.push({
                        name: this.report.experienceTypes.get(kvp[0])?.name ?? `unknown type ${kvp[0]}`,
                        count: kvp[1],
                    });
                }
                this.amount.entries.sort((a, b) => b.count - a.count);
                this.amount.total = this.report.experience.reduce((acc, iter) => acc += iter.amount, 0);
            }
        },

        components: {
            ChartBlockList, ChartBlockPieChart, Collapsible
        }
    });

    export default ReportExpBreakdown;
</script>