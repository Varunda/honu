<template>
    <div>
        <table class="table table-sm w-auto d-inline-block" style="vertical-align: top;">
            <tr class="table-secondary">
                <th>Action</th>
                <th>Amount</th>
                <th>
                    Per minute
                    <info-hover text="For actions that are per class (such as heals), this is only the time as that class"></info-hover>
                </th>
            </tr>

            <tr>
                <td>Kills</td>
                <td>
                    <span v-if="kills.state == 'loaded'">
                        {{kills.data.length}}
                    </span>
                </td>
                <td>
                    <span v-if="kills.state == 'loaded'">
                        {{kills.data.length / durationInSeconds * 60 | fixed | locale}}
                    </span>
                </td>
            </tr>

            <tr>
                <td>Spawns</td>
                <td>
                    {{expSpawns.length}}
                </td>
                <td>
                    {{expSpawns.length / durationInSeconds * 60 | fixed | locale}}
                </td>
            </tr>

            <tr v-if="classPlaytime.medic > 0">
                <td>Revives</td>
                <td>
                    {{expRevives.length}}
                </td>
                <td>
                    {{expRevives.length / classPlaytime.medic * 60 | fixed | locale}}
                </td>
            </tr>

            <tr v-if="classPlaytime.medic > 0">
                <td>Heals</td>
                <td>
                    {{expHeals.length}}
                </td>
                <td>
                    {{expHeals.length / classPlaytime.medic * 60 | fixed | locale}}
                </td>
            </tr>

            <tr v-if="classPlaytime.engineer > 0"> 
                <td>Resupplies</td>
                <td>
                    {{expResupplies.length}}
                </td>
                <td>
                    {{expResupplies.length / classPlaytime.engineer * 60 | fixed | locale}}
                </td>
            </tr>

            <tr v-if="classPlaytime.engineer > 0">
                <td>Repairs</td>
                <td>
                    {{expRepairs.length}}
                </td>
                <td>
                    {{expRepairs.length / classPlaytime.engineer * 60 | fixed | locale}}
                </td>
            </tr>
        </table>

        <table class="table table-sm w-auto d-inline-block ml-2" style="vertical-align: top">
            <tr class="table-secondary">
                <th><b>Class</b></th>
                <td>Duration</td>
                <td>%</td>
            </tr>

            <tr>
                <td>Infiltrator</td>
                <td>{{classPlaytime.infil | mduration}}</td>
                <td>{{classPlaytime.infil / durationInSeconds * 100 | fixed | locale}}%</td>
            </tr>

            <tr>
                <td>Light Assault</td>
                <td>{{classPlaytime.lightAssault | mduration}}</td>
                <td>{{classPlaytime.lightAssault / durationInSeconds * 100 | fixed | locale}}%</td>
            </tr>

            <tr>
                <td>Medic</td>
                <td>{{classPlaytime.medic | mduration}}</td>
                <td>{{classPlaytime.medic / durationInSeconds * 100 | fixed | locale}}%</td>
            </tr>

            <tr>
                <td>Engineer</td>
                <td>{{classPlaytime.engineer | mduration}}</td>
                <td>{{classPlaytime.engineer / durationInSeconds * 100 | fixed | locale}}%</td>
            </tr>

            <tr>
                <td>Heavy</td>
                <td>{{classPlaytime.heavy | mduration}}</td>
                <td>{{classPlaytime.heavy / durationInSeconds * 100 | fixed | locale}}%</td>
            </tr>

            <tr>
                <td>MAX</td>
                <td>{{classPlaytime.max | mduration}}</td>
                <td>{{classPlaytime.max / durationInSeconds * 100 | fixed | locale}}%</td>
            </tr>

            <tr class="table-secondary">
                <td><b>Total</b></td>
                <td colspan="2">
                    {{durationInSeconds | mduration}}
                </td>
            </tr>
        </table>

        <canvas id="chart-class-playtime" style="max-height: 300px; max-width: 25%" class="w-auto d-inline-block mb-2" @click="togglePink"></canvas>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";

    import { ExpandedKillEvent, KillEvent, KillStatApi } from "api/KillStatApi";
    import { Experience, ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { Session } from "api/SessionApi";

    import Loadout from "util/Loadout";

    import { randomRGB, rgbToString } from "util/Color";

    interface LoadoutEvent {
        loadoutID: number;
        timestamp: Date;
    }

    export const SessionViewerGeneral = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Object as PropType<Loading<ExpEvent[]>>, required: true },
            kills: { type: Object as PropType<Loading<ExpandedKillEvent[]>>, required: true }
        },

        data: function() {
            return {
                classPlaytime: {
                    infil: 0 as number,
                    lightAssault: 0 as number,
                    medic: 0 as number,
                    engineer: 0 as number,
                    heavy: 0 as number,
                    max: 0 as number
                },

                chart: null as Chart | null,


                usePalePink: false as boolean
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.setClassPlaytime();
                this.generateClassPlaytimeChart();
            });
        },

        methods: {

            togglePink: function(): void {
                this.usePalePink = !this.usePalePink;
                this.generateClassPlaytimeChart();
            },

            generateClassPlaytimeChart: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

                if (document.getElementById("chart-class-playtime") == null) {
                    console.warn(`Failed to find #chart-class-playtime, typo or not mounted yet?`);
                    return;
                }

                const ctx = (document.getElementById("chart-class-playtime") as any).getContext("2d");
                this.chart = new Chart(ctx, {
                    type: "pie",
                    data: {
                        labels: [ "Infiltrator", "Light Assault", "Medic", "Engineer", "Heavy Assault", "MAX" ],
                        datasets: [{
                            data: [
                                this.classPlaytime.infil,
                                this.classPlaytime.lightAssault,
                                this.classPlaytime.medic,
                                this.classPlaytime.engineer,
                                this.classPlaytime.heavy,
                                this.classPlaytime.max
                            ],
                            backgroundColor: [
                                "#666699", // infil
                                "#0d239d", // LA
                                "#cc0000", // medic
                                "#009900", // engi
                                (this.usePalePink) ? "#fcd5e7" : "#ff69b4", // heavy
                                "#663300", // max
                            ]
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

            setClassPlaytime: function(): void {
                const events: LoadoutEvent[] = [];

                if (this.exp.state == "loaded") {
                    events.push(...this.exp.data.map(iter => { return { loadoutID: iter.loadoutID, timestamp: iter.timestamp }; }));
                }

                if (this.kills.state == "loaded") {
                    events.push(...this.kills.data.map(iter => { return { loadoutID: iter.event.attackerLoadoutID, timestamp: iter.event.timestamp }; }));
                }

                if (events.length < 2) {
                    return;
                }

                let prev: LoadoutEvent = events[0];

                for (let i = 0; i < events.length; ++i) {
                    const iter: LoadoutEvent = events[i];
                    const seconds = (iter.timestamp.getTime() - prev.timestamp.getTime()) / 1000;

                    if (Loadout.isInfiltrator(iter.loadoutID)) {
                        this.classPlaytime.infil += seconds;
                    } else if (Loadout.isLightAssault(iter.loadoutID)) {
                        this.classPlaytime.lightAssault += seconds;
                    } else if (Loadout.isMedic(iter.loadoutID)) {
                        this.classPlaytime.medic += seconds;
                    } else if (Loadout.isEngineer(iter.loadoutID)) {
                        this.classPlaytime.engineer += seconds;
                    } else if (Loadout.isHeavy(iter.loadoutID)) {
                        this.classPlaytime.heavy += seconds;
                    } else if (Loadout.isMax(iter.loadoutID)) {
                        this.classPlaytime.max += seconds;
                    }

                    prev = iter;
                }

                if (this.chart == null) {
                    this.generateClassPlaytimeChart();
                }
            },

        },

        computed: {
            durationInSeconds: function(): number {
                return ((this.session.end || new Date()).getTime() - this.session.start.getTime()) / 1000;
            },

            expHeals: function(): ExpEvent[] {
                if (this.exp.state != "loaded") {
                    return [];
                }
                return this.exp.data.filter(iter => iter.experienceID == Experience.HEAL || iter.experienceID == Experience.SQUAD_HEAL);
            },

            expRevives: function(): ExpEvent[] {
                if (this.exp.state != "loaded") {
                    return [];
                }
                return this.exp.data.filter(iter => iter.experienceID == Experience.REVIVE || iter.experienceID == Experience.SQUAD_REVIVE);
            },

            expResupplies: function(): ExpEvent[] {
                if (this.exp.state != "loaded") {
                    return [];
                }
                return this.exp.data.filter(iter => iter.experienceID == Experience.RESUPPLY || iter.experienceID == Experience.SQUAD_RESUPPLY);
            },

            expRepairs: function(): ExpEvent[] {
                if (this.exp.state != "loaded") {
                    return [];
                }
                return this.exp.data.filter(iter => iter.experienceID == Experience.MAX_REPAIR || iter.experienceID == Experience.SQUAD_MAX_REPAIR);
            },

            expSpawns: function(): ExpEvent[] {
                if (this.exp.state != "loaded") {
                    return [];
                }
                return this.exp.data.filter(iter => Experience.isSpawn(iter.experienceID));
            }
        },

        components: {
            InfoHover,
            Busy,
        }

    });
    export default SessionViewerGeneral;
</script>