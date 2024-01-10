<template>
    <div>
        <collapsible header-text="Weapons">
            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--green)">
                    Weapons
                </h3>

                <a-table :entries="weaponData"
                         :paginate="true" 
                         :show-filters="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="kills" default-sort-order="desc"
                         class="border-top-0"
                >

                    <a-col sort-field="name">
                        <a-header>
                            Weapon
                        </a-header>

                        <a-filter field="name" type="string" method="input"
                                  :conditions="[ 'contains', 'equals' ]">
                        </a-filter>

                        <a-body v-slot="entry">
                            <a v-if="entry.id != 0" :href="'/i/' + entry.id">
                                {{entry.name}}
                            </a>
                            <span v-else>
                                {{entry.name}}
                            </span>
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            Infantry kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshots">
                        <a-header>
                            Headshots
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshots | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="headshotRatio">
                        <a-header>
                            HSR%
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.headshotRatio | locale(2)}}%
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKills">
                        <a-header>
                            Hip kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="hipKillRatio">
                        <a-header>
                            Hip%
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hipKillRatio | locale(2)}}%
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKills">
                        <a-header>
                            ADS kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="adsKillRatio">
                        <a-header>
                            ADS%
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.adsKillRatio | locale(2)}}%
                        </a-body>
                    </a-col>

                    <a-col sort-field="vehicleKills">
                        <a-header>
                            Vehicle kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.vehicleKills | locale}}
                        </a-body>
                    </a-col>

                </a-table>

                <h3 class="wt-header mb-2 border-0" style="background-color: var(--green)">
                    Weapon usage per week
                    <info-hover text="only weapons that are >=5% of total kills or at least 1'160 kills included"></info-hover>
                </h3>

                <div>
                    <canvas id="chart-weapons-over-time" style="max-height: 500px; height: 500px;">
                    </canvas>
                </div>

            </div>

        </collapsible>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";
    import Chart from "chart.js/auto/auto.esm";
    import * as moment from "moment";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // models
    import { PsItem } from "api/ItemApi";
    import { WrappedWeaponStats } from "../common";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";

    // util
    import ColorUtils from "util/Color";

    class WrappedWeaponData {
        public id: number = 0;
        public item: PsItem | null = null;
        public name: string = "";

        public kills: number = 0;
        public vehicleKills: number = 0;
        public headshots: number = 0;
        public headshotRatio: number = 0;

        public adsKills: number = 0;
        public adsKillRatio: number = 0;
        public hipKills: number = 0;
        public hipKillRatio: number = 0;
    }

    export const WrappedViewWeapons = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                weaponData: Loadable.idle() as Loading<WrappedWeaponData[]>,

                filter: {
                    infil: true as boolean,
                    lightAssault: true as boolean,
                    medic: true as boolean,
                    engi: true as boolean,
                    heavy: true as boolean,
                    max: true as boolean
                },

                chart: {
                    instance: null as Chart | null
                }
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeAll();
            });
        },

        methods: {
            makeAll: function(): void {
                this.makeWeaponData();
                this.makeChart();
            },

            makeChart: function(): void {
                const mostUsed: WrappedWeaponStats[] = [...this.wrapped.extra.weaponStats].filter(iter => {
                    return (iter.kills >= (this.wrapped.kills.length / 20)) || (iter.kills >= 1160);
                }).sort((a, b) => {
                    return b.kills - a.kills;
                }).slice(0, 15);

                if (this.chart.instance != null) {
                    this.chart.instance.destroy(); 
                    this.chart.instance = null;
                }

                const canvas = document.getElementById("chart-weapons-over-time") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-weapons-over-time is null`);
                    return;
                }

                const killsPerWeek: Map<number, number[]> = new Map();
                mostUsed.forEach((iter) => {
                    killsPerWeek.set(iter.itemId, [...Array(12)].map(iter => 0));
                });

                for (const ev of this.wrapped.kills) {
                    if (killsPerWeek.has(ev.weaponID) == false) {
                        continue;
                    }

                    const month: number = moment(ev.timestamp).get("month");
                    killsPerWeek.get(ev.weaponID)![month] += 1;
                }

                this.chart.instance = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: [...Array(12)].map((iter, index) => `month ${index + 1}`),
                        datasets: mostUsed.map((iter, index) => {
                            return {
                                label: (iter.itemId == 0) ? "no weapon" : iter.name,
                                data: killsPerWeek.get(iter.itemId) ?? [],
                                borderColor: ColorUtils.randomColor(0.5, mostUsed.length, index),
                                cubicInterpolationMode: "monotone"
                            };
                        }),
                    },
                    options: {
                        scales: {
                            x: {
                                ticks: {
                                    color: "white",
                                    font: {
                                        family: "Consolas"
                                    }
                                }
                            }
                        },
                        plugins: {
                            tooltip: {
                                mode: "index",
                                intersect: false
                            },
                        },
                        hover: {
                            mode: "index",
                            intersect: false
                        }
                    }

                });
            },

            makeWeaponData: function(): void {
                const map: Map<number, WrappedWeaponData> = new Map();

                for (const ev of this.wrapped.kills) {
                    let data: WrappedWeaponData | undefined = map.get(ev.weaponID);

                    if (data == undefined) {
                        data = new WrappedWeaponData();
                        data.id = ev.weaponID;
                        data.item = this.wrapped.items.get(ev.weaponID) ?? null;

                        if (ev.weaponID == 0) {
                            data.name = "no weapon";
                        } else {
                            data.name = data.item?.name ?? `<missing ${ev.weaponID}>`;
                        }
                    }

                    ++data.kills;

                    if (ev.isHeadshot == true) {
                        ++data.headshots;
                    }

                    const fireModeIndex: number | null = WrappedEntry.getFireModeIndex(this.wrapped, ev.attackerFireModeID);
                    if (fireModeIndex == 0) {
                        ++data.hipKills;
                    } else if (fireModeIndex == 1) {
                        ++data.adsKills;
                    }

                    map.set(ev.weaponID, data);
                }

                for (const ev of this.wrapped.vehicleKill) {
                    let data: WrappedWeaponData | undefined = map.get(ev.attackerWeaponID);

                    if (data == undefined) {
                        data = new WrappedWeaponData();
                        data.id = ev.attackerWeaponID;
                        data.item = this.wrapped.items.get(ev.attackerWeaponID) ?? null;

                        if (ev.attackerWeaponID == 0) {
                            data.name = "no weapon";
                        } else {
                            data.name = data.item?.name ?? `<missing ${ev.attackerWeaponID}>`;
                        }
                    }

                    ++data.vehicleKills;
                    map.set(ev.attackerWeaponID, data);
                }

                const arr: WrappedWeaponData[] = Array.from(map.values()).map((iter: WrappedWeaponData) => {
                    iter.headshotRatio = iter.headshots / Math.max(1, iter.kills) * 100;
                    iter.adsKillRatio = iter.adsKills / Math.max(1, iter.kills) * 100;
                    iter.hipKillRatio = iter.hipKills / Math.max(1, iter.kills) * 100;
                    return iter;
                });

                this.weaponData = Loadable.loaded(Array.from(map.values()));
            }
        },

        components: {
            Collapsible,
            InfoHover,
            ToggleButton,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewWeapons;
</script>