<template>
    <collapsible header-text="Class stats">
        <table class="table table-hover">
            <tr class="table-secondary th-border-top-0">
                <th>Class</th>
                <th>Kills</th>
                <th>Time</th>
                <th>KPM</th>
                <th>Deaths</th>
                <th>KD</th>
                <th>Players</th>
            </tr>

            <tbody>
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
            </tbody>
        </table>

        <div class="row mb-2">
            <div class="col-12 col-lg-2 border-right">
                <h5>
                    All
                    <info-hover text="This are box whisker plots, which show the min, Q1/25%, Q2/median, Q3/75% and max of a dataset. The dot is the mean/average"></info-hover>
                </h5>
                <chart-box-whisker :data="allKPM"></chart-box-whisker>
                <chart-box-whisker :data="allKD"></chart-box-whisker>
            </div>

            <div class="col-12 col-lg-2 border-right">
                <h5>Infiltrator</h5>
                <chart-box-whisker :data="infilKPM"></chart-box-whisker>
                <chart-box-whisker :data="infilKD"></chart-box-whisker>
            </div>

            <div class="col-12 col-lg-2 border-right">
                <h5>Light Assault</h5>
                <chart-box-whisker :data="lightAssaultKPM"></chart-box-whisker>
                <chart-box-whisker :data="lightAssaultKD"></chart-box-whisker>
            </div>

            <div class="col-12 col-lg-2 border-right">
                <h5>Medic</h5>
                <chart-box-whisker :data="medicKPM"></chart-box-whisker>
                <chart-box-whisker :data="medicKD"></chart-box-whisker>
            </div>

            <div class="col-12 col-lg-2 border-right">
                <h5>Engineer</h5>
                <chart-box-whisker :data="engineerKPM"></chart-box-whisker>
                <chart-box-whisker :data="engineerKD"></chart-box-whisker>
            </div>

            <div class="col-12 col-lg-2 border-right">
                <h5>Heavy</h5>
                <chart-box-whisker :data="heavyKPM"></chart-box-whisker>
                <chart-box-whisker :data="heavyKD"></chart-box-whisker>
            </div>
        </div>
    </collapsible>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerClassStats, PlayerMetadata, ReportParameters } from "../Report";

    import Loadout from "util/Loadout";

    import "filters/LocaleFilter";
    import "MomentFilter";

    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";
    import ChartBoxWhisker from "./charts/ChartBoxWhisker.vue";

    class ClassStats {
        public kills: number = 0;
        public vehicleKills: number = 0;
        public deaths: number = 0;
        public count: number = 0;
        public timeAs: number = 0;
    }

    export const ReportClassBreakdown = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
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

        computed: {
            allKPM: function(): number[] { return Array.from(this.report.playerMetadata.values()).filter(iter => iter.timeAs > 60 * 5).map(iter => iter.kills.length / iter.timeAs * 60); },
            infilKPM: function(): number[] { return classKpms(this.report, (p) => p.classes.infil); },
            lightAssaultKPM: function(): number[] { return classKpms(this.report, (p) => p.classes.lightAssault); },
            medicKPM: function(): number[] { return classKpms(this.report, (p) => p.classes.medic); },
            engineerKPM: function(): number[] { return classKpms(this.report, (p) => p.classes.engineer); },
            heavyKPM: function(): number[] { return classKpms(this.report, (p) => p.classes.heavy); },
            maxKPM: function(): number[] { return classKpms(this.report, (p) => p.classes.max); },

            allKD: function(): number[] { return Array.from(this.report.playerMetadata.values()).filter(iter => iter.timeAs > 60 * 5).map(iter => iter.kills.length / Math.max(1, iter.deaths.length)); },
            infilKD: function(): number[] { return classKds(this.report, (p) => p.classes.infil); },
            lightAssaultKD: function(): number[] { return classKds(this.report, (p) => p.classes.lightAssault); },
            medicKD: function(): number[] { return classKds(this.report, (p) => p.classes.medic); },
            engineerKD: function(): number[] { return classKds(this.report, (p) => p.classes.engineer); },
            heavyKD: function(): number[] { return classKds(this.report, (p) => p.classes.heavy); },
            maxKD: function(): number[] { return classKds(this.report, (p) => p.classes.max); }
        },

        components: {
            Collapsible,
            ChartBoxWhisker,
            InfoHover
        }
    });

    function classKds(report: Report, selector: (_: PlayerMetadata) => PlayerClassStats): number[] {
        return Array.from(report.playerMetadata.values())
            .filter(iter => selector(iter).timeAs > 60 * 5)
            .map(iter => selector(iter).kills / Math.max(1, selector(iter).deaths));
    }

    function classKpms(report: Report, selector: (_: PlayerMetadata) => PlayerClassStats): number[] {
        return Array.from(report.playerMetadata.values())
            .filter(iter => selector(iter).timeAs > 60 * 5)
            .map(iter => selector(iter).kills / selector(iter).timeAs * 60);
    }

    export default ReportClassBreakdown;
</script>