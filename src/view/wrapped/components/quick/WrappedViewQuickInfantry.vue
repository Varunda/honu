<template>
    <collapsible header-text="Infantry" class="text-center">

        <img class="wrapped-bg-img" :src="'/img/wrapped/' + imageUrl" />

        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr 1fr; min-height: 900px;" class="align-content-center">
            <!-- row 1 -->
            <div style="grid-area: 1 / 1 / 1 / 3" class="cell cell-center">
                <h1>
                    <img :src="'/img/classes/icon_' + mostPlayedClass.icon + '.png'" style="height: 3rem;" class="mb-2" />
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
                        <wrapped-item :entry="mostUsedInfantryWeapons[0]"></wrapped-item>
                        <h6><strong>Top infantry guns</strong></h6>
                    </div>

                    <div class="flex-grow-1 mr-3" v-if="mostUsedInfantryWeapons.length >= 2">
                        <wrapped-item :entry="mostUsedInfantryWeapons[1]"></wrapped-item>
                        <h6><strong>&nbsp;</strong></h6>
                    </div>

                    <div class="flex-grow-1" v-if="mostUsedInfantryWeapons.length >= 3">
                        <wrapped-item :entry="mostUsedInfantryWeapons[2]"></wrapped-item>
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

            <!-- row 3: session bests -->
            <div v-for="(best, i) in infantrySessionBests" :style="'grid-area: 3 / ' + (i + 1) + ' / 3 / ' + (i + 2)" class="cell">
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
    import LocaleUtil from "util/Locale";

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
    import { EntityFought, WrappedClassStats, WrappedSession, WrappedWeaponStats } from "../../common";
    import { BestSessionEntry, makeSessionBest } from "../../quick";

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

    export const WrappedViewQuickInfantry = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true },
            ShowDebug: { type: Boolean, required: true }
        },

        data: function() {
            return {
                charts: {
                    outfitKills: null as Chart | null,
                },

                infantrySessionBests: [] as BestSessionEntry[],

                imageUrl: "g_battle_air.png" as string
            }
        },

        mounted: function(): void {
            this.setImageBackground();

            this.makeInfantrySessionBests();

            this.$nextTick(() => {
                this.makeCharts();
            });
        },

        methods: {

            setImageBackground: function(): void {
                const mostClass = this.mostPlayedClass;

                let url = `c_`;

                switch (mostClass.icon) {
                    case "infil": url += "infiltrator"; break;
                    case "light": url += "light_assault"; break;
                    case "medic": url += "medic"; break;
                    case "engi": url += "engineer"; break;
                    case "heavy": url += "heavy_assault"; break;
                    case "max": url += "MAX"; break;
                    default:
                        console.warn(`failed to match class ${mostClass.icon}!`);
                        break;
                }

                const factionID: number = this.wrapped.extra.focusedCharacter.factionID;
                switch (factionID) {
                    case 1: url += "_VS"; break;
                    case 2: url += "_NC"; break;
                    case 3: url += "_TR"; break;
                    case 4: url += "_VS"; break;
                    default:
                        console.warn(`failed to match faction ${factionID}!`);
                        break;
                }

                if (mostClass.name == "max" && factionID == 4) {
                    url = `c_MAX_Disruptor_NS`;
                }

                this.imageUrl = url + ".png";
            },

            makeInfantrySessionBests: function(): void {
                this.infantrySessionBests = [];
                if (this.wrapped.extra.sessions.length == 0) {
                    return;
                }

                const sessions: WrappedSession[] = [...this.wrapped.extra.sessions];

                makeSessionBest(sessions, this.infantrySessionBests, "Highest KPM session",
                    (iter) => (iter.kills / (Math.max(1, iter.duration) / 60)),
                    (iter) => (iter.duration > 300)
                );

                const highestKd: WrappedSession | undefined = sessions.filter(iter => iter.duration > 300 && iter.deaths > 0 && iter.infantryKills > 25)
                    .sort((a, b) => (b.infantryKills / Math.max(1, b.deaths)) - (a.infantryKills / Math.max(1, a.deaths))).at(0);

                // only show if the highest KD is above 1, don't wanna embarrass someone lol
                if (highestKd != undefined && (highestKd.infantryKills / Math.max(1, highestKd.deaths)) > 1) {
                    this.infantrySessionBests.push({
                        name: "Highest KD session",
                        value: LocaleUtil.locale(highestKd.infantryKills / Math.max(1, highestKd.deaths), 2),
                        session: highestKd
                    });
                }

                if (this.mostPlayedClass.icon == "medic") {
                    makeSessionBest(sessions, this.infantrySessionBests, "Most revives",
                        (iter) => (iter.revives),

                        (iter) => (iter.revives > 0),
                        0
                    );

                    makeSessionBest(sessions, this.infantrySessionBests, "Most heals", (iter) => (iter.heals),
                        (iter) => (iter.heals > 0),
                        0
                    );

                    makeSessionBest(sessions, this.infantrySessionBests, "Most shield repairs", (iter) => iter.shieldRepairs,
                        (iter) => iter.shieldRepairs >= 100,
                        0
                    );
                } else if (this.mostPlayedClass.icon == "engi") {
                    makeSessionBest(sessions, this.infantrySessionBests, "Most resupplies", (iter) => iter.resupplies,
                        (iter) => iter.resupplies > 0,
                        0
                    );

                    makeSessionBest(sessions, this.infantrySessionBests, "Most MAX repairs", (iter) => iter.maxRepair,
                        (iter) => iter.maxRepair > 100,
                        0
                    );

                    makeSessionBest(sessions, this.infantrySessionBests, "Most hardlight assists", (iter) => iter.hardlightAssists,
                        (iter) => iter.hardlightAssists > 0,
                        0
                    );
                } else if (this.mostPlayedClass.icon == "heavy") {
                    makeSessionBest(sessions, this.infantrySessionBests, "Most assists", (iter) => iter.assists,
                        (iter) => iter.assists > 0,
                        0
                    );

                    makeSessionBest(sessions, this.infantrySessionBests, "Most MAX kills", (iter) => iter.maxKills,
                        (iter) => iter.maxKills > 0,
                        0
                    );
                }
            },

            makeCharts: function(): void {
                this.makeOutfitsKilledGraph();
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
                            // if a tag is given, only show the tag
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
        },

        computed: {
            mostPlayedClass: function(): WrappedClassStats {
                return [...this.wrapped.extra.classStats].sort((a, b) => {
                    return b.timeAs - a.timeAs;
                })[0];
            },

            mostUsedInfantryWeapons: function(): WrappedWeaponStats[] {
                return [...this.wrapped.extra.weaponStats].filter((iter) => {
                    return iter.item != null
                        && iter.item.isVehicleWeapon == false
                        && iter.kills > 0;
                }).sort((a, b) => {
                    return b.kills - a.kills;
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
        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
            WrappedItem,
            CensusImage
        }

    });
    export default WrappedViewQuickInfantry;
</script>
