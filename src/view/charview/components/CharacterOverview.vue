<template>
    <div>

        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <hr class="border" />

        <table class="table table-sm w-auto">
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

        <table class="table table-sm w-auto" v-if="history.state == 'loaded'">
            <tr>
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

        <table class="table table-sm w-auto">
            <tr>
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
                <td>KPM</td>
                <td>{{recentKills / recentTime * 60 | fixed | locale}}</td>
            </tr>

            <tr>
                <td>K/D</td>
                <td>{{recentKills / (recentDeaths || 1) | fixed | locale}}</td>
            </tr>

            <tr>
                <td>Play time</td>
                <td>{{recentTime | mduration}}</td>
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

    import { PsCharacter } from "api/CharacterApi";
    import { CharacterHistoryStat, CharacterHistoryStatApi } from "api/CharacterHistoryStatApi";

    export const CharacterOverview = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                history: Loadable.idle() as Loading<CharacterHistoryStat[]>,
            }
        },

        beforeMount: function(): void {
            this.loadHistory();
        },

        methods: {
            loadHistory: async function(): Promise<void> {
                this.history = Loadable.loading();
                try {
                    this.history = Loadable.loaded(await CharacterHistoryStatApi.getByCharacterID(this.character.id));
                } catch (err: any) {
                    this.history = Loadable.error(err);
                }
            }

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
                return new Date().getMonth();
            },

            historyKills: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "kills") || null; },
            historyDeaths: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "deaths") || null; },
            historyScore: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "score") || null; },
            historyTime: function(): CharacterHistoryStat | null { return this.history.state != "loaded" ? null : this.history.data.find(iter => iter.type == "time") || null; },
        }

    });
    export default CharacterOverview;

    (window as any).CharacterHistoryStatApi = CharacterHistoryStatApi;
</script>
