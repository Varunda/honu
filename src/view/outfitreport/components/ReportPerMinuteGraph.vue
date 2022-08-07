<template>
    <collapsible header-text="Per minute graph">
        <canvas :id="canvasKillID" style="height: 300px; max-height: 300px;" class="mb-2 w-100"></canvas>

        <canvas :id="canvasScoreID" style="height: 300px; max-height: 300px;" class="mb-2 w-100"></canvas>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { ReportParameters } from "../Report";

    import { Experience, ExpEvent } from "api/ExpStatApi";
    import { KillEvent } from "api/KillStatApi";

    import Chart, { LegendItem } from "chart.js/auto/auto.esm";
    import * as moment from "moment";

    import Collapsible from "components/Collapsible.vue";

    type GraphEntry = {
        timestamp: Date;
        count: number;
    }

    export const ReportPerMinuteGraph = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        data: function() {
            return {
                killDeathChart: null as Chart | null,
                scoreChart: null as Chart | null,

                data: {
                    kills: [] as GraphEntry[],
                    deaths: [] as GraphEntry[],
                    exp: [] as GraphEntry[],
                    score: [] as GraphEntry[],
                    vehicleDestroy: [] as GraphEntry[]
                }
            }
        },

        mounted: function(): void {
            this.makeBoth();
        },

        methods: {
            makeBoth: function(): void {
                this.makeData();
                this.makeGraph();
            },

            makeData: function(): void {
                this.data = {
                    kills: [],
                    deaths: [],
                    exp: [],
                    score: [],
                    vehicleDestroy: []
                };

                const iterationCount: number = Math.ceil(Math.floor((this.parameters.periodEnd.getTime() - this.parameters.periodStart.getTime()) / 1000) / 60);

                console.log(`PerMinuteGraph> using ${iterationCount} iterations`);

                for (let i = 0; i < iterationCount; ++i) {
                    const iterTime: Date = moment(this.parameters.periodStart).add(i, "minutes").toDate();
                    const endIter: Date = moment(iterTime).add(1, "minutes").toDate();

                    this.data.kills.push({
                        timestamp: iterTime,
                        count: this.report.kills.filter(iter => {
                            return iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });

                    this.data.deaths.push({
                        timestamp: iterTime,
                        count: this.report.deaths.filter(iter => {
                            return iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });

                    this.data.score.push({
                        timestamp: iterTime,
                        count: this.report.experience.filter(iter => {
                            return iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).reduce((acc, iter) => acc += iter.amount, 0) / 60
                    });
                }

            },

            makeGraph: function(): void {
                this.killDeathChart?.destroy();
                this.killDeathChart = null;

                this.scoreChart?.destroy();
                this.scoreChart = null;

                const killCanvas: HTMLCanvasElement | null = document.getElementById(this.canvasKillID) as HTMLCanvasElement | null;
                if (killCanvas == null) {
                    return console.error(`PerMinuteGraph> Failed to find <canvas> element with ID ${this.canvasKillID}`);
                }

                const scoreCanvas: HTMLCanvasElement | null = document.getElementById(this.canvasScoreID) as HTMLCanvasElement | null;
                if (scoreCanvas == null) {
                    return console.error(`PerMinuteGraph> Failed to find <canvas> element with ID ${this.canvasScoreID}`);
                }

                const config = {
                    responsive: true,
                    maintainAspectRatio: true,
                    scales: {
                        x: {
                            ticks: {
                                color: "#ffffff",
                                display: false
                            },
                            grid: {
                                color: "#777777"
                            }
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                color: "#ffffff",
                            },
                            grid: {
                                color: "#777777"
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

                this.killDeathChart = new Chart(killCanvas.getContext("2d")!, {
                    type: "line",
                    data: {
                        labels: this.data.kills.map(iter => moment(iter.timestamp).format("YYYY-MM-DD hh:mm")),
                        datasets: [
                            {
                                data: this.data.kills.map(iter => iter.count),
                                borderColor: "#00ff00",
                                fill: true,
                                backgroundColor: "#007700",
                                label: "Kills"
                            },
                            {
                                data: this.data.deaths.map(iter => iter.count),
                                borderColor: "#ff0000",
                                fill: true,
                                backgroundColor: "#770000",
                                label: "Deaths"
                            }
                        ]
                    },
                    options: config
                });

                this.scoreChart = new Chart(scoreCanvas.getContext("2d")!, {
                    type: "line",
                    data: {
                        labels: this.data.score.map(iter => moment(iter.timestamp).format("YYYY-MM-DD hh:mm")),
                        datasets: [
                            {
                                data: this.data.score.map(iter => iter.count),
                                borderColor: "#ffff00",
                                fill: true,
                                backgroundColor: "#777700",
                                label: "Score per minute"
                            }
                        ]
                    },
                    options: config
                });
            }
        },

        computed: {
            canvasKillID: function(): string {
                return `chart-report-per-minute-kills`;
            },

            canvasScoreID: function(): string {
                return `chart-report-per-minute-score`;
            }
        },

        components: {
            Collapsible
        }
    });

    export default ReportPerMinuteGraph;
</script>