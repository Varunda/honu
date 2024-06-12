
<template>
    <div>
        <div class="mx-3 border-bottom pb-2 mb-3">
            <h5>
                Fields
            </h5>

            <button v-for="kvp in show" @click="toggleView(kvp[0])" class="btn mr-2" :class="[ kvp[1] == true ? 'btn-primary' : 'btn-secondary' ]">
                {{getFieldName(kvp[0])}}
            </button>
        </div>

        <div class="mx-3 border-bottom pb-2 mb-3">
            <h5>
                Group by
            </h5>

            <div class="btn-group">
                <button class="btn" @click="period = 'session'" :class="[ period == 'session' ? 'btn-primary' : 'btn-secondary' ]">
                    Per session
                </button>
                <button class="btn" @click="period = 'day'" :class="[ period == 'day' ? 'btn-primary' : 'btn-secondary' ]">
                    Per day
                </button>
                <button class="btn" @click="period = 'week'" :class="[ period == 'week' ? 'btn-primary' : 'btn-secondary' ]">
                    Per week
                </button>
                <button class="btn" @click="period = 'month'" :class="[ period == 'month' ? 'btn-primary' : 'btn-secondary' ]">
                    Per month
                </button>
            </div>

            <toggle-button v-model="showLabels">
                Show labels
            </toggle-button>

            <span class="text-muted">
                Showing {{groupedSession.length}} entires
            </span>

        </div>

        <div v-if="loading == true" class="d-flex mb-3">
            <div class="flex-grow-1 flex-basis-0"></div>
            <div class="flex-grow-1 flex-basis-1 text-center">
                loading...
                <busy class="honu-busy"></busy>
            </div>
            <div class="flex-grow-1 flex-basis-0"></div>
        </div>

        <div v-show="loading == false">
            <div id="session-stat" class="w-100" style="max-height: 80vh; height: 40vh"></div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import ToggleButton from "components/ToggleButton";
    import Busy from "components/Busy.vue";

    import * as moment from "moment";
    import * as hc from "highcharts";
    import * as hs from "highcharts/highstock";
    import "highcharts/modules/drag-panes";
    import "highcharts/modules/annotations";

    import ColorUtils from "util/Color";
    import TimeUtils from "util/Time";
    import LocaleUtil from "util/Locale";

    import { FlatSession } from "./common";

    //////////////////////////////
    // strongly typing keys used in the graph
    //////////////////////////////
    type StatPeriod = "session" | "day" | "week" | "month";

    // guh
    const _statField = ["duration", "expGained", "expPerMinute",
        "kills", "vkills", "deaths", "kd", "kpm", "vkpm", "spawns", "spawnsPerMinute",
        "heals", "revives", "shieldRepairs", "healsPerMinute", "revivesPerMinute", "shieldRepairsPerMinute",
        "maxRepairs", "resupplies", "maxRepairsPerMinute", "resuppliesPerMinute"] as const;
    type StatField = typeof _statField[number];

    const ValueToYAxis: Map<StatField, number> = new Map(
        _statField.map((iter, index) => {
            return [iter, index];
        })
    );

    const YAxisToValue: Map<number, StatField> = new Map(
        _statField.map((iter, index) => {
            return [index, iter];
        })
    );

    const getDataPoint = (value: StatField, entry: FlatSession): number => {
        return entry[value];
    };

    const getFieldName = (value: StatField): string => {
        switch (value) {
            case "duration": return "Duration";
            case "kills": return "Kills";
            case "kpm": return "KPM";
            case "deaths": return "Deaths";
            case "kd": return "KD";
            case "expGained": return "Exp";
            case "expPerMinute": return "Exp/Min";
            case "vkills": return "V.Kills";
            case "vkpm": return "V.Kills/Min";
            case "spawns": return "Spawns";
            case "spawnsPerMinute": return "Spawns/Min";
            case "heals": return "Heals";
            case "revives": return "Revives";
            case "shieldRepairs": return "Shield Reps";
            case "healsPerMinute": return "Heals/Min";
            case "revivesPerMinute": return "Revives/Min";
            case "shieldRepairsPerMinute": return "Shield Reps/Min";
            case "maxRepairs": return "MAX Reps";
            case "resupplies": return "Resupplies";
            case "maxRepairsPerMinute": return "MAX Reps/Min";
            case "resuppliesPerMinute": return "Resupplies/Min";
            default:
                const _check: never = value;
                return _check;
        }
    };

    function createYAxis(name: string, visible: boolean): hc.YAxisOptions {
        return {
            min: 0,
            title: {
                text: name,
                style: {
                    color: "#ffffff"
                }
            },
            labels: {
                style: {
                    color: "#ffffff"
                }
            },
            resize: {
                enabled: true
            },
            visible: visible
        };
    }

    function getPointText(name: StatField, value: number) {
        switch (name) {
            case "duration": return `${TimeUtils.duration(value)}`; 

            case "kills": return `${LocaleUtil.locale(value, 0)}`; 
            case "deaths": return `${LocaleUtil.locale(value, 0)}`; 
            case "expGained": return `${LocaleUtil.locale(value, 0)}`; 
            case "vkills": return `${LocaleUtil.locale(value, 0)}`; 
            case "spawns": return `${LocaleUtil.locale(value, 0)}`; 
            case "heals": return `${LocaleUtil.locale(value, 0)}`; 
            case "revives": return `${LocaleUtil.locale(value, 0)}`; 
            case "shieldRepairs": return `${LocaleUtil.locale(value, 0)}`; 
            case "maxRepairs": return `${LocaleUtil.locale(value, 0)}`; 
            case "resupplies": return `${LocaleUtil.locale(value, 0)}`; 

            case "kpm": return `${LocaleUtil.locale(value, 2)}`; 
            case "kd": return `${LocaleUtil.locale(value, 2)}`; 
            case "expPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            case "vkpm": return `${LocaleUtil.locale(value, 2)}`; 
            case "spawnsPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            case "healsPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            case "revivesPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            case "shieldRepairsPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            case "maxRepairsPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            case "resuppliesPerMinute": return `${LocaleUtil.locale(value, 2)}`; 
            default:
                const _check: never = name;
                return _check;
        }
    }


    let CHART: hc.StockChart | null = null;

    /*
    * actual component now
    */
    export const SessionChart = Vue.extend({
        props: {
            entries: { type: Array as PropType<FlatSession[]>, required: true }

        },

        data: function() {
            return {
                data: [] as hc.SeriesOptionsType[],
                sessions: [] as FlatSession[],

                loading: false as boolean,

                range: {
                    start: new Date() as Date,
                    end: new Date() as Date
                },

                showLabels: true as boolean,

                period: "session" as StatPeriod,

                show: new Map(_statField.map(iter => {
                    return [iter, iter == "kpm" || iter == "kd" ? true : false];
                })) as Map<StatField, boolean>,
            }
        },

        created: function(): void {
            this.sessions = [...this.entries];
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeGraph();
            });
        },

        methods: {
            setRange: function(scalar: number, unit: string): void {
                this.range.end = new Date();
                this.range.start = moment(this.range.end).subtract(scalar, unit as any).toDate();
            },

            toggleView: function(field: StatField): void {
                if (CHART == null) {
                    console.warn(`SessionChart> cannot toggle view of field, chart is not made [field=${field}]`);
                    return;
                }

                const yaxisIndex: number | undefined = ValueToYAxis.get(field);
                if (yaxisIndex == undefined) {
                    throw `missing index for field '${field}'`;
                }

                const isVisible: boolean | undefined = this.show.get(field);
                if (isVisible == undefined) { // not sure how this could happen
                    throw `huh`;
                }

                console.log(`SessionChart> toggling vis of axis [field=${field}] [index=${yaxisIndex}] [visible=${isVisible}->${!isVisible}]`);

                // couple things to do:
                // 1. update the axis so it's visible, this shows the legend
                // 2. update the series so it's visible.
                //      if the series is set as visible but the axis is not, it still shows
                // 3. update the map that displays what fields are graphed
                // 4. force update the component, as .set on a Map is not reactive
                const axis = CHART.yAxis[yaxisIndex];
                axis.update({ visible: !isVisible }, false);
                axis.series[0].setVisible(!isVisible, true);
                this.show.set(field, !isVisible);
                this.$forceUpdate();
            },

            // HACK: i can't figure out how to get a loading indicator to work before making the chart
            //      uses all the CPU, so just add a delay lol
            makeGraph: function(): void {
                this.loading = true;
                setTimeout(() => {
                    this._makeChartReal();
                    this.loading = false;
                }, 50);
            },

            _makeChartReal: function(): void {
                let colors: string[] = ColorUtils.randomColors(0.2, this.show.size);

                console.log(`SessionChart> have ${colors.length} keys for ${this.sessions.length} sessions`);

                if (CHART != null) {
                    CHART.destroy();
                    CHART = null;
                }

                const period: StatPeriod = this.period;

                console.time("SessionChart> make graph");
                CHART = hs.stockChart("session-stat", {
                    // no title, hides it
                    title: { text: "" },

                    // allow zooming in on the X axis
                    chart: {
                        zooming: {
                            type: "x"
                        },
                        backgroundColor: "#222222",
                    },

                    // white text for legend
                    legend: {
                        itemStyle: { color: "#ffffff" }
                    },

                    xAxis: {
                        crosshair: true,
                        labels: {
                            style: { color: "#ffffff" },
                            formatter: (v) => {
                                const d: Date = new Date(v.value);
                                switch (period) {
                                    case "session": return TimeUtils.formatNoTimezone(d, "YYYY-MM-DD HH:mmA");
                                    case "day": return TimeUtils.formatNoTimezone(d, "YYYY-MM-DD");
                                    case "week": return TimeUtils.formatNoTimezone(d, "YYYY-MM-DD");
                                    case "month": return TimeUtils.formatNoTimezone(d, "YYYY-MM");
                                    default: 
                                        const _check: never = period;
                                        return _check;
                                }
                            }
                        }
                    },

                    plotOptions: {
                        series: {
                            dataLabels: {
                                enabled: this.showLabels,
                                style: {
                                    fontSize: "14px"
                                },
                                formatter: function() {
                                    return getPointText(this.series.name as any, this.y || 0);
                                },
                            }
                        }
                    },

                    // create a Y axis for each field that can be shown
                    // this lets fields of different maginitude be shown
                    // for example, duration is often in the thousands, while kpm is in the ones,
                    // so if duration and kpm were on the same Y axis, the kpm series/line would be so small
                    yAxis: _statField.map(iter => {
                        const v = this.show.get(iter);
                        if (v == undefined) {
                            throw `missing show value for ${iter}`;
                        }
                        return createYAxis(getFieldName(iter), v);
                    }),

                    tooltip: {
                        split: false,
                        headerFormat: "",
                        useHTML: true,
                        pointFormatter: function(): string {
                            const series = this.series.chart.yAxis
                                .filter(iter => iter.series[0].visible == true)
                                .map(iter => iter.series[0]);

                            const d: Date = new Date(this.x);

                            let ret: string = `${TimeUtils.format(d)}<table class="table table-sm mb-0 text-dark">`;

                            for (const s of series) {
                                if (s.name.startsWith("Navigator")) {
                                    continue;
                                }

                                ret += "<tr><td><b>" + s.name + "</b></td><td>";

                                const name: StatField = s.name as StatField;
                                const point = s.data[this.index];

                                if (point.y != undefined) {
                                    ret += getPointText(name, point.y);
                                }

                                ret += `</td></tr>`;
                            }

                            ret += `</table>`;

                            return ret;
                        }
                    },

                    series: this.generateSeries(this.groupedSession, colors),

                    annotations: [
                        {
                            crop: false,
                            labels: [
                                {
                                    point: { x: 0, y: 0, xAxis: 0, yAxis: 0 },
                                    text: "Honu starts",
                                },
                                {
                                    point: "max",
                                    text: "Max"
                                }
                            ]
                        }
                    ]
                });
                console.timeEnd("SessionChart> make graph");
                (window as any).chart = CHART;

            },

            generateSeries: function(entries: FlatSession[], colors: string[]): hc.SeriesOptionsType[] {
                console.time("SessionChart> generate series");
                const arr: hc.SeriesOptionsType[] = [];

                for (let i = 0; i < _statField.length; ++i) {
                    const value: StatField = _statField[i];

                    const yaxisIndex: number | undefined = ValueToYAxis.get(value);
                    if (yaxisIndex == undefined) {
                        throw `Missing y axis of ${value}`;
                    }

                    const options: hc.SeriesOptionsType = {
                        type: "line",
                        name: `${value}`,
                        data: entries.map(iter => {
                            return {
                                x: iter.start.getTime(),
                                y: getDataPoint(value, iter)
                            };
                        }),
                        turboThreshold: 10000000000,
                        color: colors[i],
                        yAxis: yaxisIndex,
                        visible: this.show.get(value)!,
                    };

                    console.log(`SessionChart> yaxis index for ${value} is on ${options.yAxis}`);

                    arr.push(options);
                }
                console.timeEnd("SessionChart> generate series");

                return arr;
            }
        },

        computed: {
            groupedSession: function(): FlatSession[] {
                if (this.period == "session") {
                    return this.sessions;
                }
                if (this.sessions.length == 0) {
                    return [];
                }

                const arr: FlatSession[] = [];

                console.time("SessionChart> group sessions");

                const t = (s: string, session: FlatSession): void => {
                    if (iterDate != s) {
                        console.log(`SessionChart> iteration done [iterDate=${iterDate}] [s=${s}]`);

                        // recalc all the stats that rely on other stats, as those are messed up by summation
                        iter.kd = iter.kills / Math.max(iter.deaths);
                        iter.kpm = iter.kills / Math.max(1, iter.duration) * 60;
                        iter.vkpm = iter.vkills / Math.max(1, iter.duration) * 60;
                        iter.healsPerMinute = iter.heals / Math.max(1, iter.duration) * 60;
                        iter.revivesPerMinute = iter.revives / Math.max(1, iter.duration) * 60;
                        iter.shieldRepairsPerMinute = iter.shieldRepairs / Math.max(1, iter.duration) * 60;
                        iter.expPerMinute = iter.expGained / Math.max(1, iter.duration) * 60;
                        iter.maxRepairsPerMinute = iter.maxRepairs / Math.max(1, iter.duration) * 60;
                        iter.resuppliesPerMinute = iter.resupplies / Math.max(1, iter.duration) * 60;
                        iter.spawnsPerMinute = iter.spawns / Math.max(1, iter.duration) * 60;

                        arr.push(iter);
                        iter = { ...session };
                        iterDate = null;
                    } else {
                        iter.duration += session.duration;
                        iter.kills += session.kills;
                        iter.vkills += session.vkills;
                        iter.deaths += session.deaths;
                        iter.spawns += session.spawns;
                        iter.expGained += session.expGained;
                        iter.heals += session.heals;
                        iter.revives += session.revives;
                        iter.shieldRepairs += session.shieldRepairs;
                        iter.resupplies += session.resupplies;
                        iter.maxRepairs += session.maxRepairs;
                    }
                }

                let iter: FlatSession = { ...this.sessions[0] };
                let iterDate: string | null = null;
                for (const session of this.sessions.slice(1)) {
                    if (this.period == "day") {
                        const s = moment(session.start).format("YYYY-MM-DD");
                        if (iterDate == null) {
                            iterDate = moment(session.start).format("YYYY-MM-DD");
                        }

                        t(s, session);
                    } else if (this.period == "week") {
                        const s = moment(session.start).format("YYYY-WW");
                        if (iterDate == null) {
                            iterDate = moment(session.start).format("YYYY-WW");
                        }

                        t(s, session);

                    } else if (this.period == "month") {
                        const s = moment(session.start).format("YYYY-MM");
                        if (iterDate == null) {
                            iterDate = moment(session.start).format("YYYY-MM");
                        }

                        t(s, session);
                    }
                }

                if (arr.length == 0) {
                    arr.push(iter);
                }

                console.timeEnd("SessionChart> group sessions");

                return arr;
            },

            shownValues: function(): StatField[] {
                const values: StatField[] = [];

                for (const kvp of this.show) {
                    if (kvp[1] == true) {
                        values.push(kvp[0]);
                    }
                }

                return values;
            },

            getFieldName: function() {
                return getFieldName;
            }
        },

        watch: {
            period: function(): void {
                this.$nextTick(() => {
                    this.makeGraph();
                });
            },

            showLabels: function(): void {
                this.makeGraph();
            }
        },

        components: {
            ToggleButton, Busy
        }
    });
    export default SessionChart;
</script>