<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/population">Population</a>
            </li>
        </honu-menu>

        <collapsible header-text="Options">
            <div class="d-flex">
                <div class="d-flex flex-column">
                    <h4>Date range</h4>
                    <div class="w-100">
                        <label>Start:</label>
                        <date-time-input v-model="range.start" class="form-control"></date-time-input>
                    </div>

                    <div class="w-100">
                        <label>End:</label>
                        <date-time-input v-model="range.end" class="form-control"></date-time-input>
                    </div>
                </div>

                <div class="d-flex flex-column ml-3">
                    <button class="btn btn-secondary w-100" @click="lastAll">
                        Since 2021-07-21
                        <info-hover text="This is when Honu started tracking session data"></info-hover>
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(1, 'year')">
                        Last year
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(6, 'months')">
                        Last 6 months
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(3, 'months')">
                        Last 3 months
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(1, 'month')">
                        Last month
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(2, 'weeks')">
                        Last 2 weeks
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(7, 'days')">
                        Last 7 days
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(3, 'days')">
                        Last 3 days
                    </button>
                    <button class="btn btn-secondary w-100" @click="setRange(24, 'hours')">
                        Last 24 hours
                    </button>
                </div>

                <div class="d-flex flex-column ml-3">
                    <h4>Servers</h4>
                    <toggle-button class="w-100" v-model="worlds.all" false-color="btn-secondary">
                        All servers
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.osprey" false-color="btn-secondary">
                        Osprey (US)
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.wainwright" false-color="btn-secondary">
                        Wainwright (EU)
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.cobalt" false-color="btn-secondary">
                        Cobalt
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.emerald" false-color="btn-secondary">
                        Emerald
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.soltech" false-color="btn-secondary">
                        SolTech
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.jaeger" false-color="btn-secondary">
                        Jaeger
                    </toggle-button>
                </div>

                <div class="d-flex flex-column ml-3">
                    <h4>Factions</h4>
                    <toggle-button class="w-100" v-model="factions.all" false-color="btn-secondary">
                        All factions
                    </toggle-button>

                    <toggle-button class="w-100" v-model="factions.vs" false-color="btn-secondary">
                        VS
                    </toggle-button>

                    <toggle-button class="w-100" v-model="factions.nc" false-color="btn-secondary">
                        NC
                    </toggle-button>

                    <toggle-button class="w-100" v-model="factions.tr" false-color="btn-secondary">
                        TR
                    </toggle-button>

                    <toggle-button class="w-100" v-model="factions.ns" false-color="btn-secondary">
                        NS
                    </toggle-button>
                </div>

                <div class="d-flex flex-column ml-3">
                    <h4>Y axis</h4>
                    <toggle-button class="w-100" v-model="show.unique" false-color="btn-secondary">
                        Show unique characters with sessions
                        <info-hover text="How many unique characters had a session occur within this timeframe"></info-hover>
                    </toggle-button>
                    <toggle-button class="w-100" v-model="show.total" false-color="btn-secondary">
                        Show total sessions
                        <info-hover text="How many sessions in total took over within this timeframe"></info-hover>
                    </toggle-button>
                    <toggle-button class="w-100" v-model="show.logins" false-color="btn-secondary">
                        Show session starts
                        <info-hover text="How many sessions were started within this timeframe"></info-hover>
                    </toggle-button>
                    <toggle-button class="w-100" v-model="show.logouts" false-color="btn-secondary">
                        Show session finishes
                        <info-hover text="How many sessions were concluded within this timeframe"></info-hover>
                    </toggle-button>
                    <toggle-button class="w-100" v-model="show.length" false-color="btn-secondary">
                        Show time played by all
                        <info-hover text="How much time was spent online by players"></info-hover>
                    </toggle-button>
                    <toggle-button class="w-100" v-model="show.average" false-color="btn-secondary">
                        Show average session length
                        <info-hover text="How long a session that occured within this timeframe lasted on average"></info-hover>
                    </toggle-button>
                </div>
            </div>

            <button class="btn btn-primary w-100" @click="loadEntries">
                Load
            </button>
        </collapsible>

        <hr class="border" />

        <div>
            <div v-if="entries.state == 'idle'">
                No data loaded
            </div>

            <div v-else-if="entries.state == 'loading'">
                <busy class="honu-busy"></busy>
            </div>

            <div v-else-if="entries.state == 'loaded'">
                Loaded {{entries.data.length}} data points

                <div id="hc-chart" style="height: 80vh; max-height: 80vh;" class="mb-2 w-100"></div>
            </div>

            <div v-else-if="entries.state == 'error'">
                Error loading data:
                <br />
                {{entries.message}}
            </div>

        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import * as moment from "moment";

    import * as hc from "highcharts";
    import * as hs from "highcharts/highstock";
    import "highcharts/modules/annotations";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";
    import Busy from "components/Busy.vue";
    import DateTimeInput from "components/DateTimeInput.vue";
    import Collapsible from "components/Collapsible.vue";

    import { PopulationEntry, PopulationApi } from "api/PopulationApi";

    import WorldUtils from "util/World";
    import ColorUtils from "util/Color";
    import FactionUtils from "util/Faction";
    import TimeUtils from "util/Time";

    const ValueToYAxis: Map<string, number> = new Map([
        ["total", 0],
        ["logins", 1],
        ["logouts", 2],
        ["unique", 3],
        ["length", 4],
        ["average", 5]
    ]);

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
            visible: visible
        };
    }

    export const Population = Vue.extend({
        props: {

        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<PopulationEntry[]>,

                chart: null as hc.StockChart | null,

                range: {
                    start: new Date() as Date,
                    end: new Date() as Date
                },

                worlds: {
                    all: true as boolean,
                    osprey: false as boolean,
                    wainwright: false as boolean,
                    cobalt: false as boolean,
                    emerald: false as boolean,
                    soltech: false as boolean,
                    jaeger: false as boolean
                },

                factions: {
                    all: true as boolean,
                    vs: false as boolean,
                    nc: false as boolean,
                    tr: false as boolean,
                    ns: false as boolean
                },

                show: {
                    total: false as boolean,
                    logins: false as boolean,
                    logouts: false as boolean,
                    unique: true as boolean,
                    length: false as boolean,
                    average: false as boolean
                }
            }
        },

        created: function(): void {
            document.title = "Honu / Population";

            this.range.start = moment(this.range.end).subtract(1, "month").toDate();
        },

        methods: {
            loadEntries: async function(): Promise<void> {
                this.entries = Loadable.loading();
                this.entries = await PopulationApi.getHistorical(this.range.start, this.range.end, this.queryWorlds, this.queryFactions);

                if (this.entries.state == "loaded") {
                    this.$nextTick(() => {
                        this.makeGraph();
                    });
                }
            },

            setRange: function(scalar: number, unit: string): void {
                this.range.end = new Date();
                this.range.start = moment(this.range.end).subtract(scalar, unit as any).toDate();
                if (this.entries.state == "loaded") { this.loadEntries(); }
            },

            lastAll: function(): void {
                this.range.end = new Date();
                this.range.start = new Date(0);
                if (this.entries.state == "loaded") { this.makeGraph(); }
            },

            getDataPoint: function(value: string, entry: PopulationEntry): number {
                if (value == "total") {
                    return entry.total;
                } else if (value == "logins") {
                    return entry.logins;
                } else if (value == "logouts") {
                    return entry.logouts;
                } else if (value == "unique") {
                    return entry.uniqueCharacters;
                } else if (value == "length") {
                    return entry.secondsPlayed;
                } else if (value == "average") {
                    console.log(entry.averageSessionLength);
                    return entry.averageSessionLength;
                }

                throw `Unchecked value of value: '${value}'`;
            },

            makeGraph: function(): void {
                if (this.entries.state != "loaded") {
                    return console.warn(`Population> Cannot make graph, entries is '${this.entries.state}', not 'loaded'`);
                }

                // Get the distinct timestamps used
                const times: number[] = this.entries.data.map(iter => iter.timestamp.getTime());
                const timestamps: string[] = this.entries.data.map(iter => iter.timestamp)
                    .filter((iter, index, arr) => times.indexOf(iter.getTime()) == index)
                    .sort((a, b) => a.getTime() - b.getTime())
                    .map(iter => moment(iter).format("YYYY-MM-DD hh:mmA"));

                console.log(`${timestamps.length} labels`);

                // Combine the world ID and faction ID into a single int32, where the world ID is in the upper 16 bits
                const keys: number[] = this.entries.data.map(iter => (iter.worldID << 16) | (iter.factionID))
                    .filter((iter, index, arr) => arr.indexOf(iter) == index)
                    .sort((a, b) => a - b);

                const entries: PopulationEntry[] = this.entries.data;
                let colors: string[] = ColorUtils.randomColors(Math.random(), keys.length * this.shownValues.length);

                // If we are only showing 1 world, or all worlds combined, we can show faction colors
                if (this.queryWorlds.length <= 1 && this.shownValues.length == 1) {
                    colors = [];
                    if (this.factions.all == true) { colors.push("#ffd700"); } // gold for all factions
                    if (this.factions.vs == true) { colors.push(ColorUtils.VS); }
                    if (this.factions.nc == true) { colors.push(ColorUtils.NC); }
                    if (this.factions.tr == true) { colors.push(ColorUtils.TR); }
                    if (this.factions.ns == true) { colors.push(ColorUtils.NS); }
                }

                console.log(`Have ${keys.length} keys`);

                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                this.chart = hs.stockChart("hc-chart", {
                    title: { text: "" },

                    chart: {
                        zooming: {
                            type: "x"
                        },
                        backgroundColor: "#222222",
                    },

                    legend: {
                        itemStyle: { color: "#ffffff" }
                    },

                    xAxis: {
                        labels: {
                            formatter: (v) => {
                                const d: Date = new Date(v.value);
                                return moment(d).format("YYYY-MM-DD HH:mmA");
                            },
                            style: { color: "#ffffff" }
                        },
                        crosshair: true
                    },

                    yAxis: [
                        createYAxis("Total sessions", this.show.total),
                        createYAxis("Session starts", this.show.logins),
                        createYAxis("Session finishes", this.show.logouts),
                        createYAxis("Unique characters", this.show.unique),
                        createYAxis("Total playtime", this.show.length),
                        createYAxis("Average session length", this.show.average)
                    ],

                    tooltip: {
                        split: true,
                        headerFormat: "",
                        pointFormatter: function(): string {
                            const d: Date = new Date(this.x);

                            let ret: string = "<b>" + this.series.name + "</b><br/>";
                            ret += `${moment(d).format("YYYY-MM-DD HH:mmA")} ${TimeUtils.getTimezoneName()}<br/>`;

                            const name: string = this.series.name;
                            if (name.indexOf("length") > -1 || name.indexOf("average") > -1) {
                                ret += `${TimeUtils.duration(this.y || 0)}</br>`;
                            } else {
                                ret += `${this.y}<br/>`;
                            }

                            return ret;
                        }
                    },

                    series: keys.map((key: number, index: number) => {
                        const worldID: number = (key & 0xFFFF0000) >> 16;
                        const factionID: number = key & 0xFFFF;

                        return this.generateSeries(entries, colors, index, this.shownValues, worldID, factionID);
                    }).reduce((acc, iter) => {
                        acc.push(...iter);
                        return acc;
                    }, []),

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
            },

            generateSeries: function(entries: PopulationEntry[], colors: string[], index: number, values: string[], worldID: number, factionID: number): hc.SeriesOptionsType[] {
                const arr: hc.SeriesOptionsType[] = [];

                console.log(`World = ${worldID}, Faction = ${factionID}`);

                const data: PopulationEntry[] = entries.filter(iter => {
                    return iter.worldID == worldID && iter.factionID == factionID;
                }).sort((a, b) => {
                    return a.timestamp.getTime() - b.timestamp.getTime();
                });

                const worldName: string = worldID == 0 ? "All servers" : WorldUtils.getWorldID(worldID);
                const factionName: string = factionID == 0 ? "All factions" : FactionUtils.getName(factionID);

                for (let i = 0; i < values.length; ++i) {
                    const value = values[i];

                    const yaxisIndex: number | undefined = ValueToYAxis.get(value);
                    if (yaxisIndex == undefined) {
                        throw `Missing y axis of ${value}`;
                    }

                    const options: hc.SeriesOptionsType = {
                        type: "line",
                        name: `${worldName} - ${factionName} - ${value}`,
                        data: data.map(iter => {
                            return {
                                x: iter.timestamp.getTime(),
                                y: this.getDataPoint(value, iter)
                            };
                        }),
                        color: colors[(index * values.length) + i],
                        turboThreshold: 1000000,
                        yAxis: yaxisIndex
                    };

                    console.log(`Options for ${value} is on ${yaxisIndex}`);

                    arr.push(options);
                }

                return arr;
            }
        },

        computed: {
            queryWorlds: function(): number[] {
                const w: number[] = [];

                if (this.worlds.all == true) { w.push(0); } // 0 for aggregate of all
                if (this.worlds.osprey == true) { w.push(WorldUtils.Osprey); }
                if (this.worlds.wainwright == true) { w.push(WorldUtils.Wainwright); }
                if (this.worlds.cobalt == true) { w.push(WorldUtils.Cobalt); }
                if (this.worlds.emerald == true) { w.push(WorldUtils.Emerald); }
                if (this.worlds.jaeger == true) { w.push(WorldUtils.Jaeger); }
                if (this.worlds.soltech == true) { w.push(WorldUtils.SolTech); }

                return w;
            },

            queryFactions: function(): number[] {
                const f: number[] = [];

                if (this.factions.all == true) { f.push(0); } // 0 for aggregate of all
                if (this.factions.vs == true) { f.push(1); }
                if (this.factions.nc == true) { f.push(2); }
                if (this.factions.tr == true) { f.push(3); }
                if (this.factions.ns == true) { f.push(4); }

                return f;
            },

            shownValues: function(): string[] {
                const values: string[] = [];

                if (this.show.total == true) { values.push("total"); }
                if (this.show.logins == true) { values.push("logins"); }
                if (this.show.logouts == true) { values.push("logouts"); }
                if (this.show.unique == true) { values.push("unique"); }
                if (this.show.length == true) { values.push("length"); }
                if (this.show.average == true) { values.push("average"); }

                return values;
            }

        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            InfoHover,
            ToggleButton,
            Busy,
            DateTimeInput,
            Collapsible
        }
    });
    export default Population;
</script>