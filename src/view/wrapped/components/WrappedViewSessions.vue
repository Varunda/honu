<template>
    <div>
        <collapsible header-text="Sessions">
            <div class="mb-4">
                <h3 class="wt-header" style="background-color: var(--pink)">
                    Sessions per minute
                </h3>

                <div style="height: 500px; width: 100%;">
                    <canvas id="chart-time-per-day"></canvas>
                </div>
            </div>

            <div class="mb-4">
                <h3 class="wt-header" style="background-color: var(--pink)">
                    Play time per character per week
                </h3>

                <div style="height: 500px; width: 100%;">
                    <canvas id="chart-time-per-char"></canvas>
                </div>
            </div>

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--pink)">
                    Session list
                </h3>

                <div class="border mb-3 mt-0 rounded-bottom">
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
    import Chart from "chart.js/auto/auto.esm";
    import "node_modules/chartjs-adapter-moment/dist/chartjs-adapter-moment.esm.js";

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

    // util
    import ColorUtils from "util/Color";

    // models
    import { WrappedSession } from "../common";
    import TimeUtils from "util/Time";

    class SessionWeekData {
        public weekStart: Date = new Date();
        public playtime: number = 0;
    }

    // #chart-time-per-day

    export const WrappedViewSessions = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                weekData: [] as SessionWeekData[],

                showTable: true as boolean,

                chart: {
                    timePerDay: null as Chart | null,
                    timePerChar: null as Chart | null
                },

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

        mounted: function(): void {
            this.$nextTick(() => {
                this.makeAll();
            });
        },

        methods: {

            makeAll: function(): void {
                this.makeHourData();
                this.makeWeekData();
            },

            makeHourData: function(): void {
                const map: Map<number, number> = new Map();

                for (let i = 0; i < 60 * 24; i += 1) {
                    map.set(i, 0);
                }

                for (const session of this.wrapped.sessions) {
                    if (session.end == null) {
                        continue;
                    }

                    const start: moment.Moment = moment(session.start);
                    for (let ii = session.start.getTime(); ii < session.end.getTime(); ii += (1000 * 60)) {
                        const iter = start.add(1, "minute");

                        const h: number = iter.get("hour");
                        const m: number = iter.get("minute");

                        const v: number = h * 60 + m;

                        map.set(v, (map.get(v) ?? 0) + 1);
                    }
                }

                console.log(Array.from(map.values()));

                const canvas = document.getElementById("chart-time-per-day") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-time-per-day is null`);
                    return;
                }

                const sessionCount: number[] = Array.from(map.entries()).sort((a, b) => a[0] - b[0]).map(iter => iter[1]);

                if (this.chart.timePerDay != null) {
                    this.chart.timePerDay.destroy();
                    this.chart.timePerDay = null;
                }

                this.chart.timePerDay = new Chart(ctx, {
                    type: "bar", 
                    data: {
                        labels: sessionCount.map((v, index) => {
                            return TimeUtils.format(moment(this.wrapped.timestamp).startOf("day").utc().add(index, "minutes").toDate(), "hh:mm A");
                        }),
                        datasets: [{
                            data: sessionCount.map(v => v),
                            backgroundColor: "white",
                            barPercentage: 1,
                            categoryPercentage: 1
                        }]
                    },
                    options: {
                        scales: {
                            x: {
                                ticks: {
                                    color: "white",
                                    font: {
                                        family: "Consolas"
                                    }
                                }
                            }
                        }
                    }
                });
            },

            makeWeekData: function(): void {
                const map: Map<string, number[]> = new Map();

                this.wrapped.inputCharacterIDs.forEach((iter) => {
                    map.set(iter, [...Array(52)].map(_ => 0));
                });

                for (const session of this.wrapped.sessions) {
                    if (session.end == null) {
                        continue;
                    }

                    const durationSec: number = (session.end.getTime() - session.start.getTime()) / 1000;
                    const week: number = moment(session.start).utc().get("week") - 1;

                    console.log(`${session.start} ${week}`);

                    map.get(session.characterID)![week] += durationSec;
                }

                if (this.chart.timePerChar != null) {
                    this.chart.timePerChar.destroy();
                    this.chart.timePerChar = null;
                }

                const canvas = document.getElementById("chart-time-per-char") as HTMLCanvasElement;
                const ctx = canvas.getContext("2d");
                if (ctx == null) {
                    console.error(`context for #chart-time-per-char is null`);
                    return;
                }

                this.chart.timePerChar = new Chart(ctx, {
                    type: "line",
                    data: {
                        labels: [...Array(52)].map((iter, index) => `week ${index + 1}`),
                        datasets: this.wrapped.inputCharacterIDs.map((iter, index) => {
                            return {
                                label: this.wrapped.characters.get(iter)?.name ?? `<missing ${iter}>`,
                                data: (map.get(iter) ?? []),
                                borderColor: ColorUtils.randomColor(0.5, this.wrapped.inputCharacterIDs.length, index),
                                cubicInterpolationMode: "monotone"
                            };
                        })
                    },
                    options: {
                        scales: {
                            y: {
                                title: {
                                    text: "hours"
                                },
                            }
                        },
                        plugins: {
                            tooltip: {
                                mode: "index",
                                intersect: false,
                                callbacks: {
                                    label: function(ctx) {
                                        return ctx.dataset.label + ": " + TimeUtils.duration(ctx.raw as number);
                                    }
                                }
                            },
                        },
                        hover: {
                            mode: "index",
                            intersect: false
                        }
                    }
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