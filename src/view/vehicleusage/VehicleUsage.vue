<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/vehicleusage">Vehicle usage</a>
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
                    <h4>Servers</h4>
                    <toggle-button class="w-100" v-model="worlds.all" false-color="btn-secondary">
                        All servers
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.connery" false-color="btn-secondary">
                        Connery
                    </toggle-button>

                    <toggle-button class="w-100" v-model="worlds.miller" false-color="btn-secondary">
                        Miller
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
                    <!--
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
                    -->
                </div>
            </div>

            <button class="btn btn-primary w-100" @click="loadData">
                Load
            </button>
        </collapsible>

        <hr class="border" />

        <div>
            <div v-if="usage.state == 'idle'">
                No data loaded
            </div>

            <div v-else-if="usage.state == 'loading'">
                <busy class="honu-busy"></busy>
            </div>

            <div v-else-if="usage.state == 'loaded'">
                Loaded {{usage.data.length}} data points

                <div id="hc-chart" style="height: 80vh; max-height: 80vh;" class="mb-2 w-100"></div>
            </div>

            <div v-else-if="usage.state == 'error'">
                Error loading data:
                <br />
                {{usage.message}}
            </div>

        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "filters/WorldNameFilter";

    import WorldUtils from "util/World";
    import ColorUtils from "util/Color";
    import FactionUtils from "util/Faction";
    import TimeUtils from "util/Time";

    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import Collapsible from "components/Collapsible.vue";
    import ToggleButton from "components/ToggleButton";
    import DateTimeInput from "components/DateTimeInput.vue";

    import { VehicleUsageData, VehicleDataApi, VehicleUsageEntry } from "api/VehicleUsageApi";

    import * as moment from "moment";

    import * as hc from "highcharts";
    import * as hs from "highcharts/highstock";
    import "highcharts/modules/annotations";

    class FlattenedEntry {
        public worldID: number = 0;
        public zoneID: number = 0;
        public factionID: number = 0;
        public vehicleID: number = 0;

        public timestamp: Date = new Date();

        public count: number = 0;
    }

    class FlatKey {
        public worldID: number = 0;
        public factionID: number = 0;
        public vehicle: number = 0;
    }

    export const VehicleUsage = Vue.extend({
        props: {

        },

        data: function() {
            return {
                range: {
                    start: new Date() as Date,
                    end: new Date() as Date
                },

                worlds: {
                    all: true as boolean,
                    connery: false as boolean,
                    miller: false as boolean,
                    cobalt: false as boolean,
                    emerald: false as boolean,
                    soltech: false as boolean,
                    jaeger: false as boolean
                },

                factions: {
                    all: true as boolean,
                    vs: true as boolean,
                    nc: true as boolean,
                    tr: true as boolean,
                    ns: true as boolean
                },

                chart: null as hc.StockChart | null,
                usage: Loadable.idle() as Loading<VehicleUsageData[]>
            }
        },

        created: function(): void {
            document.title = `Honu / Vehicle usage`;
        },

        beforeMount: function(): void {
            this.loadData();
        },

        methods: {
            loadData: async function(): Promise<void> {
                const start: Date = moment(new Date()).subtract(1, 'week').toDate();
                const end: Date = new Date();

                this.usage = Loadable.loading();
                this.usage = await VehicleDataApi.getHistory(start, end);

                this.$nextTick(() => {
                    this.makeGraph();
                });

            },

            makeGraph: function(): void {
                if (this.usage.state != "loaded") {
                    return console.warn(`Population> Cannot make graph, usage is '${this.usage.state}', not 'loaded'`);
                }

                // Get the distinct timestamps used
                const times: number[] = this.usage.data.map(iter => iter.timestamp.getTime());
                const timestamps: string[] = this.usage.data.map(iter => iter.timestamp)
                    .filter((iter, index, arr) => times.indexOf(iter.getTime()) == index)
                    .sort((a, b) => a.getTime() - b.getTime())
                    .map(iter => moment(iter).format("YYYY-MM-DD hh:mmA"));

                console.log(`${timestamps.length} labels`);

                let data: FlattenedEntry[] = [];
                for (const datum of this.usage.data) {
                    if (datum.worldID != WorldUtils.Emerald) {
                        continue;
                    }

                    if (this.factions.vs) {
                        datum.vs.usage.forEach((value: VehicleUsageEntry, key: number) => {
                            data.push({
                                worldID: datum.worldID,
                                zoneID: datum.zoneID,
                                factionID: FactionUtils.VS,
                                vehicleID: value.vehicleID,
                                timestamp: datum.timestamp,
                                count: value.count
                            });
                        });
                    }

                    if (this.factions.nc) {
                        datum.nc.usage.forEach((value: VehicleUsageEntry, key: number) => {
                            data.push({
                                worldID: datum.worldID,
                                zoneID: datum.zoneID,
                                factionID: FactionUtils.NC,
                                vehicleID: value.vehicleID,
                                timestamp: datum.timestamp,
                                count: value.count
                            });
                        });
                    }

                    if (this.factions.tr) {
                        datum.tr.usage.forEach((value: VehicleUsageEntry, key: number) => {
                            data.push({
                                worldID: datum.worldID,
                                zoneID: datum.zoneID,
                                factionID: FactionUtils.TR,
                                vehicleID: value.vehicleID,
                                timestamp: datum.timestamp,
                                count: value.count
                            });
                        });
                    }
                }

                console.log(data.length);

                const keys: Set<string> = new Set(data.map(iter => {
                    return `${iter.worldID}-${iter.factionID}-${iter.vehicleID}`;
                }));

                const buckets: Map<string, FlattenedEntry[]> = new Map();
                for (const datum of data) {
                    if (datum.vehicleID == 0) {
                        continue;
                    }

                    const key: string = `${datum.worldID}-${datum.factionID}-${datum.vehicleID}`;

                    if (buckets.has(key) == false) {
                        buckets.set(key, []);
                    }

                    buckets.get(key)!.push(datum);
                }

                console.log(`showing ${keys.size} lines in ${buckets.size} buckets`);

                const usage: VehicleUsageData[] = this.usage.data;
                let colors: string[] = ColorUtils.randomColors(Math.random(), keys.size);

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

                    series: Array.from(buckets.entries()).map((iter, index) => {
                        const key: string = iter[0];
                        const value: FlattenedEntry[] = iter[1].sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                        const options: hc.SeriesOptionsType = {
                            type: "spline",
                            name: `${key}`,
                            marker: {
                                enabled: true,
                                radius: 4
                            },
                            data: value.map(i => {
                                return {
                                    x: i.timestamp.getTime(),
                                    y: i.count
                                };
                            }),
                            color: colors[index],
                            turboThreshold: 1000000,
                        };

                        return options;
                    })
                });
            }
        },

        computed: {

        },

        components: {
            InfoHover, DateTimeInput, Busy, Collapsible, ToggleButton,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default VehicleUsage;
</script>
