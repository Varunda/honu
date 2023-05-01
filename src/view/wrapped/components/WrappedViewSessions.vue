<template>
    <div>
        <collapsible header-text="Sessions">
            <div>
                <h3 class="wt-header" style="background-color: var(--red)">
                    Playtime per week
                </h3>

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

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewSessions;
</script>