<template>
    <div>
        <div v-if="pop.state == 'idle'"></div>

        <div v-else-if="pop.state == 'loading'" style="max-height: 40vh" class="text-center my-3">
            <busy class="honu-busy honu-busy-lg"></busy>
        </div>

        <canvas v-else-if="pop.state == 'loaded'" id="alert-population-chart" class="w-100" style="max-height: 40vh"></canvas>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Loading, Loadable } from "Loading";

    import { PsAlert } from "api/AlertApi";
    import { AlertPopulation, AlertPopulationApi } from "api/AlertPopulationApi";

    import Chart from "chart.js/auto/auto.esm";
    import ColorUtils from "util/Color";
    import TimeUtils from "util/Time";
    import * as moment from "moment";

    import Busy from "components/Busy.vue";

    export const AlertPopulationGraph = Vue.extend({
        props: {
            alert: { type: Object as PropType<PsAlert>, required: true },
            ShowTotal: { type: Boolean, required: true }
        },

        data: function() {
            return {
                chart: null as Chart | null,
                pop: Loadable.idle() as Loading<AlertPopulation[]>
            }
        },

        mounted: function()  {
            this.$nextTick(() => {
                this.loadData().then(() => {
                    this.makeGraph();
                });
            })
        },

        methods: {
            loadData: async function(): Promise<void> {
                this.pop = Loadable.loading();
                this.pop = await AlertPopulationApi.getByAlertID(this.alert.id);

                if (this.pop.state == "loaded") {
                    this.pop = Loadable.loaded(this.pop.data.sort((a, b) => {
                        return a.timestamp.getTime() - b.timestamp.getTime();
                    }));
                }
            },

            makeGraph: function(): void {
                if (this.pop.state != "loaded") {
                    return console.warn(`AlertPopulationGraph> Cannot make graph, data is not loaded (currently ${this.pop.state})`);
                }

                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const canvas = document.getElementById(`alert-population-chart`);
                if (canvas == null) {
                    return console.error(`Failed to find #alert-population-chart`);
                }

                const datasets = [
                    {
                        label: "Total",
                        data: this.pop.data.map(iter => iter.countVS + iter.countNC + iter.countTR + iter.countUnknown),
                        borderColor: "#fff"
                    },
                    {
                        label: "VS",
                        data: this.pop.data.map(iter => iter.countVS),
                        borderColor: ColorUtils.VS
                    },
                    {
                        label: "NC",
                        data: this.pop.data.map(iter => iter.countNC),
                        borderColor: ColorUtils.NC
                    },
                    {
                        label: "TR",
                        data: this.pop.data.map(iter => iter.countTR),
                        borderColor: ColorUtils.TR
                    },
                    {
                        label: "Unknown",
                        data: this.pop.data.map(iter => iter.countUnknown),
                        borderColor: ColorUtils.NS
                    },
                ];

                if (this.ShowTotal == false) { 
                    datasets.shift(); // first element is the total
                }

                this.chart = new Chart((canvas as any).getContext("2d"), {
                    type: "line",
                    data: {
                        labels: this.pop.data.map(iter => TimeUtils.duration((iter.timestamp.getTime() - this.alert.timestamp.getTime()) / 1000)),
                        datasets: datasets
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                    }
                });
            }
        },

        watch: {
            ShowTotal: function() {
                this.makeGraph();
            }
        },

        computed: {

        },

        components: {
            Busy
        }
    });

    export default AlertPopulationGraph;
</script>