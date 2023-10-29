<template>
    <div class="w-100">
        <div style="max-width: 1600px; margin: auto">

            <h1 class="text-center mb-4">
                {{wrapped.timestamp.getFullYear() - 1}} 
                <span @click="showDebug = !showDebug">
                    wrapped
                </span>
                for

                <a :href="'/c/' + inputCharacters[0].id">
                    {{inputCharacters[0] | characterName}}
                    <span v-if="showDebug">
                        {{inputCharacters[0].id}}
                    </span>
                </a>

                <span v-if="inputCharacters.length > 1" data-target="#full-char-list" data-toggle="collapse" class="wt-link">
                    (+{{inputCharacters.length - 1}} more)
                </span>

                <br />

                <span class="collapse" id="full-char-list">
                    <span v-for="(char, i) in inputCharacters">
                        <a v-if="i > 0" :href="'/c/' + char.id" class="mr-2">{{char | characterName}}<span v-if="showDebug">{{char.id}}</span></a>
                    </span>
                </span>
            </h1>

            <collapsible header-text="General" class="text-center">
                <div style="display: grid; grid-template-columns: 1fr 1fr 1fr;" class="align-content-center">
                    <div style="grid-area: 1 / 1 / 1 / 2" class="cell cell-center">
                        <h1>
                            {{Math.round(wrapped.extra.totalPlaytime / 60 / 60) | locale(0)}}
                        </h1>

                        <h5>
                            Hours played
                        </h5>
                    </div>

                    <div style="grid-area: 1 / 2 / 1 / 4" class="cell cell-center w-100">
                        <canvas id="chart-time-per-class" class="w-100" style="max-height: 300px;"></canvas>
                        <h5>
                            Time per class
                        </h5>
                    </div>

                    <div style="grid-area: 2 / 1 / 2 / 3" class="cell cell-center w-100">
                        <canvas id="chart-time-per-zone" class="w-100" style="max-height: 300px;"></canvas>
                        <h5>
                            Continents played on
                        </h5>
                    </div>

                    <div style="grid-area: 2 / 3 / 2 / 3" class="cell cell-center">
                        <h1>
                            {{wrapped.kills.length | locale}}
                        </h1>

                        <h5>
                            Kills
                        </h5>
                    </div>
                </div>
            </collapsible>

            <collapsible header-text="Infantry" class="text-center">
                <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr 1fr;" class="align-content-center">
                    <!-- row 1 -->
                    <div style="grid-area: 1 / 1 / 1 / 3" class="cell">
                        <h1>
                            {{mostPlayedClass.name}}
                        </h1>

                        <h5>
                            {{mostPlayedClass.timeAs / 1000 / 60 / 60 | locale(0)}} hours
                            / 
                            {{mostPlayedClass.kills | locale(0)}} kills
                        </h5>

                        <h6>
                            <strong>Favorite Class</strong>
                        </h6>
                    </div>

                    <div style="grid-area: 1 / 3 / 1 / 6" class="cell text-left">
                        <div class="d-flex">
                            <div class="flex-grow-1 mr-3" v-if="mostUsedInfantryWeapons.length >= 1">
                                <h3>{{mostUsedInfantryWeapons[0].name}}</h3>
                                <h5>{{mostUsedInfantryWeapons[0].kills | locale}}</h5>
                                <h6><strong>Top infantry guns</strong></h6>
                            </div>

                            <div class="flex-grow-1 mr-3" v-if="mostUsedInfantryWeapons.length >= 2">
                                <h3>{{mostUsedInfantryWeapons[1].name}}</h3>
                                <h5>{{mostUsedInfantryWeapons[1].kills | locale}}</h5>
                                <h6><strong>&nbsp;</strong></h6>
                            </div>

                            <div class="flex-grow-1" v-if="mostUsedInfantryWeapons.length >= 3">
                                <h3>{{mostUsedInfantryWeapons[2].name}}</h3>
                                <h5>{{mostUsedInfantryWeapons[2].kills | locale}}</h5>
                                <h6><strong>&nbsp;</strong></h6>
                            </div>
                        </div>
                    </div>

                    <!-- row 2 -->
                    <div style="grid-area: 2 / 1 / 2 / 5" class="cell">
                        <canvas id="chart-outfit-kills" class="w-100" style="max-height: 300px;"></canvas>
                        <h5>
                            Outfits killed
                        </h5>
                    </div>

                    <div style="grid-area: 2 / 5 / 2 / 6" class="cell cell-center">
                        <h6>
                            <strong>Highest K/D</strong>
                        </h6>

                        <h1>
                            {{highestKdOutfit.kills / Math.max(1, highestKdOutfit.deaths) | locale(2)}}
                        </h1>

                        <h5>
                            <a :href="'/o/' + highestKdOutfit.id">
                                {{highestKdOutfit.displayName}}
                            </a>
                        </h5>
                    </div>

                    <!-- session bests -->
                    <div v-for="(best, i) in bestSessions" :style="'grid-area: 3 / ' + (i + 1) + ' / 3 / ' + (i + 2)" class="cell">
                        <h6>
                            <strong>{{best.name}}</strong>
                        </h6>

                        <h1>
                            {{best.value}}
                        </h1>

                        <h5>
                            <a :href="'/s/' + best.session.session.id">
                                on {{best.session.start | moment}}
                            </a>
                        </h5>
                    </div>
                </div>
            </collapsible>

            <collapsible v-if="showVehicleStats" header-text="Vehicle" class="text-center">
                <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr 1fr;" class="align-content-center">
                    <!-- row 1 -->

                    <div style="grid-area: 1 / 1 / 1 / 4" class="cell text-center">
                        <canvas id="chart-vehicle-kills" class="w-100" style="max-height: 300px;"></canvas>
                        <h5>
                            Vehicles killed
                        </h5>
                    </div>

                    <div style="grid-area: 1 / 4 / 1 / 6" class="cell cell-center">
                        <h1>
                            {{mostUsedVehicles[0].vehicleName}}
                        </h1>

                        <h5>
                            {{mostUsedVehicles[0].killsAs | locale(0)}} vehicle kills
                        </h5>

                        <h6>
                            <strong>Most used vehicle</strong>
                        </h6>
                    </div>
                </div>
            </collapsible>
            <span v-if="showDebug">
                {{vehicleKillCount}} / {{wrapped.kills.length}}
            </span>

        </div>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";
    import Chart from "chart.js/auto/auto.esm";

    // util
    import ColorUtils from "util/Color";
    import TimeUtils from "util/Time";
    import LocaleUtil from "util/Locale";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/CharacterName";

    // models
    import { EntityFought, WrappedClassStats, WrappedSession, WrappedWeaponStats } from "../common";
    import { WrappedVehicleUsage } from "../data/vehicles";
    import { PsCharacter } from "api/CharacterApi";

    class BestSessionEntry {
        public name: string = "";
        public value: string = "";
        public session: WrappedSession = new WrappedSession();
    }

    export const WrappedViewHighlight = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                charts: {
                    timePerClass: null as Chart | null,
                    timePerZone: null as Chart | null,
                    outfitKills: null as Chart | null,
                    vehicleKills: null as Chart | null
                },

                showDebug: false as boolean,

                bestSessions: [] as BestSessionEntry[]

            }
        },

        mounted: function(): void {
            this.makeSessionBests();

            this.$nextTick(() => {
                this.makeCharts();
            });
        },

        methods: {

            makeSessionBests: function(): void {
                this.bestSessions = [];
                if (this.wrapped.extra.sessions.length == 0) {
                    return;
                }

                const sessions: WrappedSession[] = [...this.wrapped.extra.sessions];

                const make = (
                    name: string,
                    selector: (_: WrappedSession) => number,
                    filter?: ((_: WrappedSession) => boolean) | undefined,
                    localePrecision: number = 2
                ) => {

                    const b: WrappedSession | undefined = sessions.filter(iter => {
                        if (filter) {
                            return filter(iter);
                        }
                        return true;
                    }).sort((a, b) => {
                        return selector(b) - selector(a);
                    }).at(0);

                    if (b != undefined) {
                        this.bestSessions.push({
                            name: name,
                            value: LocaleUtil.locale(selector(b), localePrecision),
                            session: b
                        });
                    }
                }

                make("Highest KPM session",
                    (iter) => (iter.kills / (Math.max(1, iter.duration) / 60)),
                    (iter) => (iter.duration > 300)
                );

                const highestKd: WrappedSession | undefined = sessions.filter(iter => iter.duration > 300 && iter.deaths > 0 && iter.infantryKills > 25)
                    .sort((a, b) => (b.infantryKills / Math.max(1, b.deaths)) - (a.infantryKills / Math.max(1, a.deaths))).at(0);

                // only show if the highest KD is above 1, don't wanna embarrass someone lol
                if (highestKd != undefined && (highestKd.infantryKills / Math.max(1, highestKd.deaths)) > 1) {
                    this.bestSessions.push({
                        name: "Highest KD session",
                        value: LocaleUtil.locale(highestKd.infantryKills / Math.max(1, highestKd.deaths), 2),
                        session: highestKd
                    });
                }

                if (this.mostPlayedClass.icon == "medic") {
                    make("Most revives", (iter) => (iter.revives),
                        (iter) => (iter.revives > 0),
                        0
                    );

                    make("Most heals", (iter) => (iter.heals),
                        (iter) => (iter.heals > 0),
                        0
                    );

                    make("Most shield repairs", (iter) => iter.shieldRepairs,
                        (iter) => iter.shieldRepairs >= 100,
                        0
                    );
                } else if (this.mostPlayedClass.icon == "engi") {
                    make("Most resupplies", (iter) => iter.resupplies,
                        (iter) => iter.resupplies > 0,
                        0
                    );

                    make("Most MAX repairs", (iter) => iter.maxRepair,
                        (iter) => iter.maxRepair > 100,
                        0
                    );
                } else if (this.mostPlayedClass.icon == "heavy") {
                    make("Most assists", (iter) => iter.assists,
                        (iter) => iter.assists > 0,
                        0
                    );

                    make("Most MAX kills", (iter) => iter.maxKills,
                        (iter) => iter.maxKills > 0,
                        0
                    );

                }
            },

            makeCharts: function(): void {
                Chart.defaults.responsive = true;
                Chart.defaults.maintainAspectRatio = false;
                Chart.defaults.plugins.legend.display = false;
                Chart.defaults.scales.linear.ticks.color = "white";

                this.makeTimePerClassChart();
                this.makeTimePerZoneChart();
                this.makeOutfitsKilledGraph();
                this.makeVehiclesKillChart();
            },

            makeTimePerClassChart: function(): void {
                if (this.charts.timePerClass != null) {
                    this.charts.timePerClass.destroy();
                    this.charts.timePerClass = null;
                }

                const canvas = document.getElementById("chart-time-per-class") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-time-per-class is null`);
                    return;
                }

                this.charts.timePerClass = new Chart(ctx, {
                    type: "bar", 
                    data: {
                        labels: this.wrapped.extra.classStats.map(iter => iter.name),
                        datasets: [{
                            data: this.wrapped.extra.classStats.map(iter => iter.timeAs),
                            backgroundColor: ColorUtils.randomColors(0, this.wrapped.extra.classStats.length)
                        }]
                    },
                    options: {
                        plugins: {
                            tooltip: {
                                callbacks: {
                                    label: function(iter) {
                                        const v: number = iter.parsed.y / 1000 / 60 / 60;
                                        if (v < 1) {
                                            return `${LocaleUtil.locale(v * 60, 0)} minutes`;
                                        }

                                        return `${LocaleUtil.locale(v, 0)} hours`;
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                ticks: {
                                    callback: function(value, index, values) {
                                        if (typeof (value) == "string") {
                                            return value;
                                        }
                                        return `${LocaleUtil.locale(value / 1000 / 60 / 60, 0)} hours`;
                                    }
                                }
                            },
                            x: {
                                ticks: {
                                    color: "white"
                                }
                            }
                        }
                    }
                });
            },

            makeTimePerZoneChart: function(): void {
                if (this.charts.timePerZone != null) {
                    this.charts.timePerZone.destroy();
                    this.charts.timePerZone = null;
                }

                const canvas = document.getElementById("chart-time-per-zone") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-time-per-zone is null`);
                    return;
                }

                this.charts.timePerZone = new Chart(ctx, {
                    type: "bar", 
                    data: {
                        labels: this.wrapped.extra.zoneStats.map(iter => iter.name),
                        datasets: [{
                            data: this.wrapped.extra.zoneStats.map(iter => iter.timeMs),
                            backgroundColor: ColorUtils.randomColors(0.2, this.wrapped.extra.zoneStats.length)
                        }]
                    },
                    options: {
                        plugins: {
                            tooltip: {
                                callbacks: {
                                    label: function(iter) {
                                        const v: number = iter.parsed.y / 1000 / 60 / 60;
                                        if (v < 1) {
                                            return `${LocaleUtil.locale(v * 60, 0)} minutes`;
                                        }

                                        return `${LocaleUtil.locale(v, 0)} hours`;
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                ticks: {
                                    callback: function(value, index, values) {
                                        if (typeof (value) == "string") {
                                            return value;
                                        }
                                        return `${LocaleUtil.locale(value / 1000 / 60 / 60, 0)} hours`;
                                    }
                                }
                            },
                            x: {
                                ticks: {
                                    color: "white"
                                }
                            }
                        }
                    }
                });
            },

            makeOutfitsKilledGraph: function(): void {
                if (this.charts.outfitKills != null) {
                    this.charts.outfitKills.destroy();
                    this.charts.outfitKills = null;
                }

                const canvas = document.getElementById("chart-outfit-kills") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-outfit-kills is null`);
                    return;
                }

                const outfits: EntityFought[] = [...this.wrapped.extra.outfitFight].filter((iter) => {
                    // ignore the <no outfit> outfits
                    return iter.id.startsWith("0-") == false;
                }).sort((a, b) => {
                    return b.kills - a.kills;
                }).slice(0, 8);

                this.charts.outfitKills = new Chart(ctx, {
                    type: "bar", 
                    data: {
                        labels: outfits.map(iter => {
                            if (iter.displayName.indexOf("]") > -1) {
                                return iter.displayName.slice(0, iter.displayName.indexOf("]") + 1);
                            }
                            return iter.displayName;
                        }),
                        datasets: [{
                            data: outfits.map(iter => iter.kills),
                            backgroundColor: outfits.map(iter => {
                                if (iter.factionID == 1) {
                                    return ColorUtils.BG_VS;
                                } else if (iter.factionID == 2) {
                                    return ColorUtils.BG_NC;
                                } else if (iter.factionID == 3) {
                                    return ColorUtils.BG_TR;
                                }
                                return ColorUtils.BG_NS;
                            })
                        }]
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
                        }
                    }
                });
            },

            makeVehiclesKillChart: function(): void {
                if (this.charts.vehicleKills != null) {
                    this.charts.vehicleKills.destroy();
                    this.charts.vehicleKills = null;
                }

                const canvas = document.getElementById("chart-vehicle-kills") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-vehicle-kills is null`);
                    return;
                }

                const vehicles: WrappedVehicleUsage[] = [...this.wrapped.extra.vehicleUsage].sort((a, b) => {
                    return b.killed - a.killed;
                }).slice(0, 6);

                this.charts.vehicleKills = new Chart(ctx, {
                    type: "bar", 
                    data: {
                        labels: vehicles.map(iter => {
                            return iter.vehicleName.split(" ");
                        }),
                        datasets: [{
                            data: vehicles.map(iter => iter.killed),
                            backgroundColor: ColorUtils.randomColors(0.25, vehicles.length)
                        }]
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
                        }
                    }
                });

            }

        },

        computed: {
            mostPlayedClass: function(): WrappedClassStats {
                return [...this.wrapped.extra.classStats].sort((a, b) => {
                    return b.timeAs - a.timeAs;
                })[0];
            },

            mostUsedInfantryWeapons: function(): WrappedWeaponStats[] {
                return [...this.wrapped.extra.weaponStats].filter((iter) => {
                    return iter.item != null && iter.item.isVehicleWeapon == false;
                }).sort((a, b) => {
                    return b.kills - a.kills;
                }).slice(0, 3);
            },

            mostUsedVehicleWeapons: function(): WrappedWeaponStats[] {
                return [...this.wrapped.extra.weaponStats].filter((iter) => {
                    return iter.item != null
                        && iter.item.isVehicleWeapon == true
                        && iter.item.categoryID != 102; // infantry weapons (MANA turrets)
                }).sort((a, b) => {
                    return b.kills - a.kills;
                }).slice(0, 3);
            },

            mostUsedVehicles: function(): WrappedVehicleUsage[] {
                return [...this.wrapped.extra.vehicleUsage].sort((a, b) => {
                    return b.killsAs - a.killsAs;
                }).slice(0, 3);
            },

            highestKpmSession: function(): WrappedSession {
                return [...this.wrapped.extra.sessions].filter(iter => {
                    return iter.duration > 300;
                }).sort((a, b) => {
                    return (b.kills / b.duration) - (a.kills / a.duration);
                })[0];
            },

            highestKdOutfit: function(): EntityFought {
                return [...this.wrapped.extra.outfitFight].filter((iter) => {
                    // ignore <no outfit> outfits
                    return iter.id.startsWith("0-") == false && iter.characters.size > 10;
                }).sort((a, b) => {
                    return (b.kills + b.deaths) - (a.kills + a.deaths)
                }).slice(0, 15).sort((a, b) => {
                    return (b.kills / Math.max(1, b.deaths)) - (a.kills / (Math.max(1, a.deaths)));
                })[0];
            },

            inputCharacters: function(): PsCharacter[] {
                const arr: (PsCharacter | undefined)[] = this.wrapped.inputCharacterIDs.map((iter: string) => {
                    return this.wrapped.characters.get(iter);
                });

                return arr.filter(iter => iter != undefined) as PsCharacter[];
            },

            vehicleKillCount: function(): number {
                return this.wrapped.extra.vehicleUsage.reduce((acc, iter) => {
                    return acc + iter.killsAs;
                }, 0);
            },

            showVehicleStats: function(): boolean {
                //return (this.vehicleKillCount / this.wrapped.kills.length) > 0.10;

                return true;
            }

        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewHighlight;
</script>