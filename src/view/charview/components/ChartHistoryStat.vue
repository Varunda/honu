<template>
    <canvas :id="'chart-history-stat-' + ID" class="w-100 d-inline-block" style="max-height: 240px;"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Chart from "chart.js/auto/auto.esm";

    import ColorUtils from "util/Color";
    import * as moment from "moment";

    export const ChartHistoryStat = Vue.extend({
        props: {
            data: { type: Array as PropType<number[]>, required: true },
            period: { type: String, required: true },
            title: { type: String, required: true },
            timestamp: { type: Date, required: true },
            IsTime: { type: Boolean, required: false, default: false },
        },

        data: function() {
            return {
                ID: Math.floor(Math.random() * 100000),
                chart: null as Chart | null,
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

                const canvas = document.getElementById(`chart-history-stat-${this.ID}`);
                if (canvas == null) {
                    return console.error(`Failed to find #chart-history-stat-${this.ID}`);
                }

                const max: number = Math.max(...this.data);

                let format = "yyyy-MM-DD";
                if (this.period == "months") {
                    format = "yyyy-MM";
                }

                const color: string = ColorUtils.randomColorSingle();

                this.chart = new Chart((canvas as any).getContext("2d"), {
                    type: "line",
                    data: {
                        labels: this.data.map((_, index) => moment(this.timestamp).add(-index, (this.period as any)).format(format)),
                        datasets: [{
                            data: this.data,
                            label: this.title,
                            backgroundColor: color,
                            borderColor: color,
                            fill: true
                        }]
                    },
                    options: {
                        animation: {
                            onComplete: function() {
                                const chart = this.ctx;
                                chart.textAlign = "center";
                                chart.textBaseline = "bottom";
                                chart.font = `16px ${(Chart.defaults.font as any).family}`;

                                this.data.datasets.forEach((dataset, i) => {
                                    const meta = this.getDatasetMeta(i);

                                    meta.data.forEach((bar, index) => {
                                        const data = dataset.data[index];

                                        let display: string = data?.toString() ?? ``;
                                        if (typeof (data) == "number") {
                                            display = data.toFixed(2);
                                        }

                                        chart.fillStyle = "#fff";
                                        chart.fillText(display, bar.x, bar.y - 2);
                                    });
                                });
                            }
                        },
                        scales: {
                            x: {
                                offset: true,
                                reverse: true,
                                ticks: {
                                    color: "#fff",
                                },
                                grid: {
                                    color: "#999",
                                    display: false,
                                }
                            },
                            y: {
                                beginAtZero: true,
                                max: Math.max(1, Math.round(max * 1.5)), // There's a ChartJS bug where if the max is 0 and beginAtZero is set, it fails. Ensure that it's above 0
                                ticks: {
                                    count: 5
                                },
                                grid: {
                                    color: "#999"
                                }
                            },
                        },
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                labels: {
                                    color: "#fff",
                                }
                            }
                        }
                    },
                    //plugins: [ ChartDataLabels ]
                });
            }
        },

        watch: {
            data: function(): void {
                this.$nextTick(() => {
                    this.makeChart();
                });
            }
        }

    });
    export default ChartHistoryStat;
</script>