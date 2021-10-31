<template>
    <div class="d-flex">
        <table class="table table-sm w-auto d-inline-block" style="vertical-align: top;">
            <thead>
                <tr class="table-secondary">
                    <th>Weapon</th>
                    <th>Kills</th>
                    <th>HS kills</th>
                    <th>HSR</th>
                    <th>%</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="entry in groupedKillEventsArray">
                    <td>
                        <span v-if="groupedKillWeapons.get(entry[0])">
                            {{groupedKillWeapons.get(entry[0]).name}}
                        </span>
                        <span v-else>
                            &lt;missing {{entry[0]}}&gt;
                        </span>
                    </td>

                    <td>
                        {{entry[1].length | locale}}
                    </td>

                    <td>
                        {{entry[1].filter(iter => iter.isHeadshot == true).length}}
                    </td>

                    <td>
                        {{entry[1].filter(iter => iter.isHeadshot == true).length / entry[1].length * 100 | fixed | locale}}%
                    </td>

                    <td>
                        {{entry[1].length / kills.data.length * 100 | fixed | locale}}%
                    </td>
                </tr>

                <tr class="table-secondary">
                    <td>
                        <b>Total</b>
                    </td>

                    <td>
                        {{kills.data.length | locale}}
                    </td>

                    <td colspan="3">
                        {{kills.data.filter(iter => iter.event.isHeadshot == true).length / kills.data.length * 100 | fixed | locale}}%
                    </td>
                </tr>
            </tbody>
        </table>

        <canvas id="chart-kills-weapon-usage" style="max-height: 300px; max-width: 25%" class="w-auto d-inline-block mb-2"></canvas>

        <canvas id="chart-kpm-intervals" style="max-height: 300px; max-width: 25%;" class="w-auto d-inline-block mb-2"></canvas>

        <canvas id="chart-kpm-trend" style="max-height: 300px; max-width: 50%;" class="w-auto d-inline-block mb-2"></canvas>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";
    import * as moment from "moment";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import { randomRGB, rgbToString } from "util/Color";

    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { PsItem } from "api/ItemApi";
    import { Session } from "api/SessionApi";

    interface KpmInterval {
        kpm: number;
        timestamp: Date;
    }

    export const SessionViewerKills = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            kills: { type: Object as PropType<Loading<ExpandedKillEvent[]>>, required: true }
        },

        data: function() {
            return {
                chart: null as Chart | null,

                kpmIntervalPeriod: 30 as number,

                kpmIntervalsChart: null as Chart | null,
                kpmTrend: [] as KpmInterval[],
                kpmIntervals: [] as KpmInterval[],
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.generateKillWeaponChart();
                this.generateKpmIntervals();
                this.generateKpmTrend();
                this.generateKpmChart();
            });
        },

        methods: {
            generateKillWeaponChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                }

                const groupedEvents: Map<string, KillEvent[]> = this.groupedKillEvents;

                const ctx = (document.getElementById("chart-kills-weapon-usage") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: Array.from(groupedEvents.entries()).map((iter) => {
                            const weaponID: string = iter[0];
                            const weaponName: string = `${this.groupedKillWeapons.get(weaponID)?.name ?? `<missing ${weaponID}>`}`;
                            return `${weaponName} - ${(iter[1].length / (this.kills as any).data.length * 100).toFixed(2)}%`;
                        }),
                        datasets: [{
                            data: Array.from(groupedEvents.values()).map(iter => iter.length),
                            backgroundColor: Array.from(groupedEvents.keys()).map(_ => rgbToString(randomRGB()))
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: "right",
                                labels: {
                                    color: "#fff"
                                }
                            }
                        },
                    }
                });
            },

            generateKpmTrend: function(): void {
                this.kpmTrend = [];

                if (this.kills.state != "loaded" || this.kills.data.length == 0) {
                    return;
                }

                const start: number = this.session.start.getTime();

                let total: number = 0;

                this.kpmTrend.push({
                    kpm: 0,
                    timestamp: this.session.start
                });

                for (const kill of this.kills.data) {
                    ++total;

                    const totalTime: number = (kill.event.timestamp.getTime() - start) / 1000 / 60;

                    //console.log(`total = ${total}, ${kill.event.timestamp} totalTime = ${totalTime} = ${total / totalTime}`);

                    this.kpmTrend.push({
                        kpm: total / totalTime,
                        timestamp: kill.event.timestamp
                    });
                }

                const last: number = (this.session.end || new Date()).getTime();

                this.kpmTrend.push({
                    kpm: total / ((last - start) / 1000 / 60),
                    timestamp: (this.session.end || new Date())
                });

            },

            generateKpmIntervals: function(): void {
                this.kpmIntervals = [];

                if (this.kills.state != "loaded" || this.kills.data.length == 0) {
                    return;
                }

                let previous: KpmInterval[] = [];

                const start: number = this.session.start.getTime();
                const last: number = (this.session.end || new Date()).getTime();

                //console.log(`Kills go from ${start} to ${last}`);

                for (let i = start; i <= last; i += this.kpmIntervalPeriod * 1000) {
                    //console.log(`Getting kills from ${i} - ${i + this.kpmIntervalPeriod * 1000}`);

                    const slice: ExpandedKillEvent[] = this.kills.data.filter(iter => iter.event.timestamp.getTime() >= i && iter.event.timestamp.getTime() < (i + this.kpmIntervalPeriod * 1000));
                    //console.log(`\tGot ${slice.length} kills in this interval`);

                    if (previous.length > 8) {
                        previous.shift();
                    }

                    let kpm: number = slice.length / (this.kpmIntervalPeriod / 60);
                    //console.log(`\tkpm: ${kpm}`);

                    const kpms: number[] = [...previous.map(iter => iter.kpm), kpm];
                    const sum: number = kpms.reduce((prev, curr) => prev + curr, 0);

                    //console.log(`\tkpms: [${kpms.join(", ")}], SUM: ${sum}, LEN: ${kpms.length}, AVG: ${sum / kpms.length}`);

                    let interval: KpmInterval = {
                        kpm: kpm,
                        timestamp: new Date(i)
                    };

                    previous.push({ ...interval });

                    interval.kpm = sum / kpms.length;
                    this.kpmIntervals.push(interval);
                }
            },

            generateKpmChart: function(): void {
                if (this.kpmIntervalsChart != null) {
                    this.kpmIntervalsChart.destroy();
                    this.kpmIntervalsChart = null;
                }

                const ctx = (document.getElementById("chart-kpm-intervals") as any).getContext("2d");
                this.kpmIntervalsChart = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: this.kpmIntervals.map(_ => ""),
                        datasets: [
                            {
                                data: this.kpmIntervals.map(iter => { return { x: (iter.timestamp.getTime() - this.session.start.getTime()) / 1000, y: iter.kpm }; }),
                                borderWidth: 3,
                                borderColor: "#fff",
                                label: "KPM intervals"
                            },
                            {
                                data: this.kpmTrend.map(iter => { return { x: (iter.timestamp.getTime() - this.session.start.getTime()) / 1000, y: iter.kpm }; }), // x = how many seconds into the session
                                borderWidth: 3,
                                borderColor: "#ff0000",
                                label: "KPM trend"
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
                                max: ((this.session.end || new Date()).getTime() - this.session.start.getTime()) / 1000, // How many seconds the session was for
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
                    },
                });
            },

        },

        computed: {
            groupedKillEvents: function(): Map<string, KillEvent[]> {
                if (this.kills.state != "loaded") {
                    return new Map();
                }

                return this.kills.data.reduce(
                    (entryMap: Map<string, KillEvent[]>, event: ExpandedKillEvent) => entryMap.set(event.event.weaponID, [...entryMap.get(event.event.weaponID) || [], event.event]),
                    new Map()
                );
            },

            groupedKillWeapons: function(): Map<string, PsItem> {
                if (this.kills.state != "loaded") {
                    return new Map();
                }

                const map: Map<string, PsItem> = new Map();
                for (const iter of this.kills.data) {
                    if (map.has(iter.event.weaponID) == false && iter.item != null) {
                        map.set(iter.event.weaponID, iter.item);
                    }
                }

                return map;
            },

            groupedKillEventsArray: function(): any[] {
                return Array.from(this.groupedKillEvents.entries());
            },

            durationInSeconds: function(): number {
                return ((this.session.end || new Date()).getTime() - this.session.start.getTime()) / 1000;
            }
        }

    });
    export default SessionViewerKills;
</script>