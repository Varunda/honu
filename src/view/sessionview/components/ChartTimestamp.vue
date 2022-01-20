<template>
    <canvas :id="'chart-timestamp-' + ID" style="max-height: 300px;" class="w-auto mb-2"></canvas>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import * as moment from "moment";

    import Chart from "chart.js/auto/auto.esm";

    interface Interval {
        value: number;
        timestamp: Date;
    };

    export const ChartTimestamp = Vue.extend({
        props: {
            data: { type: Array as PropType<Date[]>, required: true },
            start: { type: Date, required: true },
            end: { type: Date, required: true },
        },

        data: function() {
            return {
                chart: null as Chart | null,

                sortedData: [] as Date[],

                ID: 0 as number,

                period: 30 as number,

                interval: [] as Interval[],
                trend: [] as Interval[]
            }
        },

        beforeMount: function(): void {
            this.ID = Math.floor(Math.random() * 100000);

            this.$nextTick(() => {
                this.generateInterval();
                this.generateTrend();
                this.makeChart();
            });
        },

        mounted: function(): void {
            this.sortedData = [...this.data].sort((a, b) => a.getTime() - b.getTime());
        },

        methods: {
            generateInterval: function(): void {
                this.interval = [];

                if (this.data.length < 2) {
                    return;
                }

                let previous: Interval[] = [];

                const start: number = this.startDate.getTime();
                const last: number = this.endDate.getTime();

                //console.log(`Kills go from ${start} to ${last}`);

                for (let i = start; i <= last; i += this.period * 1000) {
                    //console.log(`Getting kills from ${i} - ${i + this.kpmIntervalPeriod * 1000}`);

                    const slice: Date[] = this.sortedData.filter(iter => iter.getTime() >= i && iter.getTime() < (i + this.period * 1000));
                    //console.log(`\tGot ${slice.length} kills in this interval`);

                    if (previous.length > 8) {
                        previous.shift();
                    }

                    let kpm: number = slice.length / (this.period / 60);
                    //console.log(`\tkpm: ${kpm}`);

                    const kpms: number[] = [...previous.map(iter => iter.value), kpm];
                    const sum: number = kpms.reduce((prev, curr) => prev + curr, 0);

                    //console.log(`\tkpms: [${kpms.join(", ")}], SUM: ${sum}, LEN: ${kpms.length}, AVG: ${sum / kpms.length}`);

                    let interval: Interval = {
                        value: kpm,
                        timestamp: new Date(i)
                    };

                    previous.push({ ...interval });

                    interval.value = sum / kpms.length;
                    this.interval.push(interval);
                }
            },

            generateTrend: function(): void {
                this.trend = [];

                if (this.data.length < 2) {
                    return;
                }

                const start: number = this.startDate.getTime();

                let total: number = 0;

                this.trend.push({
                    value: 0,
                    timestamp: this.data[0]
                });

                for (const kill of this.sortedData) {
                    ++total;

                    const totalTime: number = (kill.getTime() - start) / 1000 / 60;

                    //console.log(`total = ${total}, ${kill.event.timestamp} totalTime = ${totalTime} = ${total / totalTime}`);

                    this.trend.push({
                        value: total / totalTime,
                        timestamp: kill
                    });
                }

                const last: number = this.endDate.getTime();

                this.trend.push({
                    value: total / ((last - start) / 1000 / 60),
                    timestamp: new Date(last)
                });
            },

            makeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                const ctx = (document.getElementById(`chart-timestamp-${this.ID}`) as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: this.interval.map(_ => ""),
                        datasets: [
                            {
                                data: this.interval.map(iter => { return { x: (iter.timestamp.getTime() - this.startDate.getTime()) / 1000, y: iter.value }; }),
                                borderWidth: 3,
                                borderColor: "#fff",
                                label: "Interval"
                            },
                            {
                                data: this.trend.map(iter => { return { x: (iter.timestamp.getTime() - this.startDate.getTime()) / 1000, y: iter.value }; }), // x = how many seconds into the session
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
                                max: (this.endDate.getTime() - this.startDate.getTime()) / 1000,
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
        },

        computed: {
            endDate: function(): Date {
                return (this.end as any);
            },

            startDate: function(): Date {
                return (this.start as any);
            }
        }

    });
    export default ChartTimestamp;
</script>