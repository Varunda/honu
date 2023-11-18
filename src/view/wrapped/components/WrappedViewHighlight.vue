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

            <wrapped-view-quick-infantry :wrapped="wrapped" :show-debug="showDebug">
            </wrapped-view-quick-infantry>

            <wrapped-view-quick-vehicle v-if="showVehicleStats" :wrapped="wrapped" :show-debug="showDebug">
            </wrapped-view-quick-vehicle>
        </div>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";

    // chart stuff
    import Chart from "chart.js/auto/auto.esm";
    Chart.defaults.responsive = true;
    Chart.defaults.maintainAspectRatio = false;
    Chart.defaults.plugins.legend.display = false;
    Chart.defaults.scales.linear.ticks.color = "white";

    // util
    import ColorUtils from "util/Color";
    import LocaleUtil from "util/Locale";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";
    import CensusImage from "components/CensusImage";

    import WrappedViewQuickInfantry from "./quick/WrappedViewQuickInfantry.vue";
    import WrappedViewQuickVehicle from "./quick/WrappedViewQuickVehicle.vue";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/CharacterName";

    // models
    import { WrappedClassStats, WrappedSession, WrappedWeaponStats } from "../common";
    import { PsCharacter } from "api/CharacterApi";

    const WrappedItem = Vue.extend({
        props: {
            entry: { type: Object as PropType<WrappedWeaponStats>, required: true },
            IsVehicle: { type: Boolean, required: false, default: false }
        },

        template: `
            <div style="position: relative;">
                <census-image v-if="entry.item != null && entry.item.imageID && entry.item.imageID != 0" :image-id="entry.item.imageID"
                    style="text-align: center; height: 100%; max-height: 300px;" class="mr-1">
                </census-image>
                <div style="width: 100%; display: inline-block;" >
                    <h3>{{entry.name}}</h3>
                    <h5 v-if="IsVehicle == false">
                        {{entry.kills | locale}}
                    </h5>
                    <h5 v-else>
                        {{entry.vehicleKills | locale}}
                    </h5>
                </div>
            </div>
        `,

        components: {
            CensusImage
        }
    });

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
                    vehicleKills: null as Chart | null
                },

                showDebug: false as boolean,
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeCharts();
            });
        },

        methods: {

            /**
             * Using a list of wrapped sessions, select the one with the highest value given a selector func
             * 
             * @param sessions Sessions to find the best of
             * @param target What list of best to put it into
             * @param name Name of the best
             * @param selector Selector function of the sessions to get the value of 
             * @param filter Optional function filter on the sessions
             * @param localePrecision How many decimals of precision are included, defaults to 2
             */
            makeSessionBest: function(
                sessions: WrappedSession[],
                target: BestSessionEntry[],
                name: string,
                selector: (_: WrappedSession) => number,
                filter?: ((_: WrappedSession) => boolean) | undefined,
                localePrecision: number = 2
            ) {

                const b: WrappedSession | undefined = sessions.filter(iter => {
                    if (filter) {
                        return filter(iter);
                    }
                    return true;
                }).sort((a, b) => {
                    return selector(b) - selector(a);
                }).at(0);

                if (b != undefined) {
                    target.push({
                        name: name,
                        value: LocaleUtil.locale(selector(b), localePrecision),
                        session: b
                    });
                }
            },

            makeCharts: function(): void {
                Chart.defaults.responsive = true;
                Chart.defaults.maintainAspectRatio = false;
                Chart.defaults.plugins.legend.display = false;
                Chart.defaults.scales.linear.ticks.color = "white";

                this.makeTimePerClassChart();
                this.makeTimePerZoneChart();
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
        },

        computed: {
            mostPlayedClass: function(): WrappedClassStats {
                return [...this.wrapped.extra.classStats].sort((a, b) => {
                    return b.timeAs - a.timeAs;
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
            WrappedItem,
            CensusImage,
            WrappedViewQuickInfantry, WrappedViewQuickVehicle
        }

    });
    export default WrappedViewHighlight;
</script>