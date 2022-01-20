<template>
    <canvas :id="'chart-item-total-stats-' + ID" style="height: 240px; max-height: 300px;" class="mb-2"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Chart from "chart.js/auto/auto.esm";

    import { Bucket } from "api/ItemApi";

    import ColorUtils from "util/Color";

    export const ChartItemTotalStats = Vue.extend({
        props: {
            stats: { type: Array as PropType<Bucket[]>, required: true },
            name: { type: String, required: true }
        },

        data: function() {
            return {
                ID: Math.floor(Math.random() * 100000) as number,

                chart: null as Chart | null
            }
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

                const elem = document.getElementById(`chart-item-total-stats-${this.ID}`);
                if (elem == null) {
                    throw `Failed to get canvas #chart-item-percentile-stats-${this.ID}`;
                }

                const ctx = (elem as any).getContext("2d");

                const total: number = this.stats.reduce((acc, iter) => acc += iter.count, 0);

                console.log(`Graphing over ${this.stats.length} buckets`);

                let acc: number = 0;

                this.chart = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: this.stats.map((iter: Bucket) => `${iter.start.toFixed(4)}`),
                        datasets: [{
                            data: this.stats.map((iter: Bucket) => (acc += iter.count) / total * 100),
                            fill: true,
                            backgroundColor: ColorUtils.randomColors(Math.random(), 1)[0],
                            label: this.name,
                            //spanGaps: true
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                labels: {
                                    color: "#fff",
                                }
                            },
                        },
                        elements: {
                            point: {
                                radius: 4
                            }
                        },
                        interaction: {
                            intersect: false,
                            mode: "nearest"
                        },
                        scales: {
                            x: {
                                ticks: {
                                    color: "#fff",
                                },
                                //beginAtZero: true,
                                //min: 0,
                                //max: this.stats[this.stats.length - 1].start + this.stats[0].width,
                            },
                            y: {
                                ticks: {
                                    color: "#fff",
                                    callback: function(value) {
                                        return `${value}%`;
                                    },
                                },
                                beginAtZero: true
                            }
                        }
                    }
                });

            }
        },

        computed: {

        }
    });
    export default ChartItemTotalStats;
</script>
