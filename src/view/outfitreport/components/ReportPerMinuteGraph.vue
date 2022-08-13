<template>
    <collapsible header-text="Per minute graph">
        <canvas :id="canvasKillID" style="height: 20vh; max-height: 20vh;" class="mb-2 w-100"></canvas>

        <canvas :id="canvasScoreID" style="height: 20vh; max-height: 20vh;" class="mb-2 w-100"></canvas>

        <canvas :id="canvasMedicID" style="height: 20vh; max-height: 20vh;" class="mb-2 w-100"></canvas>

        <canvas :id="canvasEngiID" style="height: 20vh; max-height: 20vh;" class="mb-2 w-100"></canvas>

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
                medicChart: null as Chart | null,
                engiChart: null as Chart | null,

                data: {
                    kills: [] as GraphEntry[],
                    deaths: [] as GraphEntry[],
                    score: [] as GraphEntry[],
                    vehicleDestroy: [] as GraphEntry[],
                    revive: [] as GraphEntry[],
                    heal: [] as GraphEntry[],
                    shieldRepair: [] as GraphEntry[],
                    resupply: [] as GraphEntry[],
                    maxRepair: [] as GraphEntry[]
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

            getSlice(arr: { timestamp: Date }[], startTime: Date, endTime: Date): GraphEntry {
                const datum: GraphEntry = {
                    timestamp: startTime,
                    count: 0
                };
                let i = 0;
                for (i = 0; i < arr.length; ++i) {
                    let k = arr[i];
                    if (k.timestamp > endTime) {
                        break;
                    }
                    ++datum.count;
                }

                return datum;
            },

            getSliceAndAdvance(arr: { timestamp: Date }[], startTime: Date, endTime: Date): GraphEntry {
                const datum: GraphEntry = {
                    timestamp: startTime,
                    count: 0
                };
                let i = 0;
                for (i = 0; i < arr.length; ++i) {
                    let k = arr[i];
                    if (k.timestamp > endTime) {
                        break;
                    }
                    ++datum.count;
                }
                arr = arr.slice(0, i);

                return datum;
            },

            makeData: function(): void {
                this.data = {
                    kills: [],
                    deaths: [],
                    vehicleDestroy: [],
                    score: [],
                    revive: [],
                    heal: [],
                    shieldRepair: [],
                    resupply: [],
                    maxRepair: []
                };

                const iterationCount: number = Math.ceil(Math.floor((this.parameters.periodEnd.getTime() - this.parameters.periodStart.getTime()) / 1000) / 60);

                console.log(`PerMinuteGraph> using ${iterationCount} iterations`);

                let kills: KillEvent[] = [...this.report.kills];
                let deaths: KillEvent[] = [...this.report.deaths];
                let exp: ExpEvent[] = [...this.report.experience];

                // This code is bad, it iterates thru all kill/death/exp events every single block, which is really bad
                for (let i = 0; i < iterationCount; ++i) {
                    const iterTime: Date = moment(this.parameters.periodStart).add(i, "minutes").toDate();
                    const endIter: Date = moment(iterTime).add(1, "minutes").toDate();

                    /*
                    const datum: GraphEntry = {
                        timestamp: iterTime,
                        count: 0
                    };
                    for (let i = 0; i < kills.length; ++i) {
                        let k: KillEvent = kills[i];
                        if (k.timestamp > endIter) {
                            break;
                        }
                        ++datum.count;
                    }
                    kills = kills.slice(0, i);
                    this.data.kills.push(datum);

                    this.data.kills.push(this.getSliceAndAdvance(kills, iterTime, endIter));

                    this.data.deaths.push(this.getSliceAndAdvance(deaths, iterTime, endIter));
                    */

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
                        }).reduce((acc, iter) => acc += iter.amount, 0)
                    });

                    this.data.revive.push({
                        timestamp: iterTime,
                        count: this.report.experience.filter(iter => {
                            return Experience.isRevive(iter.experienceID)
                                && iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });

                    this.data.shieldRepair.push({
                        timestamp: iterTime,
                        count: this.report.experience.filter(iter => {
                            return Experience.isShieldRepair(iter.experienceID)
                                && iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });

                    this.data.heal.push({
                        timestamp: iterTime,
                        count: this.report.experience.filter(iter => {
                            return Experience.isHeal(iter.experienceID)
                                && iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });

                    this.data.resupply.push({
                        timestamp: iterTime,
                        count: this.report.experience.filter(iter => {
                            return Experience.isResupply(iter.experienceID)
                                && iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });

                    this.data.maxRepair.push({
                        timestamp: iterTime,
                        count: this.report.experience.filter(iter => {
                            return Experience.isMaxRepair(iter.experienceID)
                                && iter.timestamp >= iterTime && iter.timestamp <= endIter;
                        }).length
                    });
                }
            },

            makeGraph: function(): void {
                this.killDeathChart?.destroy();
                this.killDeathChart = null;

                this.scoreChart?.destroy();
                this.scoreChart = null;

                this.medicChart?.destroy();
                this.medicChart = null;

                this.engiChart?.destroy();
                this.engiChart = null;

                const killCanvas: HTMLCanvasElement | null = document.getElementById(this.canvasKillID) as HTMLCanvasElement | null;
                if (killCanvas == null) {
                    return console.error(`PerMinuteGraph> Failed to find <canvas> element with ID ${this.canvasKillID}`);
                }

                const scoreCanvas: HTMLCanvasElement | null = document.getElementById(this.canvasScoreID) as HTMLCanvasElement | null;
                if (scoreCanvas == null) {
                    return console.error(`PerMinuteGraph> Failed to find <canvas> element with ID ${this.canvasScoreID}`);
                }

                const medicCanvas: HTMLCanvasElement | null = document.getElementById(this.canvasMedicID) as HTMLCanvasElement | null;
                if (medicCanvas == null) {
                    return console.error(`PerMinuteGraph> Failed to find <canvas> element with ID ${this.canvasMedicID}`);
                }

                const engiCanvas: HTMLCanvasElement | null = document.getElementById(this.canvasEngiID) as HTMLCanvasElement | null;
                if (engiCanvas == null) {
                    return console.error(`PerMinuteGraph> Failed to find <canvas> element with ID ${this.canvasEngiID}`);
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
                                precision: 0
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
                        },
                        tooltip: {
                            mode: "nearest" as "nearest", // as "x", // tell typescript this isn't a string
                            intersect: false,
                            axis: "x" as "x"
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
                                backgroundColor: "#00770088",
                                label: "Kills"
                            },
                            {
                                data: this.data.deaths.map(iter => iter.count),
                                borderColor: "#ff0000",
                                fill: true,
                                backgroundColor: "#770000FF",
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
                                backgroundColor: "#77770088",
                                label: "Score"
                            }
                        ]
                    },
                    options: config
                });

                this.medicChart = new Chart(medicCanvas.getContext("2d")!, {
                    type: "line",
                    data: {
                        labels: this.data.score.map(iter => moment(iter.timestamp).format("YYYY-MM-DD hh:mm")),
                        datasets: [
                            {
                                data: this.data.revive.map(iter => iter.count),
                                borderColor: "#ff00ff",
                                fill: true,
                                backgroundColor: "#77007788",
                                label: "Revives"
                            },
                            {
                                data: this.data.heal.map(iter => iter.count),
                                borderColor: "#00ffff",
                                fill: true,
                                backgroundColor: "#00777788",
                                label: "Heals"
                            },
                            {
                                data: this.data.shieldRepair.map(iter => iter.count),
                                borderColor: "#0000ff",
                                fill: true,
                                backgroundColor: "#00007788",
                                label: "Shield repair"
                            }
                        ]
                    },
                    options: config
                });

                this.engiChart = new Chart(engiCanvas.getContext("2d")!, {
                    type: "line",
                    data: {
                        labels: this.data.score.map(iter => moment(iter.timestamp).format("YYYY-MM-DD hh:mm")),
                        datasets: [
                            {
                                data: this.data.resupply.map(iter => iter.count),
                                borderColor: "#ff8888",
                                fill: true,
                                backgroundColor: "#77333388",
                                label: "Resupplies"
                            },
                            {
                                data: this.data.maxRepair.map(iter => iter.count),
                                borderColor: "#8888ff",
                                fill: true,
                                backgroundColor: "#33337788",
                                label: "MAX repairs"
                            }
                        ]
                    },
                    options: config
                });
            }
        },

        computed: {
            canvasKillID: function(): string { return `chart-report-per-minute-kills`; },

            canvasMedicID: function(): string { return `chart-report-per-minute-medic`; },

            canvasScoreID: function(): string { return `chart-report-per-minute-score`; },

            canvasEngiID: function(): string { return `chart-report-per-minute-engi`; },
        },

        components: {
            Collapsible
        }
    });

    export default ReportPerMinuteGraph;
</script>