<template>
    <collapsible v-if="showVehicleStats" header-text="Vehicle" class="text-center">
        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr 1fr;" class="align-content-center">

            <!-- row 1 -->
            <div style="grid-area: 1 / 1 / 1 / 3" class="cell cell-center">
                <template v-if="mostUsedVehicles.length > 0">
                    <h1>
                        <census-image v-if="mostUsedVehicles[0].vehicle && mostUsedVehicles[0].vehicle.imageID != 0"
                                      :image-id="mostUsedVehicles[0].vehicle.imageID" style="height: 3rem;" class="mb-2">
                        </census-image>
                        {{mostUsedVehicles[0].vehicleName}}
                    </h1>

                    <h5>
                        {{mostUsedVehicles[0].killsAs | locale(0)}} vehicle kills
                    </h5>
                </template>

                <h1 v-else>
                    none!
                </h1>

                <h6>
                    <strong>Most used vehicle</strong>
                </h6>
            </div>
            <div style="grid-area: 1 / 3 / 1 / 6" class="cell text-left">
                <div class="d-flex">
                    <div class="flex-grow-1 mr-3" v-if="mostUsedVehicleWeapons.length >= 1">
                        <wrapped-item :entry="mostUsedVehicleWeapons[0]" :is-vehicle="true"></wrapped-item>
                        <h6><strong>Top vehicle guns</strong></h6>
                    </div>

                    <div class="flex-grow-1 mr-3" v-if="mostUsedVehicleWeapons.length >= 2">
                        <wrapped-item :entry="mostUsedVehicleWeapons[1]" :is-vehicle="true"></wrapped-item>
                        <h6><strong>&nbsp;</strong></h6>
                    </div>

                    <div class="flex-grow-1" v-if="mostUsedVehicleWeapons.length >= 3">
                        <wrapped-item :entry="mostUsedVehicleWeapons[2]" :is-vehicle="true"></wrapped-item>
                        <h6><strong>&nbsp;</strong></h6>
                    </div>
                </div>
            </div>

            <!-- row 2 -->
            <div style="grid-area: 2 / 1 / 2 / 6" class="cell text-center">
                <canvas id="chart-vehicle-kills" class="w-100" style="max-height: 300px;"></canvas>
                <h5>
                    Vehicles killed
                </h5>
            </div>

            <!-- row 3 -->
            <div v-for="(best, i) in vehicleSessionBests" :style="'grid-area: 3 / ' + (i + 1) + ' / 3 / ' + (i + 2)" class="cell">
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

</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import Chart from "chart.js/auto/auto.esm";

    // util
    import ColorUtils from "util/Color";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";
    import CensusImage from "components/CensusImage";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/CharacterName";

    // models
    import { WrappedSession, WrappedWeaponStats } from "../../common";
    import { BestSessionEntry, makeSessionBest } from "../../quick";
    import { WrappedVehicleUsage } from "../../data/vehicles";

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

    export const WrappedViewQuickVehicle = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true },
            ShowDebug: { type: Boolean, required: true }
        },

        data: function() {
            return {
                charts: {
                    vehicleKills: null as Chart | null
                },

                vehicleSessionBests: [] as BestSessionEntry[],
            }
        },

        mounted: function(): void {
            this.makeVehicleSessionBests();

            this.$nextTick(() => {
                this.makeCharts();
            });
        },

        methods: {
            makeVehicleSessionBests: function(): void {
                this.vehicleSessionBests = [];
                if (this.wrapped.extra.sessions.length == 0) {
                    return;
                }

                const sessions: WrappedSession[] = [...this.wrapped.extra.sessions];

                makeSessionBest(sessions, this.vehicleSessionBests, "Highest VKPM",
                    (iter) => (iter.vehicleKills / (Math.max(1, iter.duration) / 60)),
                    (iter) => (iter.duration > 300)
                );

                makeSessionBest(sessions, this.vehicleSessionBests, "Most vehicle repairs",
                    (iter) => iter.vehicleRepair,
                    (iter) => iter.vehicleRepair > 0,
                    0
                );

                makeSessionBest(sessions, this.vehicleSessionBests, "Most driver assists",
                    (iter) => iter.driverAssists,
                    (iter) => iter.driverAssists > 0,
                    0
                );

                makeSessionBest(sessions, this.vehicleSessionBests, "Longest vehicle kill streak",
                    (iter) => iter.vehicleKillStreak,
                    (iter) => iter.vehicleKillStreak > 4,
                    0
                );

                makeSessionBest(sessions, this.vehicleSessionBests, "Most vehicle resupplies",
                    (iter) => iter.vehicleResupply,
                    (iter) => iter.vehicleResupply > 50,
                    0
                );

                makeSessionBest(sessions, this.vehicleSessionBests, "Most roadkills",
                    (iter) => (iter.expEarned.get(26) || 0),
                    (iter) => ((iter.expEarned.get(26) || 0) > 10),
                    0
                );

            },

            makeCharts: function(): void {
                this.makeVehiclesKillChart();
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
                }).slice(0, 10);

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
            mostUsedVehicleWeapons: function(): WrappedWeaponStats[] {
                return [...this.wrapped.extra.weaponStats].filter((iter) => {
                    return iter.item != null
                        && iter.vehicleKills > 0
                        && iter.item.isVehicleWeapon == true
                        && iter.item.categoryID != 102; // infantry weapons (MANA turrets)
                }).sort((a, b) => {
                    return b.vehicleKills - a.vehicleKills;
                }).slice(0, 3);
            },

            mostUsedVehicles: function(): WrappedVehicleUsage[] {
                return [...this.wrapped.extra.vehicleUsage].filter(iter => {
                    return iter.killsAs > 0;
                }).sort((a, b) => {
                    return b.killsAs - a.killsAs;
                }).slice(0, 3);
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
            CensusImage
        }

    });
    export default WrappedViewQuickVehicle;
</script>
