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

                <div class="border rounded my-3">
                    <h4 class="ml-3 my-2">
                        Column selection
                    </h4>

                    <div class="w-100 btn-group">
                        <toggle-button v-model="columns.kills">
                            Kills
                        </toggle-button>

                        <toggle-button v-model="columns.deaths">
                            Deaths
                        </toggle-button>

                        <toggle-button v-model="columns.kd">
                            K/D
                        </toggle-button>

                        <toggle-button v-model="columns.kpm">
                            KPM
                        </toggle-button>

                        <toggle-button v-model="columns.expEarned">
                            Exp earned
                        </toggle-button>

                        <toggle-button v-model="columns.spm">
                            SPM
                            <info-hover text="Score per minute"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.vkills">
                            Vehicle kills
                        </toggle-button>

                        <toggle-button v-model="columns.vkpm">
                            V.Kills/Min
                        </toggle-button>

                        <toggle-button v-model="columns.heals">
                            Heals
                        </toggle-button>

                        <toggle-button v-model="columns.healsPerMinute">
                            Heals/Min
                        </toggle-button>

                        <toggle-button v-model="columns.revives">
                            Revives
                        </toggle-button>

                        <toggle-button v-model="columns.revivesPerMinute">
                            Revive/Min
                        </toggle-button>
                    </div>
                </div>

                <a-table v-if="showTable" :entries="sessions"
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

                    <a-col v-if="columns.kills" sort-field="kills">
                        <a-header>
                            Kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.deaths" sort-field="deaths">
                        <a-header>
                            Deaths
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.kd" sort-field="kd">
                        <a-header>
                            K/D
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kd | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.kpm" sort-field="kpm">
                        <a-header>
                            KPM
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kpm | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.expEarned" sort-field="exp">
                        <a-header>
                            Exp earned
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.exp | compact}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.spm" sort-field="spm">
                        <a-header>
                            SPM
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.spm | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.vkills" sort-field="vehicleKills">
                        <a-header>
                            V.Kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.vehicleKills | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.vkpm" sort-field="vkpm">
                        <a-header>
                            V.Kills/Min
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.vkpm | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.heals" sort-field="heals">
                        <a-header>
                            Heals
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.heals | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.healsPerMinute" sort-field="hpm">
                        <a-header>
                            Heals/Min
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.hpm | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.revives" sort-field="revives">
                        <a-header>
                            Revives
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.revives | locale(0)}}
                        </a-body>
                    </a-col>

                    <a-col v-if="columns.revivesPerMinute" sort-field="rpm">
                        <a-header>
                            Revives/Min
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.rpm | locale(2)}}
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
    import ToggleButton from "components/ToggleButton";

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
                weekData: [] as SessionWeekData[],

                showTable: true as boolean,

                columns: {
                    kills: true as boolean,
                    deaths: true as boolean,
                    kd: true as boolean,
                    kpm: true as boolean,

                    vkills: false as boolean,
                    vkpm: false as boolean,

                    expEarned: true as boolean,
                    spm: true as boolean,

                    heals: false as boolean,
                    revives: false as boolean,
                    maxRepairs: false as boolean,
                    revivesPerMinute: false as boolean,
                    healsPerMinute: false as boolean,
                }
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
                    map.set(key, week);
                }

                this.weekData = Array.from(map.values()).sort((a, b) => {
                    return a.weekStart.getTime() - b.weekStart.getTime();
                });

                console.log(`got ${this.weekData.length} from ${this.wrapped.sessions.length} sessions`);
            },
        },

        computed: {
            sessions: function(): Loading<WrappedSession[]> {
                return Loadable.loaded(this.wrapped.extra.sessions);
            }
        },

        watch: {

            columns: {
                deep: true,
                handler: async function(): Promise<void> {
                    // force the table to be destroyed then re-created
                    this.showTable = false;
                    await this.$nextTick();
                    this.showTable = true;
                }
                
            }

        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
            ToggleButton
        }

    });
    export default WrappedViewSessions;
</script>