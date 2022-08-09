<template>
    <canvas id="chart-timestamp-spm" style="max-height: 300px;" class="w-auto mb-2"></canvas>
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
    import { ExpEvent } from "api/ExpStatApi";

    import TimeUtils from "util/Time";

    type Interval = {
        timestamp: Date;
        score: number;
    };

    export const ChartScorePerMinuteTrend = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Array as PropType<ExpEvent[]>, required: true },
        },

        data: function() {
            return {
                intervals: [] as Interval[],
                trend: [] as Interval[],
                period: 30 as number,
                chart: null as Chart | null,
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeTrend();
                this.makeIntervals();
                this.makeChart();
            });
        },

        methods: {
            makeIntervals: function(): void {
                this.intervals = [];

                let previous: Interval[] = [];

                const start: number = this.session.start.getTime();
                const end: number = (this.session.end ?? new Date()).getTime();

                for (let i = start; i <= end; i += 1000 * this.period) {
                    const slice: ExpEvent[] = this.exp.filter(iter => iter.timestamp.getTime() >= i && iter.timestamp.getTime() < (i + this.period * 1000));

                    if (previous.length > 8) {
                        previous.shift();
                    }

                    let interval: Interval = {
                        timestamp: new Date(i),
                        score: slice.reduce((acc, i) => acc += i.amount, 0)
                    };

                    const sumScore: number = previous.reduce((acc, i) => acc += i.score, 0);

                    previous.push({ ...interval });

                    interval.score += sumScore;

                    this.intervals.push(interval);
                }
            },

            makeTrend: function(): void {
                this.trend = [];

                let totalScore: number = 0;

                this.trend.push({
                    timestamp: this.session.start,
                    score: 0
                });

                for (const ev of this.exp) {
                    totalScore += ev.amount;

                }

                /*
                this.trend.push({
                    timestamp: (this.session.end ?? new Date()),
                    kills: totalKills,
                    deaths: totalDeaths
                });
                */
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
    export default ChartScorePerMinuteTrend;
</script>
