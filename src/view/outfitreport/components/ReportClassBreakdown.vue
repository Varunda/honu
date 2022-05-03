<template>
    <collapsible header-text="Class stats">
        <table class="table table-sm">
            <tr class="table-secondary">
                <th>Class</th>
                <th>Kills</th>
                <th>Time</th>
                <th>KPM</th>
                <th>Deaths</th>
                <th>KD</th>
                <th>Players</th>
            </tr>

            <tr>
                <td>Total</td>
                <td>{{all.kills}}</td>
                <td>{{all.timeAs | mduration}}</td>
                <td>{{all.kills / Math.max(1, all.timeAs) * 60 | locale(2)}}</td>
                <td>{{all.deaths}}</td>
                <td>{{all.kills / Math.max(1, all.deaths) | locale(2)}}</td>
                <td>{{all.count}}</td>
            </tr>

            <tr>
                <td>Infiltrator</td>
                <td>{{infil.kills}}</td>
                <td>{{infil.timeAs | mduration}}</td>
                <td>{{infil.kills / Math.max(1, infil.timeAs) * 60 | locale(2)}}</td>
                <td>{{infil.deaths}}</td>
                <td>{{infil.kills / Math.max(1, infil.deaths) | locale(2)}}</td>
                <td>{{infil.count}}</td>
            </tr>

            <tr>
                <td>Light Assault</td>
                <td>{{lightAssault.kills}}</td>
                <td>{{lightAssault.timeAs | mduration}}</td>
                <td>{{lightAssault.kills / Math.max(1, lightAssault.timeAs) * 60 | locale(2)}}</td>
                <td>{{lightAssault.deaths}}</td>
                <td>{{lightAssault.kills / Math.max(1, lightAssault.deaths) | locale(2)}}</td>
                <td>{{lightAssault.count}}</td>
            </tr>

            <tr>
                <td>Medic</td>
                <td>{{medic.kills}}</td>
                <td>{{medic.timeAs | mduration}}</td>
                <td>{{medic.kills / Math.max(1, medic.timeAs) * 60 | locale(2)}}</td>
                <td>{{medic.deaths}}</td>
                <td>{{medic.kills / Math.max(1, medic.deaths) | locale(2)}}</td>
                <td>{{medic.count}}</td>
            </tr>

            <tr>
                <td>Engineer</td>
                <td>{{engineer.kills}}</td>
                <td>{{engineer.timeAs | mduration}}</td>
                <td>{{engineer.kills / Math.max(1, engineer.timeAs) * 60 | locale(2)}}</td>
                <td>{{engineer.deaths}}</td>
                <td>{{engineer.kills / Math.max(1, engineer.deaths) | locale(2)}}</td>
                <td>{{engineer.count}}</td>
            </tr>

            <tr>
                <td>Heavy</td>
                <td>{{heavy.kills}}</td>
                <td>{{heavy.timeAs | mduration}}</td>
                <td>{{heavy.kills / Math.max(1, heavy.timeAs) * 60 | locale(2)}}</td>
                <td>{{heavy.deaths}}</td>
                <td>{{heavy.kills / Math.max(1, heavy.deaths) | locale(2)}}</td>
                <td>{{heavy.count}}</td>
            </tr>

            <tr>
                <td>MAX</td>
                <td>{{max.kills}}</td>
                <td>{{max.timeAs | mduration}}</td>
                <td>{{max.kills / Math.max(1, max.timeAs) * 60 | locale(2)}}</td>
                <td>{{max.deaths}}</td>
                <td>{{max.kills / Math.max(1, max.deaths) | locale(2)}}</td>
                <td>{{max.count}}</td>
            </tr>
        </table>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    import Loadout from "util/Loadout";

    import "filters/LocaleFilter";
    import "MomentFilter";

    import Collapsible from "components/Collapsible.vue";

    class ClassStats {
        public kills: number = 0;
        public vehicleKills: number = 0;
        public deaths: number = 0;
        public count: number = 0;
        public timeAs: number = 0;
    }

    export const ReportClassBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {
                hideHint: false as boolean,

                all: new ClassStats() as ClassStats,
                infil: new ClassStats() as ClassStats,
                lightAssault: new ClassStats() as ClassStats,
                medic: new ClassStats() as ClassStats,
                engineer: new ClassStats() as ClassStats,
                heavy: new ClassStats() as ClassStats,
                max: new ClassStats() as ClassStats
            }
        },

        mounted: function(): void {
            this.make();
        },

        methods: {
            make: function(): void {
                this.setKills();
                this.setDeaths();

                for (const metadata of Array.from(this.report.playerMetadata.values())) {
                    if (metadata.classes.mostPlayed == metadata.classes.infil) {
                        ++this.infil.count;
                    } else if (metadata.classes.mostPlayed == metadata.classes.lightAssault) {
                        ++this.lightAssault.count;
                    } else if (metadata.classes.mostPlayed == metadata.classes.medic) {
                        ++this.medic.count;
                    } else if (metadata.classes.mostPlayed == metadata.classes.engineer) {
                        ++this.engineer.count;
                    } else if (metadata.classes.mostPlayed == metadata.classes.heavy) {
                        ++this.heavy.count;
                    } else if (metadata.classes.mostPlayed == metadata.classes.max) {
                        ++this.max.count;
                    }

                    this.all.timeAs += metadata.timeAs;
                    this.infil.timeAs += metadata.classes.infil.timeAs;
                    this.lightAssault.timeAs += metadata.classes.lightAssault.timeAs;
                    this.medic.timeAs += metadata.classes.medic.timeAs;
                    this.engineer.timeAs += metadata.classes.engineer.timeAs;
                    this.heavy.timeAs += metadata.classes.heavy.timeAs;
                    this.max.timeAs += metadata.classes.max.timeAs;
                }

                this.all.count = this.report.playerMetadata.size;
            },

            setKills: function(): void {
                for (const kill of this.report.kills) {
                    const id: number = kill.attackerLoadoutID;

                    ++this.all.kills;

                    if (Loadout.isInfiltrator(id)) {
                        ++this.infil.kills;
                    } else if (Loadout.isLightAssault(id)) {
                        ++this.lightAssault.kills;
                    } else if (Loadout.isMedic(id)) {
                        ++this.medic.kills;
                    } else if (Loadout.isEngineer(id)) {
                        ++this.engineer.kills;
                    } else if (Loadout.isHeavy(id)) {
                        ++this.heavy.kills;
                    } else if (Loadout.isMax(id)) {
                        ++this.max.kills;
                    }
                }
            },

            setDeaths: function(): void {
                for (const death of this.report.deaths) {
                    const id: number = death.killedLoadoutID;

                    ++this.all.deaths;

                    if (Loadout.isInfiltrator(id)) {
                        ++this.infil.deaths;
                    } else if (Loadout.isLightAssault(id)) {
                        ++this.lightAssault.deaths;
                    } else if (Loadout.isMedic(id)) {
                        ++this.medic.deaths;
                    } else if (Loadout.isEngineer(id)) {
                        ++this.engineer.deaths;
                    } else if (Loadout.isHeavy(id)) {
                        ++this.heavy.deaths;
                    } else if (Loadout.isMax(id)) {
                        ++this.max.deaths;
                    }
                }
            }
        },

        components: {
            Collapsible
        }
    });

    export default ReportClassBreakdown;
</script>