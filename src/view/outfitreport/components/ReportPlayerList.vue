<template>
    <div>
        <h2 class="wt-header">
            Player list
        </h2>

        <table class="table table-sm table-striped">
            <tr class="table-secondary">
                <td>Character</td>
                <td>Most played class</td>
                <td>Playtime</td>
                <td>Infil</td>
                <td>Light assault</td>
                <td>Medic</td>
                <td>Engineer</td>
                <td>Heavy</td>
                <td>Max</td>
                <td>Sessions</td>
            </tr>

            <tr v-for="meta in metadata">
                <td>
                    <a :href="'/c/' + meta.ID">
                        {{meta.name}}
                    </a>
                </td>
                <td>{{meta.classes.mostPlayed.name}}</td>
                <td>{{meta.timeAs | mduration}}</td>
                <td>{{meta.classes.infil.timeAs | mduration}}</td>
                <td>{{meta.classes.lightAssault.timeAs | mduration}}</td>
                <td>{{meta.classes.medic.timeAs | mduration}}</td>
                <td>{{meta.classes.engineer.timeAs | mduration}}</td>
                <td>{{meta.classes.heavy.timeAs | mduration}}</td>
                <td>{{meta.classes.max.timeAs | mduration}}</td>
                <td>
                    <a v-for="session in meta.sessions" :key="session.id" :href="'/s/' + session.id">
                        {{session.id}}
                    </a>
                </td>
            </tr>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report, { PlayerMetadata } from "../Report";

    import "MomentFilter";

    export const ReportPlayerList = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        created: function(): void {
            this.metadata = Array.from(this.report.playerMetadata.values()).sort((a, b) => a.name.localeCompare(b.name));
        },

        data: function() {
            return {
                metadata: [] as PlayerMetadata[]
            }
        },

        methods: {

        }
    });

    export default ReportPlayerList;
</script>