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
                    {{kills.length}}
                </td>
                <td>
                    {{kills.length / durationInSeconds * 60 | fixed | locale}}
                </td>
            </tr>

            <tr>
                <td>Assists</td>
                <td>
                    {{expAssists.length}}
                </td>
                <td>
                    {{expAssists.length / durationInSeconds * 60 | fixed | locale}}
                </td>
            </tr>

            <tr>
                <td>Deaths</td>
                <td>
                    {{deaths.filter(iter => iter.event.attackerTeamID != iter.event.killedTeamID).length}}
                </td>
                <td>
                    --
                </td>
            </tr>

            <tr>
                <td>TKed</td>
                <td>
                    {{deaths.filter(iter => iter.event.attackerTeamID == iter.event.killedTeamID).length}}
                </td>
                <td>
                    --
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
                <td>Kills</td>
                <td>Deaths</td>
                <td>K/D</td>
            </tr>

            <tr>
                <td>Infiltrator</td>
                <td>{{classPlaytime.infil.secondsAs | mduration}}</td>
                <td>{{classPlaytime.infil.secondsAs / durationInSeconds * 100 | fixed | locale}}%</td>
                <td>{{classPlaytime.infil.kills}}</td>
                <td>{{classPlaytime.infil.deaths}}</td>
                <td>{{classPlaytime.infil.kills / Math.max(classPlaytime.infil.deaths, 1) | fixed}}</td>
            </tr>

            <tr>
                <td>Light Assault</td>
                <td>{{classPlaytime.lightAssault.secondsAs | mduration}}</td>
                <td>{{classPlaytime.lightAssault.secondsAs / durationInSeconds * 100 | fixed | locale}}%</td>
                <td>{{classPlaytime.lightAssault.kills}}</td>
                <td>{{classPlaytime.lightAssault.deaths}}</td>
                <td>{{classPlaytime.lightAssault.kills / Math.max(classPlaytime.lightAssault.deaths, 1) | fixed}}</td>
            </tr>

            <tr>
                <td>Medic</td>
                <td>{{classPlaytime.medic.secondsAs | mduration}}</td>
                <td>{{classPlaytime.medic.secondsAs / durationInSeconds * 100 | fixed | locale}}%</td>
                <td>{{classPlaytime.medic.kills}}</td>
                <td>{{classPlaytime.medic.deaths}}</td>
                <td>{{classPlaytime.medic.kills / Math.max(classPlaytime.medic.deaths, 1) | fixed}}</td>
            </tr>

            <tr>
                <td>Engineer</td>
                <td>{{classPlaytime.engineer.secondsAs | mduration}}</td>
                <td>{{classPlaytime.engineer.secondsAs / durationInSeconds * 100 | fixed | locale}}%</td>
                <td>{{classPlaytime.engineer.kills}}</td>
                <td>{{classPlaytime.engineer.deaths}}</td>
                <td>{{classPlaytime.engineer.kills / Math.max(classPlaytime.engineer.deaths, 1) | fixed}}</td>
            </tr>

            <tr>
                <td>Heavy</td>
                <td>{{classPlaytime.heavy.secondsAs | mduration}}</td>
                <td>{{classPlaytime.heavy.secondsAs / durationInSeconds * 100 | fixed | locale}}%</td>
                <td>{{classPlaytime.heavy.kills}}</td>
                <td>{{classPlaytime.heavy.deaths}}</td>
                <td>{{classPlaytime.heavy.kills / Math.max(classPlaytime.heavy.deaths, 1) | fixed}}</td>
            </tr>

            <tr>
                <td>MAX</td>
                <td>{{classPlaytime.max.secondsAs | mduration}}</td>
                <td>{{classPlaytime.max.secondsAs / durationInSeconds * 100 | fixed | locale}}%</td>
                <td>{{classPlaytime.max.kills}}</td>
                <td>{{classPlaytime.max.deaths}}</td>
                <td>{{classPlaytime.max.kills / Math.max(classPlaytime.max.deaths, 1) | fixed}}</td>
            </tr>

            <tr class="table-secondary">
                <td><b>Total</b></td>
                <td colspan="5">
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
    import { Experience, ExpandedExpEvent, ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { Session } from "api/SessionApi";

    import Loadout from "util/Loadout";

    import { randomRGB, rgbToString } from "util/Color";

    interface LoadoutEvent {
        loadoutID: number;
        timestamp: Date;
    }

    class ClassSummary {
        public secondsAs: number = 0;
        public kills: number = 0;
        public deaths: number = 0;
    }

    export const SessionViewerGeneral = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Array as PropType<ExpandedExpEvent[]>, required: true },
            kills: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            deaths: { type: Array as PropType<ExpandedKillEvent[]>, required: true }
        },

        data: function() {
            return {
                classPlaytime: {
                    infil: new ClassSummary() as ClassSummary,
                    lightAssault: new ClassSummary() as ClassSummary,
                    medic: new ClassSummary() as ClassSummary,
                    engineer: new ClassSummary() as ClassSummary,
                    heavy: new ClassSummary() as ClassSummary,
                    max: new ClassSummary() as ClassSummary,
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
                                this.classPlaytime.infil.secondsAs,
                                this.classPlaytime.lightAssault.secondsAs,
                                this.classPlaytime.medic.secondsAs,
                                this.classPlaytime.engineer.secondsAs,
                                this.classPlaytime.heavy.secondsAs,
                                this.classPlaytime.max.secondsAs
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

                events.push(...this.exp.map(iter => { return { loadoutID: iter.event.loadoutID, timestamp: iter.event.timestamp }; }));
                events.push(...this.kills.map(iter => { return { loadoutID: iter.event.attackerLoadoutID, timestamp: iter.event.timestamp }; }));
                events.push(...this.deaths.map(iter => { return { loadoutID: iter.event.killedLoadoutID, timestamp: iter.event.timestamp }; }));
                events.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());

                if (events.length < 2) {
                    return;
                }

                let prev: LoadoutEvent = events[0];

                for (let i = 0; i < events.length; ++i) {
                    const iter: LoadoutEvent = events[i];
                    const seconds = (iter.timestamp.getTime() - prev.timestamp.getTime()) / 1000;

                    if (Loadout.isInfiltrator(iter.loadoutID)) {
                        this.classPlaytime.infil.secondsAs += seconds;
                    } else if (Loadout.isLightAssault(iter.loadoutID)) {
                        this.classPlaytime.lightAssault.secondsAs += seconds;
                    } else if (Loadout.isMedic(iter.loadoutID)) {
                        this.classPlaytime.medic.secondsAs += seconds;
                    } else if (Loadout.isEngineer(iter.loadoutID)) {
                        this.classPlaytime.engineer.secondsAs += seconds;
                    } else if (Loadout.isHeavy(iter.loadoutID)) {
                        this.classPlaytime.heavy.secondsAs += seconds;
                    } else if (Loadout.isMax(iter.loadoutID)) {
                        this.classPlaytime.max.secondsAs += seconds;
                    }

                    prev = iter;
                }

                this.classPlaytime.infil.kills = this.kills.filter(iter => Loadout.isInfiltrator(iter.event.attackerLoadoutID)).length;
                this.classPlaytime.infil.deaths = this.deaths.filter(iter => Loadout.isInfiltrator(iter.event.attackerLoadoutID)).length;

                this.classPlaytime.lightAssault.kills = this.kills.filter(iter => Loadout.isLightAssault(iter.event.attackerLoadoutID)).length;
                this.classPlaytime.lightAssault.deaths = this.deaths.filter(iter => Loadout.isLightAssault(iter.event.attackerLoadoutID)).length;

                this.classPlaytime.medic.kills = this.kills.filter(iter => Loadout.isMedic(iter.event.attackerLoadoutID)).length;
                this.classPlaytime.medic.deaths = this.deaths.filter(iter => Loadout.isMedic(iter.event.attackerLoadoutID)).length;

                this.classPlaytime.engineer.kills = this.kills.filter(iter => Loadout.isEngineer(iter.event.attackerLoadoutID)).length;
                this.classPlaytime.engineer.deaths = this.deaths.filter(iter => Loadout.isEngineer(iter.event.attackerLoadoutID)).length;

                this.classPlaytime.heavy.kills = this.kills.filter(iter => Loadout.isHeavy(iter.event.attackerLoadoutID)).length;
                this.classPlaytime.heavy.deaths = this.deaths.filter(iter => Loadout.isHeavy(iter.event.attackerLoadoutID)).length;

                this.classPlaytime.max.kills = this.kills.filter(iter => Loadout.isMax(iter.event.attackerLoadoutID)).length;
                this.classPlaytime.max.deaths = this.deaths.filter(iter => Loadout.isMax(iter.event.attackerLoadoutID)).length;

                if (this.chart == null) {
                    this.generateClassPlaytimeChart();
                }
            },

        },

        computed: {
            durationInSeconds: function(): number {
                return ((this.session.end || new Date()).getTime() - this.session.start.getTime()) / 1000;
            },

            expHeals: function(): ExpandedExpEvent[] {
                return this.exp.filter(iter => Experience.isHeal(iter.event.experienceID));
            },

            expRevives: function(): ExpandedExpEvent[] {
                return this.exp.filter(iter => Experience.isRevive(iter.event.experienceID));
            },

            expResupplies: function(): ExpandedExpEvent[] {
                return this.exp.filter(iter => Experience.isResupply(iter.event.experienceID));
            },

            expRepairs: function(): ExpandedExpEvent[] {
                return this.exp.filter(iter => Experience.isMaxRepair(iter.event.experienceID));
            },

            expSpawns: function(): ExpandedExpEvent[] {
                return this.exp.filter(iter => Experience.isSpawn(iter.event.experienceID));
            },

            expAssists: function(): ExpandedExpEvent[] {
                return this.exp.filter(iter => Experience.isAssist(iter.event.experienceID));
            }
        },

        components: {
            InfoHover,
            Busy,
        }

    });
    export default SessionViewerGeneral;
</script>