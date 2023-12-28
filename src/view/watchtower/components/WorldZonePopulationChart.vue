<template>
    <div>
        <canvas :id="'zone-pop-' + WorldId + '-' + ZoneId" class="w-100 px-2" style="height: 200px;"></canvas>
    </div>
</template>


<script lang="ts">
    import Vue, { PropType } from "vue";
    import Chart from "chart.js/auto/auto.esm";

    import { WorldZonePopulation } from "../WorldData";
    import ColorUtils from "util/Color";

    export const WorldZonePopulationChart = Vue.extend({
        props: {
            WorldId: { type: Number, required: true },
            ZoneId: { type: Number, required: true },
            data: { type: Array as PropType<WorldZonePopulation[]>, required: false },
            ShowTeams: { type: Boolean, required: false, default: true }
        },

        data: function() {
            return {
                chart: null as Chart | null,
            };
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeChart();
            });
        },

        methods: {
            makeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const elemID: string = `zone-pop-${this.WorldId}-${this.ZoneId}`;

                const elem = document.getElementById(elemID);
                if (elem == null) {
                    throw `Failed to get canvas #${elemID}`;
                }

                try {
                    const ctx = (elem as any).getContext("2d");
                    this.chart = new Chart(ctx, {
                        type: "line",
                        data: {
                            labels: [],
                            datasets: [
                                {
                                    label: "VS",
                                    borderColor: ColorUtils.VS,
                                    backgroundColor: ColorUtils.VS,
                                    data: []
                                },
                                {
                                    label: "NC",
                                    borderColor: ColorUtils.NC,
                                    backgroundColor: ColorUtils.NC,
                                    data: []
                                },
                                {
                                    label: "TR",
                                    borderColor: ColorUtils.TR,
                                    backgroundColor: ColorUtils.TR,
                                    data: []
                                },
                                {
                                    label: this.ShowTeams == true ? "Unknown" : "NS",
                                    borderColor: ColorUtils.NS,
                                    backgroundColor: ColorUtils.NS,
                                    data: []
                                },
                            ]
                        },
                        options: {
                            plugins: {
                                tooltip: {
                                    mode: "index",
                                    intersect: false
                                }
                            },
                            elements: {
                                point: {
                                    radius: 0
                                }
                            },
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                x: {
                                    type: "time",
                                    /*
                                    ticks: {
                                        maxRotation: 90,
                                        minRotation: 90,
                                        callback: function(label, index, ticks) {
                                            return TimeUtils.format(label, "hh:mm:ss A");
                                        }
                                    }
                                    */
                                }
                            }
                        }
                    });
                } catch (err: any) {
                    console.error(`Failed to created chart: ${err}`);
                }

                this.updateChart();
            },

            updateChart: function(): void {
                if (this.chart == null) {
                    console.log(`no chart!`);
                    return;
                }

                this.chart.data.labels = this.data.map(iter => iter.timestamp);
                if (this.ShowTeams) {
                    this.chart.data.datasets[0].data = this.data.map(iter => iter.teamVs);
                    this.chart.data.datasets[1].data = this.data.map(iter => iter.teamNc);
                    this.chart.data.datasets[2].data = this.data.map(iter => iter.teamTr);
                    this.chart.data.datasets[3].data = this.data.map(iter => iter.teamUnknown);
                } else {
                    this.chart.data.datasets[0].data = this.data.map(iter => iter.factionVs);
                    this.chart.data.datasets[1].data = this.data.map(iter => iter.factionNc);
                    this.chart.data.datasets[2].data = this.data.map(iter => iter.factionTr);
                    this.chart.data.datasets[3].data = this.data.map(iter => iter.factionNs);
                }

                this.chart.update();
            }
        },

        watch: {
            data: function(): void {
                this.updateChart();
            }
        }
    });
    export default WorldZonePopulationChart;

</script>
