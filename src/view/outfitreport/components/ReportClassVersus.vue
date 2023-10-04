<template>
    <div>
        <collapsible header-text="Class versus">
            <table class="table">
                <thead class="table-secondary th-border-top-0">
                    <tr class="th-border-top-0">
                        <th>Class</th>
                        <th>
                            Kills (%)
                            <info-hover text="How many kills were against this class"></info-hover>
                        </th>
                        <th>
                            Unique kills
                        </th>
                        <th>
                            Deaths (%)
                            <info-hover text="How many deaths were from this class"></info-hover>
                        </th>
                        <th>
                            Unique deaths
                        </th>
                        <th>K/D</th>
                    </tr>
                </thead>

                <tbody>
                    <tr>
                        <td>Infiltrator</td>
                        <td>
                            {{infil.killed}} ({{infil.killed / report.kills.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{infil.uniqueKilled.size}}
                        </td>
                        <td>
                            {{infil.deathsFrom}} ({{infil.deathsFrom / report.deaths.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{infil.uniqueDeaths.size}}
                        </td>
                        <td>{{infil.killed / Math.max(1, infil.deathsFrom) | locale(2)}}</td>
                    </tr>
                    <tr>
                        <td>Light assault</td>
                        <td>
                            {{lightAssault.killed}} ({{lightAssault.killed / report.kills.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{lightAssault.uniqueKilled.size}}
                        </td>
                        <td>
                            {{lightAssault.deathsFrom}} ({{lightAssault.deathsFrom / report.deaths.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{lightAssault.uniqueDeaths.size}}
                        </td>
                        <td>{{lightAssault.killed / Math.max(1, lightAssault.deathsFrom) | locale(2)}}</td>
                    </tr>
                    <tr>
                        <td>Medic</td>
                        <td>
                            {{medic.killed}} ({{medic.killed / report.kills.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{medic.uniqueKilled.size}}
                        </td>
                        <td>
                            {{medic.deathsFrom}} ({{medic.deathsFrom / report.deaths.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{medic.uniqueDeaths.size}}
                        </td>
                        <td>{{medic.killed / Math.max(1, medic.deathsFrom) | locale(2)}}</td>
                    </tr>
                    <tr>
                        <td>Engineer</td>
                        <td>
                            {{engi.killed}} ({{engi.killed / report.kills.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{engi.uniqueKilled.size}}
                        </td>
                        <td>
                            {{engi.deathsFrom}} ({{engi.deathsFrom / report.deaths.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{engi.uniqueDeaths.size}}
                        </td>
                        <td>{{engi.killed / Math.max(1, engi.deathsFrom) | locale(2)}}</td>
                    </tr>
                    <tr>
                        <td>Heavy</td>
                        <td>
                            {{heavy.killed}} ({{heavy.killed / report.kills.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{heavy.uniqueKilled.size}}
                        </td>
                        <td>
                            {{heavy.deathsFrom}} ({{heavy.deathsFrom / report.deaths.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{heavy.uniqueDeaths.size}}
                        </td>
                        <td>{{heavy.killed / Math.max(1, heavy.deathsFrom) | locale(2)}}</td>
                    </tr>
                    <tr>
                        <td>MAX</td>
                        <td>
                            {{max.killed}} ({{max.killed / report.kills.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{max.uniqueKilled.size}}
                        </td>
                        <td>
                            {{max.deathsFrom}} ({{max.deathsFrom / report.deaths.length * 100 | locale(2)}}%)
                        </td>
                        <td>
                            {{max.uniqueDeaths.size}}
                        </td>
                        <td>{{max.killed / Math.max(1, max.deathsFrom) | locale(2)}}</td>
                    </tr>
                </tbody>

            </table>


        </collapsible>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { ReportParameters } from "../Report";

    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";

    import LoadoutUtils from "util/Loadout";

    class ClassVersusEntry {
        public name: string = "";
        public killed: number = 0;
        public deathsFrom: number = 0;
        public uniqueKilled: Set<string> = new Set();
        public uniqueDeaths: Set<string> = new Set();
    }

    export const ReportClassVersus = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true },
            parameters: { type: Object as PropType<ReportParameters>, required: true }
        },

        data: function() {
            return {
                infil: new ClassVersusEntry() as ClassVersusEntry,
                lightAssault: new ClassVersusEntry() as ClassVersusEntry,
                medic: new ClassVersusEntry() as ClassVersusEntry,
                engi: new ClassVersusEntry() as ClassVersusEntry,
                heavy: new ClassVersusEntry() as ClassVersusEntry,
                max: new ClassVersusEntry() as ClassVersusEntry
            }
        },

        mounted: function(): void {
            this.makeData();
        },

        methods: {

            makeData: function(): void {

                for (const kill of this.report.kills) {
                    const id: number = kill.killedLoadoutID;

                    if (LoadoutUtils.isInfiltrator(id) == true) {
                        this.infil.killed += 1;
                        this.infil.uniqueKilled.add(kill.killedCharacterID);
                    } else if (LoadoutUtils.isLightAssault(id) == true) {
                        this.lightAssault.killed += 1;
                        this.lightAssault.uniqueKilled.add(kill.killedCharacterID);
                    } else if (LoadoutUtils.isMedic(id) == true) {
                        this.medic.killed += 1;
                        this.medic.uniqueKilled.add(kill.killedCharacterID);
                    } else if (LoadoutUtils.isEngineer(id) == true) {
                        this.engi.killed += 1;
                        this.engi.uniqueKilled.add(kill.killedCharacterID);
                    } else if (LoadoutUtils.isHeavy(id) == true) {
                        this.heavy.killed += 1;
                        this.heavy.uniqueKilled.add(kill.killedCharacterID);
                    } else if (LoadoutUtils.isMax(id) == true) {
                        this.max.killed += 1;
                        this.max.uniqueKilled.add(kill.killedCharacterID);
                    } else {
                        console.error(`ReportClassVersus> unchecked killedLoadoutID ${id}`);
                    }
                }

                for (const death of this.report.deaths) {
                    const id: number = death.attackerLoadoutID;

                    if (LoadoutUtils.isInfiltrator(id) == true) {
                        this.infil.deathsFrom += 1;
                        this.infil.uniqueDeaths.add(death.attackerCharacterID);
                    } else if (LoadoutUtils.isLightAssault(id) == true) {
                        this.lightAssault.deathsFrom += 1;
                        this.lightAssault.uniqueDeaths.add(death.attackerCharacterID);
                    } else if (LoadoutUtils.isMedic(id) == true) {
                        this.medic.deathsFrom += 1;
                        this.medic.uniqueDeaths.add(death.attackerCharacterID);
                    } else if (LoadoutUtils.isEngineer(id) == true) {
                        this.engi.deathsFrom += 1;
                        this.engi.uniqueDeaths.add(death.attackerCharacterID);
                    } else if (LoadoutUtils.isHeavy(id) == true) {
                        this.heavy.deathsFrom += 1;
                        this.heavy.uniqueDeaths.add(death.attackerCharacterID);
                    } else if (LoadoutUtils.isMax(id) == true) {
                        this.max.deathsFrom += 1;
                        this.max.uniqueDeaths.add(death.attackerCharacterID);
                    } else {
                        console.error(`ReportClassVersus> unchecked attackerLoadoutID ${id}`);
                    }
                }
            }

        },

        components: {
            InfoHover, Collapsible
        }
    });

    export default ReportClassVersus;
</script>