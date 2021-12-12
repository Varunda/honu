<template>
    <div>
        <div class="btn-group w-100">
            <button @click="show.kills = !show.kills" type="button" class="btn" :class="[ show.kills == true ? 'btn-primary' : 'btn-secondary' ]">
                Show kills
            </button>

            <button @click="show.deaths = !show.deaths" type="button" class="btn" :class="[ show.deaths == true ? 'btn-primary' : 'btn-secondary' ]">
                Show deaths
            </button>

            <button @click="show.score = !show.score" type="button" class="btn" :class="[ show.score == true ? 'btn-primary' : 'btn-secondary' ]">
                Show score
            </button>

            <button @click="show.certs = !show.certs" type="button" class="btn" :class="[ show.certs == true ? 'btn-primary' : 'btn-secondary' ]">
                Show certs
            </button>

            <button @click="show.time = !show.time" type="button" class="btn" :class="[ show.time == true ? 'btn-primary' : 'btn-secondary' ]">
                Show time
            </button>

            <button @click="show.kd = !show.kd" type="button" class="btn" :class="[ show.kd == true ? 'btn-primary' : 'btn-secondary' ]">
                Show KD
            </button>

            <button @click="show.kpm = !show.kpm" type="button" class="btn" :class="[ show.kpm == true ? 'btn-primary' : 'btn-secondary' ]">
                Show KPM
            </button>

            <button @click="show.spm = !show.spm" type="button" class="btn" :class="[ show.spm == true ? 'btn-primary' : 'btn-secondary' ]">
                Show SPM
            </button>

            <button @click="show.cpm = !show.cpm" type="button" class="btn" :class="[ show.cpm == true ? 'btn-primary' : 'btn-secondary' ]">
                Show CPM
            </button>
        </div>

        <div class="btn-group w-100 mt-2">
            <button @click="setScale('days')" type="button" class="btn" :class="[ scale == 'days' ? 'btn-primary' : 'btn-secondary' ]">
                Days
            </button>

            <button @click="setScale('weeks')" type="button" class="btn" :class="[ scale == 'weeks' ? 'btn-primary' : 'btn-secondary' ]">
                Weeks
            </button>

            <button @click="setScale('months')" type="button" class="btn" :class="[ scale == 'months' ? 'btn-primary' : 'btn-secondary' ]">
                Months
            </button>
        </div>

        <hr class="border" />

        <chart-history-stat v-if="show.kills == true && kills != null" class="border-bottom"
            :data="data.kills" :period="scale" :timestamp="kills.lastUpdated" title="Kills">
        </chart-history-stat>

        <chart-history-stat v-if="show.deaths == true && deaths != null" class="border-bottom"
            :data="data.deaths" :period="scale" :timestamp="deaths.lastUpdated" title="Deaths">
        </chart-history-stat>

        <chart-history-stat v-if="show.score == true && score != null" class="border-bottom"
            :data="data.score" :period="scale" :timestamp="score.lastUpdated" title="Score">
        </chart-history-stat>

        <chart-history-stat v-if="show.certs == true && certs != null" class="border-bottom"
            :data="data.certs" :period="scale" :timestamp="certs.lastUpdated" title="Certs">
        </chart-history-stat>

        <chart-history-stat v-if="show.time == true && time != null" class="border-bottom"
            :data="data.time" :period="scale" :timestamp="time.lastUpdated" title="Time played">
        </chart-history-stat>

        <chart-history-stat v-if="show.kd == true && kills != null && deaths != null" class="border-bottom"
            :data="data.kd" :period="scale" :timestamp="kills.lastUpdated" title="KD">
        </chart-history-stat>

        <chart-history-stat v-if="show.kpm == true && kills != null && time != null" class="border-bottom"
            :data="data.kpm" :period="scale" :timestamp="kills.lastUpdated" title="KPM">
        </chart-history-stat>

        <chart-history-stat v-if="show.spm == true && score != null && time != null" class="border-bottom"
            :data="data.spm" :period="scale" :timestamp="score.lastUpdated" title="Score per minute">
        </chart-history-stat>

        <chart-history-stat v-if="show.cpm == true && certs != null && time != null" class="border-bottom"
            :data="data.cpm" :period="scale" :timestamp="certs.lastUpdated" title="Certs per minute">
        </chart-history-stat>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { CharacterHistoryStat } from "api/CharacterHistoryStatApi";

    import ChartHistoryStat from "./ChartHistoryStat.vue";

    export const CharacterHistoryStats = Vue.extend({
        props: {
            stats: { type: Array as PropType<CharacterHistoryStat[]>, required: true },
        },

        data: function() {
            return {
                show: {
                    kills: false as boolean,
                    deaths: false as boolean,
                    score: false as boolean,
                    certs: false as boolean,
                    time: true as boolean,

                    kd: true as boolean,

                    kpm: true as boolean,
                    spm: true as boolean,
                    cpm: true as boolean,
                },

                scale: "weeks" as "days" | "weeks" | "months",

                data: {
                    kills: [] as number[],
                    deaths: [] as number[],
                    score: [] as number[],
                    certs: [] as number[],
                    time: [] as number[],

                    kd: [] as number[],

                    spm: [] as number[],
                    kpm: [] as number[],
                    cpm: [] as number[],
                }
            }
        },

        mounted: function(): void {
            this.bind();
        },

        methods: {
            bind: function(): void {
                this.data.kills = this.getDataset(this.kills);
                this.data.deaths = this.getDataset(this.deaths);
                this.data.score = this.getDataset(this.score);
                this.data.certs = this.getDataset(this.certs);
                this.data.time = this.getDataset(this.time);

                this.data.kd = this.combineDataset(this.data.kills, this.data.deaths, 1, 0);

                this.data.kpm = this.combineDataset(this.data.kills, this.data.time, 60);
                this.data.spm = this.combineDataset(this.data.score, this.data.time, 60);
                this.data.cpm = this.combineDataset(this.data.certs, this.data.time, 60);
            },

            setScale: function(scale: string): void {
                if (scale == "days" || scale == "weeks" || scale == "months") {
                    this.scale = scale;
                }

                this.bind();
            },

            getDataset: function(stat: CharacterHistoryStat | null): number[] {
                if (stat == null) {
                    return [];
                }

                if (this.scale == "days") {
                    return stat.days;
                } else if (this.scale == "weeks") {
                    return stat.weeks;
                } else if (this.scale == "months") {
                    return stat.months;
                }
                throw `Unchecked value of this.scale: '${this.scale}'. Expected 'days' | 'weeks' | 'months'`;
            },

            combineDataset: function(setA: number[], setB: number[], scale: number = 1, min: number = 120): number[] {
                const len: number = Math.min(setA.length, setB.length);

                const ratio: number[] = [];

                for (let i = 0; i < len; ++i) {
                    let a = setA[i];
                    let b = setB[i];
                    if (b <= min) {
                        a = 0;
                        b = 0;
                    }

                    ratio.push(a / Math.max(1, b) * scale);
                }

                return ratio;
            }
        },

        computed: {
            kills: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "kills") || null;
            },

            deaths: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "deaths") || null;
            },

            score: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "score") || null;
            },

            time: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "time") || null;
            },

            battleRank: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "battle_rank") || null;
            },

            certs: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "certs") || null;
            },

            medals: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "medals") || null;
            },

            ribbons: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "ribbons") || null;
            },

            facilityCaptures: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "facility_capture") || null;
            },

            facilityDefends: function(): CharacterHistoryStat | null {
                return this.stats.find(iter => iter.type == "facility_defend") || null;
            },
        },

        components: {
            ChartHistoryStat,
        }
    });
    export default CharacterHistoryStats;
</script>