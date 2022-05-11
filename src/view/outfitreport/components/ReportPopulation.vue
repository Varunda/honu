<template>
    <collapsible header-text="Players online">
        <canvas :id="canvasID" style="height: 300px; max-height: 300px;" class="mb-2 w-100"></canvas>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Report from "../Report";

    import Chart, { LegendItem } from "chart.js/auto/auto.esm";
    import * as moment from "moment";

    import Collapsible from "components/Collapsible.vue";

    type PopulationEntry = {
        timestamp: Date;
        count: number;
    }

    export const ReportPopulation = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                chart: null as Chart | null,

                data: [] as PopulationEntry[]
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeBoth();
            });
        },

        methods: {
            makeBoth: function(): void {
                this.makeData();
                this.makeGraph();
            },

            makeData: function(): void {
                this.data = [];

                const iterationCount: number = Math.ceil(Math.floor((this.report.periodEnd.getTime() - this.report.periodStart.getTime()) / 1000) / 60);

                console.log(`ReportPopulation> using ${iterationCount} iterations`);

                for (let i = 0; i < iterationCount; ++i) {
                    const iterTime: Date = moment(this.report.periodStart).add(i, "minutes").toDate();
                    const endIter: Date = moment(iterTime).add(1, "minutes").toDate();

                    const entry: PopulationEntry = {
                        timestamp: iterTime,
                        count: this.report.sessions.filter(iter => {
                            return iter.start <= endIter
                                && (iter.end == null || iter.end >= iterTime);
                        }).length
                    };

                    //console.log(`ReportPopulation> PERIOD: ${moment(iterTime).toISOString()} TO ${moment(endIter).toISOString()} => ${entry.count}`);

                    this.data.push(entry);
                }
            },

            makeGraph: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const canvas: HTMLCanvasElement | null = document.getElementById(this.canvasID) as HTMLCanvasElement | null;
                if (canvas == null) {
                    console.error(`ReportPopulation> Failed to find <canvas> element with ID ${this.canvasID}`);
                    return;
                }

                this.chart = new Chart(canvas.getContext("2d")!, {
                    type: "line",
                    data: {
                        labels: this.data.map(iter => moment(iter.timestamp).format("YYYY-MM-DD hh:mm")),
                        datasets: [{
                            data: this.data.map(iter => iter.count),
                            borderColor: "#ffffff",
                            label: "Tracker characters online",
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        scales: {
                            x: {
                                ticks: {
                                    color: "#ffffff",
                                    display: false
                                },
                            },
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    color: "#ffffff",
                                }
                            }
                        },
                        plugins: {
                            legend: {
                                labels: {
                                    color: "#ffffff"
                                }
                            }
                        }
                    }
                });
            },
        },

        computed: {
            canvasID: function(): string {
                return `chart-report-population`;
            }
        },

        components: {
            Collapsible
        }
    });
    export default ReportPopulation;
</script>