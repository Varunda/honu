<template>
    <div>
        <div>
            <select v-model.number="settings.intervalSize" class="form-control">
                <option :value="60">1 hour</option>
                <option :value="30">30 minutes</option>
                <option :value="15">15 minutes</option>
                <option :value="10">10 minutes</option>
                <option :value="5">5 minutes</option>
                <option :value="1">1 minute (LAGGY)</option>
            </select>

            <toggle-button v-model="settings.showServers">Show servers</toggle-button>

            <toggle-button v-model="settings.showSeconds">Show seconds</toggle-button>

            <toggle-button v-model="settings.includeJaeger">Include Jaeger</toggle-button>
        </div>

        <canvas :id="'reconnect-graph-' + ID" style="height: 240px; max-height: 40vh;" class="mb-2"></canvas>
    </div>

</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { RealtimeReconnectEntry } from "api/RealtimeReconnectApi";

    import Chart from "chart.js/auto/auto.esm";

    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";

    import WorldUtils from "util/World";
    import TimeUtils from "util/Time";

    import "MomentFilter";
    import "filters/TimeAgoFilter";
    import "filters/WorldNameFilter";
    import "filters/LocaleFilter";

    type ReconnectBucket = {
        timestamp: Date;
        total: number;
        connery: number;
        miller: number;
        cobalt: number;
        emerald: number;
        jaeger: number;
        soltech: number;
    };

    function newDataset(label: string, color: string, borderWidth: number = 2) {
        return {
            label: label,
            backgroundColor: color,
            borderColor: color,
            borderWidth: borderWidth,
            data: [],
            barPercentage: 1,
            categoryPercentage: 1,
            barThickness: "flex"
        };
    }

    export const ReconnectGraph = Vue.extend({
        props: {
            reconnects: { type: Array as PropType<RealtimeReconnectEntry[]>, required: true }
        },

        data: function() {
            return {
                ID: Math.floor(Math.random() * 100000) as number,

                chart: null as Chart | null,

                data: [] as ReconnectBucket[],

                settings: {
                    intervalSize: 15 as number,
                    showServers: true as boolean,
                    showSeconds: false as boolean,
                    includeJaeger: true as boolean
                }
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeData();
                this.makeChart();
            });
        },

        methods: {
            makeData: function(): void {
                const now: Date = new Date();
                const start: Date = new Date(now.getTime() - (1000 * 60 * 60 * 24)); // 24 hours ago
                const diff: number = now.getTime() - start.getTime();
                const intervalSize: number = 1000 * 60 * this.settings.intervalSize;

                this.data = [];

                for (let i = 0; i <= diff; i += intervalSize) {
                    const intervalStart: number = start.getTime() + i;
                    const intervalEnd: number = intervalStart + intervalSize;

                    const interval: RealtimeReconnectEntry[] = this.reconnects.filter((iter: RealtimeReconnectEntry) => {
                        if (this.settings.includeJaeger == false && iter.worldID == WorldUtils.Jaeger) {
                            return false;
                        }
                        const timestamp: number = iter.timestamp.getTime() + intervalSize;
                        return timestamp >= intervalStart && timestamp <= intervalEnd;
                    });

                    //console.log(`${i} ${new Date(intervalStart).toISOString()} - ${new Date(intervalEnd).toISOString()} = ${interval.length}`);

                    if (this.settings.showSeconds == true) {
                        this.data.push({
                            timestamp: new Date(intervalStart),
                            total: interval.reduce((acc, i) => acc += i.duration, 0),
                            connery: interval.filter(iter => iter.worldID == WorldUtils.Connery).reduce((acc, i) => acc += i.duration, 0),
                            cobalt: interval.filter(iter => iter.worldID == WorldUtils.Cobalt).reduce((acc, i) => acc += i.duration, 0),
                            emerald: interval.filter(iter => iter.worldID == WorldUtils.Emerald).reduce((acc, i) => acc += i.duration, 0),
                            jaeger: this.settings.includeJaeger == true ? interval.filter(iter => iter.worldID == WorldUtils.Jaeger).reduce((acc, i) => acc += i.duration, 0) : 0,
                            miller: interval.filter(iter => iter.worldID == WorldUtils.Miller).reduce((acc, i) => acc += i.duration, 0),
                            soltech: interval.filter(iter => iter.worldID == WorldUtils.SolTech).reduce((acc, i) => acc += i.duration, 0)
                        });
                    } else {
                        this.data.push({
                            timestamp: new Date(intervalStart),
                            total: interval.length,
                            connery: interval.filter(iter => iter.worldID == WorldUtils.Connery).length,
                            cobalt: interval.filter(iter => iter.worldID == WorldUtils.Cobalt).length,
                            emerald: interval.filter(iter => iter.worldID == WorldUtils.Emerald).length,
                            jaeger: this.settings.includeJaeger == true ? interval.filter(iter => iter.worldID == WorldUtils.Jaeger).length : 0,
                            miller: interval.filter(iter => iter.worldID == WorldUtils.Miller).length,
                            soltech: interval.filter(iter => iter.worldID == WorldUtils.SolTech).length,
                        });
                    }

                }
            },

            makeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const elem = document.getElementById(`reconnect-graph-${this.ID}`);
                if (elem == null) {
                    throw `Failed to get canvas #reconnect-graph-${this.ID}`;
                }

                const ctx = (elem as any).getContext("2d");

                const now: number = new Date().getTime();

                let datasets = [];
                if (this.settings.showServers == true) {
                    datasets.push(
                        newDataset("Connery", "#ff00ff"),
                        newDataset("Cobalt", "#0000ff"),
                        newDataset("Emerald", "#00ff00"),
                        newDataset("Miller", "#882222"),
                        newDataset("Jaeger", "#770077"),
                        newDataset("SolTech", "#007777")
                    );
                }

                datasets.push({
                    label: "Total",
                    backgroundColor: "#fff",
                    borderColor: "#fff",
                    data: [],
                    borderWidth: 8,
                    type: "line",
                    fill: true,
                    tension: 0.4
                });

                this.chart = new Chart(ctx, {
                    type: "bar",
                    data: {
                        labels: this.data.map((iter: ReconnectBucket) => `${TimeUtils.duration((now - iter.timestamp.getTime()) / 1000)} ago`),
                        datasets: datasets as any // ugh
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
                                radius: 0
                            }
                        },
                        interaction: {
                            intersect: false,
                            mode: "index"
                        },
                        scales: {
                            x: {
                                stacked: true,
                                ticks: {
                                    color: "#fff",
                                },
                            },
                            y: {
                                stacked: true,
                                ticks: {
                                    color: "#fff",
                                    callback: function(value) {
                                        return value;
                                    },
                                },
                                beginAtZero: true
                            }
                        }
                    }
                });
            },

            updateData: function(): void {
                if (this.chart == null) {
                    return;
                }

                this.chart.data.datasets.forEach((dataset) => {
                    if (dataset.label == "Total") {
                        dataset.data = this.data.map(iter => iter.total)
                    } else if (dataset.label == "Connery") {
                        dataset.data = this.data.map(iter => iter.connery)
                    } else if (dataset.label == "Cobalt") {
                        dataset.data = this.data.map(iter => iter.cobalt)
                    } else if (dataset.label == "Emerald") {
                        dataset.data = this.data.map(iter => iter.emerald)
                    } else if (dataset.label == "Miller") {
                        dataset.data = this.data.map(iter => iter.miller)
                    } else if (dataset.label == "Jaeger") {
                        dataset.data = this.data.map(iter => iter.jaeger)
                    } else if (dataset.label == "SolTech") {
                        dataset.data = this.data.map(iter => iter.soltech)
                    } else {
                        console.warn(`unchecked label '${dataset.label}'`);
                    }
                });

                this.chart.update();
            }
        },

        watch: {
            reconnects: function(): void {
                this.makeData();
                this.updateData();
            },

            settings: {
                deep: true,
                handler: function(): void {
                    this.makeData();
                    this.makeChart();
                }
            }
        },

        components: {
            InfoHover,
            ToggleButton
        }
    });
    export default ReconnectGraph;
</script>