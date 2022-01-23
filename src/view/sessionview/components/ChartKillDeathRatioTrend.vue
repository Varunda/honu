<template>
    <canvas id="chart-timestamp-kill-death" style="max-height: 300px;" class="w-auto mb-2"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import * as moment from "moment";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import { ExpandedKillEvent, KillEvent, KillStatApi } from "api/KillStatApi";
    import { Session, SessionApi } from "api/SessionApi";

    import TimeUtils from "util/Time";

    type KDTrendEntry = {
        timestamp: Date;
        type: "kill" | "death";
    };

    type Interval = {
        timestamp: Date;
        kills: number;
        deaths: number;
    };

    export const ChartKillDeathRatioTrend = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            kills: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            deaths: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
        },

        data: function() {
            return {
                entries: [] as KDTrendEntry[],
                intervals: [] as Interval[],
                trend: [] as Interval[],
                period: 30 as number,
                chart: null as Chart | null,
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeEntries();
                this.makeTrend();
                this.makeIntervals();
                this.makeChart();
            });
        },

        methods: {
            makeEntries: function(): void {
                const entries: KDTrendEntry[] = this.kills.map(iter => {
                    return {
                        timestamp: iter.event.timestamp,
                        type: "kill"
                    };
                });

                const d: KDTrendEntry[] = this.deaths.map(iter => {
                    return {
                        timestamp: iter.event.timestamp,
                        type: "death"
                    };
                });

                entries.push(...d);
                entries.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                this.entries = entries;
            },

            makeIntervals: function(): void {
                this.intervals = [];

                if (this.entries.length < 2) {
                    return;
                }

                let previous: Interval[] = [];

                const start: number = this.session.start.getTime();
                const end: number = (this.session.end ?? new Date()).getTime();

                for (let i = start; i <= end; i += 1000 * this.period) {
                    const slice: KDTrendEntry[] = this.entries.filter(iter => iter.timestamp.getTime() >= i && iter.timestamp.getTime() < (i + this.period * 1000));

                    if (previous.length > 8) {
                        previous.shift();
                    }

                    let interval: Interval = {
                        timestamp: new Date(i),
                        kills: slice.filter(iter => iter.type == "kill").length,
                        deaths: slice.filter(iter => iter.type == "death").length
                    };

                    const sumKills: number = previous.reduce((acc, iter) => acc += iter.kills, 0);
                    const sumDeaths: number = previous.reduce((acc, iter) => acc += iter.deaths, 0);

                    previous.push({ ...interval });

                    interval.kills += sumKills;
                    interval.deaths += sumDeaths;

                    this.intervals.push(interval);
                }
            },

            makeTrend: function(): void {
                this.trend = [];

                if (this.entries.length < 2) {
                    return;
                }

                let totalKills: number = 0;
                let totalDeaths: number = 0;

                this.trend.push({
                    timestamp: this.session.start,
                    kills: 0,
                    deaths: 0
                });

                for (const entry of this.entries) {
                    if (entry.type == "kill") {
                        ++totalKills;
                    } else if (entry.type == "death") {
                        ++totalDeaths;
                    }

                    this.trend.push({
                        timestamp: entry.timestamp,
                        kills: totalKills,
                        deaths: totalDeaths
                    });
                }

                this.trend.push({
                    timestamp: (this.session.end ?? new Date()),
                    kills: totalKills,
                    deaths: totalDeaths
                });
            },

            makeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const labelData: Interval[] = [...this.intervals]; //, ...this.trend];
                labelData.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                const secondsData: number[] = labelData.map(iter => (iter.timestamp.getTime() - this.session.start.getTime()) / 1000);
                const labels: string[] = secondsData.map(iter => TimeUtils.duration(iter));

                console.log(labels);

                const ctx = (document.getElementById("chart-timestamp-kill-death") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: labels,
                        datasets: [
                            {
                                // x = how many seconds into the session
                                data: this.intervals.map(iter => { return { x: (iter.timestamp.getTime() - this.session.start.getTime()) / 1000, y: (iter.kills / Math.max(1, iter.deaths)) }; }),
                                borderWidth: 3,
                                borderColor: "#fff",
                                label: "Interval"
                            },
                            {
                                data: this.trend.map(iter => { return { x: (iter.timestamp.getTime() - this.session.start.getTime()) / 1000, y: (iter.kills / Math.max(1, iter.deaths)) }; }),
                                borderWidth: 3,
                                borderColor: "#ff0000",
                                label: "Trend"
                            }
                        ],
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        parsing: false,
                        elements: {
                            point: {
                                radius: 0
                            }
                        },
                        scales: {
                            x: {
                                type: "linear",
                                beginAtZero: true,
                                min: 0,
                                max: ((this.session.end ?? new Date()).getTime() - this.session.start.getTime()) / 1000,
                                ticks: {
                                    callback: (value: string | number, index: number, values) => {
                                        const val: number = typeof (value) == "string" ? Number.parseInt(value) : value;
                                        const dur: moment.Duration = moment.duration(val * 1000);

                                        return `${dur.hours()}:${dur.minutes().toString().padStart(2, "0")}`;
                                    }
                                }
                            }
                        },
                        plugins: {
                            tooltip: {
                                mode: "x",
                                intersect: false,
                            }
                        }
                    }
                });
            }

        }
    });
    export default ChartKillDeathRatioTrend;
</script>