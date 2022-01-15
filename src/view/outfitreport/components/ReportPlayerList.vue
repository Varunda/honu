<template>
    <div>
        <h2 class="wt-header" data-toggle="collapse" data-target="#report-player-list">
            Player list
        </h2>

        <div id="report-player-list" class="collapse show">


            <a-table 
                :entries="metadata"
                default-sort-field="name" default-sort-order="asc"
                :paginate="false" :striped="true"
                display-type="table" row-padding="compact">

                <a-col sort-field="name">
                    <a-header>
                        <b>Player</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <a :href="'/c/' + entry.id">
                            <span v-if="entry.outfitTag != null">
                                [{{entry.outfitTag}}]
                            </span>
                            {{entry.name}}
                        </a>
                    </a-body>
                </a-col>

                <a-col sort-field="mostPlayedClass">
                    <a-header>
                        <b>Most played class</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.mostPlayedClass}}
                    </a-body>
                </a-col>

                <a-col sort-field="playtime">
                    <a-header>
                        <b>Playtime</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.playtime | mduration}}
                    </a-body>
                </a-col>

                <a-col sort-field="infilPlaytime">
                    <a-header>
                        <b>Infil</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span :class="{ 'text-muted': entry.infilPlaytime < 60 }">
                            {{entry.infilPlaytime | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="lightAssaultPlaytime">
                    <a-header>
                        <b>Light Assault</b>
                    </a-header>

                    <a-body v-slot="entry" class="text-danger">
                        <span :class="{ 'text-muted': entry.lightAssaultPlaytime < 60 }">
                            {{entry.lightAssaultPlaytime | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="medicPlaytime">
                    <a-header>
                        <b>Medic</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span :class="{ 'text-muted': entry.medicPlaytime < 60 }">
                            {{entry.medicPlaytime | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="engineerPlaytime">
                    <a-header>
                        <b>Engineer</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span :class="{ 'text-muted': entry.engineerPlaytime < 60 }">
                            {{entry.engineerPlaytime | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="heavyPlaytime">
                    <a-header>
                        <b>Heavy</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span :class="{ 'text-muted': entry.heavyPlaytime < 60 }">
                            {{entry.heavyPlaytime | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="maxPlaytime">
                    <a-header>
                        <b>MAX</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span :class="{ 'text-muted': entry.maxPlaytime < 60 }">
                            {{entry.maxPlaytime | mduration}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <b>Sessions</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <a v-for="session in entry.sessions" :key="session.id" :href="'/s/' + session.id">
                            {{session.id}}
                        </a>
                    </a-body>
                </a-col>

            </a-table>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";
    import Report, { PlayerMetadata } from "../Report";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import "MomentFilter";

    import { Session } from "/api/SessionApi";

    class FlatPlayerMetadata {
        public id: string = "";
        public name: string = "";
        public outfitTag: string | null = null;
        public mostPlayedClass: string = "";
        public playtime: number = 0;

        public infilPlaytime: number = 0;
        public lightAssaultPlaytime: number = 0;
        public medicPlaytime: number = 0;
        public engineerPlaytime: number = 0;
        public heavyPlaytime: number = 0;
        public maxPlaytime: number = 0;

        public sessions: Session[] = [];
    }

    export const ReportPlayerList = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        created: function(): void {
            this.metadata = Loadable.loaded(
                Array.from(this.report.playerMetadata.values())
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .map((iter: PlayerMetadata): FlatPlayerMetadata => {
                        console.log(iter);
                        return {
                            id: iter.ID,
                            name: iter.name,
                            outfitTag: iter.outfitTag,
                            mostPlayedClass: iter.classes.mostPlayed.name,
                            playtime: iter.timeAs,

                            infilPlaytime: iter.classes.infil.timeAs,
                            lightAssaultPlaytime: iter.classes.lightAssault.timeAs,
                            medicPlaytime: iter.classes.medic.timeAs,
                            engineerPlaytime: iter.classes.engineer.timeAs,
                            heavyPlaytime: iter.classes.heavy.timeAs,
                            maxPlaytime: iter.classes.max.timeAs,

                            sessions: iter.sessions
                        };
                    })
            );
        },

        data: function() {
            return {
                metadata: Loadable.idle() as Loading<FlatPlayerMetadata[]>,
            }
        },

        methods: {

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
        }
    });

    export default ReportPlayerList;
</script>