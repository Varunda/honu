<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/population">Population</a>
            </li>
        </honu-menu>

        <div>

            <h4>Date range</h4>
            <div class="d-flex mb-2">
                <div class="w-100">
                    <label>Start:</label>
                    <date-time-input v-model="range.start" class="form-control"></date-time-input>
                </div>

                <div class="w-100">
                    <label>End:</label>
                    <date-time-input v-model="range.end" class="form-control"></date-time-input>
                </div>
            </div>

            <h4>Servers</h4>
            <div class="d-flex mb-2">
                <toggle-button class="w-100" v-model="worlds.connery">
                    Connery
                </toggle-button>

                <toggle-button class="w-100" v-model="worlds.miller">
                    Miller
                </toggle-button>

                <toggle-button class="w-100" v-model="worlds.cobalt">
                    Cobalt
                </toggle-button>

                <toggle-button class="w-100" v-model="worlds.emerald">
                    Emerald
                </toggle-button>

                <toggle-button class="w-100" v-model="worlds.soltech">
                    SolTech
                </toggle-button>

                <toggle-button class="w-100" v-model="worlds.jaeger">
                    Jaeger
                </toggle-button>
            </div>

            <h4>Factions</h4>
            <div class="d-flex mb-2">
                <toggle-button class="w-100" v-model="factions.vs">
                    VS
                </toggle-button>

                <toggle-button class="w-100" v-model="factions.nc">
                    NC
                </toggle-button>

                <toggle-button class="w-100" v-model="factions.tr">
                    TR
                </toggle-button>

                <toggle-button class="w-100" v-model="factions.nc">
                    NS
                </toggle-button>
            </div>

            <button class="btn btn-success w-100" @click="loadEntries">
                Load
            </button>

            <hr/>

        </div>

        <div>

            <div v-if="entries.state == 'idle'">
                No data loaded
            </div>

            <div v-else-if="entries.state == 'loading'">
                <busy class="honu-busy"></busy>
            </div>

            <div v-else-if="entries.state == 'loaded'">
                Loaded {{entries.data.length}} data points
                <canvas id="chart-population" style="height: 80vh; max-height: 80vh;" class="mb-2 w-100"></canvas>
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

    import Chart, { LegendItem } from "chart.js/auto/auto.esm";
    import * as moment from "moment";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";
    import Busy from "components/Busy.vue";
    import DateTimePicker from "components/DateTimePicker.vue";
    import DateTimeInput from "components/DateTimeInput.vue";

    import { PopulationEntry, PopulationApi } from "api/PopulationApi";

    import WorldUtils from "util/World";
    import ColorUtils from "util/Color";
import LoadoutUtils from "../../util/Loadout";
import FactionUtils from "../../util/Faction";

    export const Population = Vue.extend({
        props: {

        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<PopulationEntry[]>,
                graph: null as Chart | null,

                range: {
                    start: new Date() as Date,
                    end: new Date() as Date
                },

                worlds: {
                    connery: false as boolean,
                    miller: false as boolean,
                    cobalt: false as boolean,
                    emerald: false as boolean,
                    soltech: false as boolean,
                    jaeger: false as boolean
                },

                factions: {
                    vs: false as boolean,
                    nc: false as boolean,
                    tr: false as boolean,
                    ns: false as boolean
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

            makeGraph: function(): void {
                if (this.entries.state != "loaded") {
                    return console.warn(`Population> Cannot make graph, entries is '${this.entries.state}', not 'loaded'`);
                }

                const elem: HTMLElement | null = document.getElementById("chart-population");
                if (elem == null) {
                    return console.error(`Population> canvas element is null, did you nextTick the make graph?`);
                }

                const context: CanvasRenderingContext2D | null = (elem as HTMLCanvasElement).getContext("2d");
                if (context == null) {
                    return console.error(`Population> 2d context is null?`);
                }

                if (this.graph != null) {
                    this.graph.destroy();
                    this.graph = null;
                }

                const times: number[] = this.entries.data.map(iter => iter.timestamp.getTime());

                const timestamps: string[] = this.entries.data.map(iter => iter.timestamp)
                    .filter((iter, index, arr) => times.indexOf(iter.getTime()) == index)
                    .sort((a, b) => a.getTime() - b.getTime())
                    .map(iter => moment(iter).format("YYYY-MM-DD hh:mmA"));

                console.log(`${timestamps.length} labels`);

                // Combine the world ID and faction ID into a single int32, where the world ID is in the upper 16 bits
                const keys: number[] = this.entries.data.map(iter => (iter.worldID << 16) | (iter.factionID))
                    .filter((iter, index, arr) => arr.indexOf(iter) == index);

                const entries: PopulationEntry[] = this.entries.data;
                const colors: string[] = ColorUtils.randomColors(Math.random(), keys.length);

                console.log(`Have ${keys.length} keys`);

                this.graph = new Chart(context, {
                    type: "line",
                    data: {
                        labels: timestamps,
                        datasets: [
                            ...keys.map((key: number, index: number) => {
                                const worldID: number = (key & 0xFFFF0000) >> 16;
                                const factionID: number = key & 0xFFFF;

                                console.log(`World = ${worldID}, Faction = ${factionID}`);

                                const data: PopulationEntry[] = entries.filter(iter => {
                                    return iter.worldID == worldID && iter.factionID == factionID;
                                });

                                const worldName: string = worldID == 0 ? "All" : WorldUtils.getWorldID(worldID);
                                const factionName: string = factionID == 0 ? "All" : FactionUtils.getName(factionID);

                                return {
                                    data: data.map(iter => iter.logins),
                                    label: `${worldName} - ${factionName}`,
                                    borderColor: colors[index]
                                };
                            })
                        ]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        scales: {
                            x: {
                                ticks: {
                                    color: "#ffffff",
                                    display: false
                                },
                                grid: {
                                    color: "#777777",
                                    display: false
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
                });

            }
        },

        computed: {
            queryWorlds: function(): number[] {
                const w: number[] = [];

                if (this.worlds.connery == true) { w.push(WorldUtils.Connery); }
                if (this.worlds.miller == true) { w.push(WorldUtils.Miller); }
                if (this.worlds.cobalt == true) { w.push(WorldUtils.Cobalt); }
                if (this.worlds.emerald == true) { w.push(WorldUtils.Emerald); }
                if (this.worlds.jaeger == true) { w.push(WorldUtils.Jaeger); }
                if (this.worlds.soltech == true) { w.push(WorldUtils.SolTech); }

                return w;
            },

            queryFactions: function(): number[] {
                const f: number[] = [];

                if (this.factions.vs == true) { f.push(1); }
                if (this.factions.nc == true) { f.push(2); }
                if (this.factions.tr == true) { f.push(3); }
                if (this.factions.ns == true) { f.push(4); }

                return f;
            }

        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            InfoHover,
            ToggleButton,
            Busy,
            DateTimePicker, DateTimeInput
        }
    });
    export default Population;
</script>