<template>
    <div>

        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <hr class="border" />

        <div class="d-flex">
            <table class="table table-sm w-auto d-inline-block mr-5">
                <tr class="table-secondary">
                    <td colspan="2"><b>General</b></td>
                </tr>

                <tr>
                    <td>Outfit</td>
                    <td>
                        <span v-if="character.outfitID == null">
                            No outfit
                        </span>
                        <a v-else :href="'/o/' + character.outfitID">
                            [{{character.outfitTag}}] {{character.outfitName}}
                        </a>
                    </td>
                </tr>

                <tr>
                    <td>Battle rank</td>
                    <td>
                        <span v-if="character.prestige == true" title="A.S.P">1~</span>{{character.battleRank}}
                    </td>
                </tr>
            </table>

            <table class="table table-sm w-auto d-inline-block mr-2" v-if="history.state == 'loaded'">
                <tr class="table-secondary">
                    <td colspan="2"><b>Lifetime stats</b></td>
                </tr>

                <tr>
                    <td>Kills</td>
                    <td>{{historyKills.allTime | locale}}</td>
                </tr>

                <tr>
                    <td>Deaths</td>
                    <td>{{historyDeaths.allTime | locale}}</td>
                </tr>

                <tr>
                    <td>Play time</td>
                    <td>{{historyTime.allTime | mduration}}</td>
                </tr>

                <tr>
                    <td>KPM</td>
                    <td>{{historyKills.allTime / historyTime.allTime * 60 | fixed | locale}}</td>
                </tr>

                <tr>
                    <td>K/D</td>
                    <td>{{historyKills.allTime / (historyDeaths.allTime || 1) | fixed | locale}}</td>
                </tr>

                <tr>
                    <td>Score</td>
                    <td>{{historyScore.allTime | locale}}</td>
                </tr>

                <tr>
                    <td>SPM</td>
                    <td>{{historyScore.allTime / historyTime.allTime * 60 | fixed | locale}}</td>
                </tr>
            </table>

            <character-class-stats v-if="stats.state == 'loaded'" class="mr-5"
                :data="stats.data" type="forever" title="All time" :include-metadata="true">
            </character-class-stats>

            <table v-if="history.state == 'loaded'" class="table table-sm w-auto d-inline-block mr-2">
                <tr class="table-secondary">
                    <td colspan="2"><b>30 day stats</b></td>
                </tr>

                <tr>
                    <td>Kills</td>
                    <td>{{recentKills | locale}}</td>
                </tr>

                <tr>
                    <td>Deaths</td>
                    <td>{{recentDeaths | locale}}</td>
                </tr>

                <tr>
                    <td>Play time</td>
                    <td>{{recentTime | mduration}}</td>
                </tr>

                <tr>
                    <td>KPM</td>
                    <td>{{recentKills / recentTime * 60 | fixed | locale}}</td>
                </tr>

                <tr>
                    <td>K/D</td>
                    <td>{{recentKills / (recentDeaths || 1) | fixed | locale}}</td>
                </tr>

                <tr>
                    <td>Score</td>
                    <td>{{recentScore | locale}}</td>
                </tr>

                <tr>
                    <td>SPM</td>
                    <td>{{recentScore / recentTime * 60 | fixed | locale}}</td>
                </tr>
            </table>

            <character-class-stats v-if="stats.state == 'loaded'"
                :data="stats.data" type="monthly" title="This month" :include-metadata="false">
            </character-class-stats>
        </div>

        <h2 class="wt-header">History stats</h2>

        <chart-history-stat v-if="historyKills != null"
            :data="historyKills.days" period="days" title="Kills" :timestamp="historyKills.lastUpdated">
        </chart-history-stat>

        <chart-history-stat v-if="historyKills != null"
            :data="perDayKD" period="days" title="KD" :timestamp="historyKills.lastUpdated">
        </chart-history-stat>

        <chart-history-stat v-if="historyKills != null"
            :data="perDayKPM" period="days" title="KPM" :timestamp="historyKills.lastUpdated">
        </chart-history-stat>

        <chart-history-stat v-if="historyTime != null"
            :data="historyTime.days" period="days" title="Time played" :timestamp="historyTime.lastUpdated">
        </chart-history-stat>

        <chart-history-stat v-if="historyScore != null"
            :data="historyScore.days" period="days" title="Score" :timestamp="historyScore.lastUpdated">
        </chart-history-stat>

        <hr class="border" />

        <h4>Census links</h4>

        <table class="table table-sm w-auto">
            <tbody>
                <tr>
                    <td><b>Character ID</b></td>
                    <td>{{character.id}}</td>
                    <td>
                        <a :href="'https://census.daybreakgames.com/s:example/get/ps2:v2/character?character_id=' + character.id" target="_blank">
                            Census
                            <span class="fas fa-external-link-alt"></span>
                        </a>
                    </td>
                </tr>

                <tr v-if="character.outfitID != null">
                    <td><b>Outfit ID</b></td>
                    <td>{{character.outfitID}}</td>
                    <td>
                        <a :href="'https://census.daybreakgames.com/s:example/get/ps2:v2/outfit?outfit_id=' + character.outfitID" target="_blank">
                            Census
                            <span class="fas fa-external-link-alt"></span>
                        </a>
                    </td>
                </tr>
            </tbody>
        </table>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "filters/LocaleFilter";
    import "filters/FixedFilter";
    import "MomentFilter";

    import { PsCharacter } from "api/CharacterApi";
    import { CharacterHistoryStat, CharacterHistoryStatApi } from "api/CharacterHistoryStatApi";
    import { CharacterStat, CharacterStatApi } from "api/CharacterStatApi";

    import ChartHistoryStat from "./ChartHistoryStat.vue";
    import CharacterClassStats from "./CharacterClassStats.vue";

    export const CharacterOverview = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                history: Loadable.idle() as Loading<CharacterHistoryStat[]>,
                stats: Loadable.idle() as Loading<CharacterStat[]>,
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.loadHistory();
                this.loadStats();
            });
        },

        methods: {
            loadHistory: async function(): Promise<void> {
                this.history = Loadable.loading();
                try {
                    this.history = Loadable.loaded(await CharacterHistoryStatApi.getByCharacterID(this.character.id));
                } catch (err: any) {
                    this.history = Loadable.error(err);
                }
            },

            loadStats: async function(): Promise<void> {
                this.stats = Loadable.loading();
                this.stats = await Loadable.promise(CharacterStatApi.getByCharacterID(this.character.id));
            },
        },

        computed: {
            recentKills: function(): number {
                return this.historyKills == null ? -1 : this.historyKills.days.reduce((a, b) => a + b, 0);
            },

            recentDeaths: function(): number {
                return this.historyDeaths == null ? -1 : this.historyDeaths.days.reduce((a, b) => a + b, 0);
            },

            recentTime: function(): number {
                return this.historyTime == null ? -1 : this.historyTime.days.reduce((a, b) => a + b, 0);
            },

            recentScore: function(): number {
                return this.historyScore == null ? -1 : this.historyScore.days.reduce((a, b) => a + b, 0);
            },

            currentMonth: function(): number {
                return new Date().getMonth(); // 0 indexed
            },

            perDayKD: function(): number[] {
                const kills: CharacterHistoryStat | null = this.historyKills;
                const deaths: CharacterHistoryStat | null = this.historyDeaths;

                if (kills == null || deaths == null) {
                    return [];
                }

                const k: number[] = kills.days;
                const d: number[] = deaths.days;

                const len: number = Math.min(k.length, d.length);

                const kd: number[] = [];

                for (let i = 0; i < len; ++i) {
                    kd.push(k[i] / Math.max(1, d[i]));
                }

                return kd;
            },

            perDayKPM: function(): number[] {
                const kills: CharacterHistoryStat | null = this.historyKills;
                const time: CharacterHistoryStat | null = this.historyTime;

                if (kills == null || time == null) {
                    return [];
                }

                const k: number[] = kills.days;
                const t: number[] = time.days;

                const len: number = Math.min(k.length, t.length);

                const kpm: number[] = [];

                for (let i = 0; i < len; ++i) {
                    kpm.push(k[i] / Math.max(1, t[i]) * 60);
                }

                return kpm;
            },

            historyKills: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "kills") || null; },
            historyDeaths: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "deaths") || null; },
            historyScore: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "score") || null; },
            historyTime: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "time") || null; },
        },

        components: {
            ChartHistoryStat,
            CharacterClassStats,
        }

    });
    export default CharacterOverview;
</script>
