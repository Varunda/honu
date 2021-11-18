<template>
    <canvas :id="'chart-history-stat-' + ID" class="w-100 d-inline-block" style="max-height: 300px;"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Chart from "chart.js/auto/auto.esm";
    import * as moment from "moment";

    export const ChartHistoryStat = Vue.extend({
        props: {
            data: { type: Array as PropType<number[]>, required: true },
            period: { type: String, required: true },
            title: { type: String, required: true },
            timestamp: { type: Date, required: true }
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

                const ctx = (canvas as any).getContext("2d");

                this.chart = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: this.data.map((_, index) => moment(this.timestamp).add(-index, (this.period as any)).format("yyyy-MM-DD")),
                        datasets: [{
                            data: this.data,
                            label: this.title,
                            backgroundColor: "#fff",
                            borderColor: "#fff"
                        }]
                    },
                    options: {
                        scales: {
                            x: {
                                reverse: true,
                                ticks: {
                                    color: "#fff",
                                }
                            }
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
                    }
                });

            }
        },

    });
    export default ChartHistoryStat;
</script>