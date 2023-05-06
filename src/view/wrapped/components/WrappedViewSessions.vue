<template>
    <div>
        <collapsible header-text="Sessions">
            <div>
                <h3 class="wt-header" style="background-color: var(--red)">
                    Playtime per week
                </h3>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--purple)">
                    Session list
                </h3>

                <a-table :entries="sessions"
                         :paginate="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         class="border-top-0"
                >

                    <a-col sort-field="start">
                        <a-header>
                            Start
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.start | moment}}
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            Character
                        </a-header>

                        <a-body v-slot="entry">
                            <a :href="'/c/' + entry.characterID">
                                {{entry.character | characterName}}
                            </a>
                        </a-body>
                    </a-col>

                    <a-col sort-field="duration">
                        <a-header>
                            Duration
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.duration | mduration}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            Kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="deaths">
                        <a-header>
                            Deaths
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            K/D
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills / Math.max(1, entry.deaths) | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            KPM
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills / Math.max(1, entry.duration / 60) | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="exp">
                        <a-header>
                            Exp earned
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.exp | compact}}
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            SPM
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.exp / Math.max(1, entry.duration / 60) | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            Session link
                        </a-header>

                        <a-body v-slot="entry">
                            <a :href="'/s/' + entry.session.id">
                                {{entry.session.id}}
                            </a>
                        </a-body>
                    </a-col>

                </a-table>

            </div>
        </collapsible>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";
    import * as moment from "moment";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/CompactFilter";
    import "filters/CharacterName";

    // models
    import { WrappedSession } from "../common";

    class SessionWeekData {
        public weekStart: Date = new Date();
        public playtime: number = 0;
    }

    export const WrappedViewSessions = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                weekData: [] as SessionWeekData[]
            }
        },

        created: function(): void {
            this.makeAll();
        },

        methods: {

            makeAll: function(): void {
                this.makeWeekData();
            },

            makeWeekData: function(): void {
                const map: Map<string, SessionWeekData> = new Map();

                for (const session of this.wrapped.sessions) {
                    if (session.end == null) {
                        continue;
                    }

                    const m: moment.Moment = moment(session.start);
                    const key: string = moment(session.start).format("w");

                    let week: SessionWeekData | undefined = map.get(key);
                    if (week == undefined) {
                        week = new SessionWeekData();
                        week.weekStart = m.startOf("week").toDate();
                    }

                    week.playtime += (session.end.getTime() - session.start.getTime()) / 1000;
                }

                this.weekData = Array.from(map.values());
            },
        },

        computed: {
            sessions: function(): Loading<WrappedSession[]> {
                return Loadable.loaded(this.wrapped.extra.sessions);
            }
        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewSessions;
</script>