<template>
    <div>
        <div v-if="killboard.state == 'loading' && simpleKillboard.state == 'loaded'" class="alert alert-info text-center">
            <busy class="honu-busy-sm"></busy>
            Stats have not finished loading! Only showing basic character information
        </div>

        <div class="mb-2">
            <toggle-button v-model="options.useRecent">
                Use recent stats
                <info-hover text="Use the 30 days stats or all time"></info-hover>
            </toggle-button>

            <toggle-button v-model="options.showRelative">
                Show relative KD diff
                <info-hover text="Include an indicator if your KD against a character is higher/lower than average"></info-hover>
            </toggle-button>
        </div>

        <div>
            <a-table 
                :entries="data"
                :striped="true"
                default-sort-field="kills" default-sort-order="desc"
                display-type="table">

                <a-col>
                    <a-header>
                        <b>Character</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <a :href="'/c/' + entry.characterID">
                            <span v-if="entry.character != null">
                                {{entry.character | characterName}}
                            </span>
                            <span v-else>
                                {{entry.characterName}}
                            </span>
                        </a>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <b>Faction</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <faction :faction-id="entry.characterFaction"></faction>
                    </a-body>
                </a-col>

                <a-col sort-field="kills">
                    <a-header>
                        <b>Kills</b>
                        <info-hover text="How many times the character being viewed has killed this character"></info-hover>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.kills | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="deaths">
                    <a-header>
                        <b>Deaths</b>
                        <info-hover text="How many times the character being viewed has died to this character"></info-hover>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.deaths | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="kd">
                    <a-header>
                        <b>Your K/D</b>
                        <info-hover text="This is non-rez KD, which is different from what honu usually displays!"></info-hover>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.kd | locale(2)}}

                        <span v-if="options.showRelative == true && entry.recentCharacterKd != null">
                            <template v-if="entry.recentCharacterKd == 0">
                                <span class="text-secondary fa-fw fas fa-question border rounded border-secondary"
                                      title="This character has a KD of 0, no useful info had here"></span>
                            </template>

                            <template v-else-if="Math.abs(entry.recentCharacterKd - entry.kdInverse) < 0.1">
                                <span class="text-info fa-fw fas fa-equals border rounded border-info"
                                      title="Your KD is equal to the average against this character (within 0.1)"></span>
                            </template>

                            <template v-else-if="entry.kdInverse > entry.recentCharacterKd">
                                <span class="text-danger fa-fw fas fa-caret-down border rounded border-danger"
                                      title="Your KD is lower than the average against this character"></span>
                            </template>

                            <template v-else-if="entry.kdInverse < entry.recentCharacterKd">
                                <span class="text-success fa-fw fas fa-caret-up border rounded border-success"
                                      title="Your KD is higher than the average against this character"></span>
                            </template>
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="lastLogin">
                    <a-header>
                        <b>Last login</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.lastLogin | moment}}
                        ({{entry.lastLogin | timeAgo}})
                    </a-body>
                </a-col>

                <a-col sort-field="recentCharacterKd">
                    <a-header>
                        <b>Character K/D</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.recentCharacterKd == null">
                            --
                        </span>
                        <span v-else>
                            {{entry.recentCharacterKd | locale(2)}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="recentCharacterKpm">
                    <a-header>
                        <b>Character KPM</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.recentCharacterKpm == null">
                            --
                        </span>
                        <span v-else>
                            {{entry.recentCharacterKpm | locale(2)}}
                        </span>
                    </a-body>
                </a-col>

            </a-table>
        </div>

        <hr class="border" />

        <div class="d-flex mb-2">
            <div class="flex-grow-1"></div>

            <div class="text-center">
                This data uses Planetside 2&rsquo;s API (Census), which only contains data AFTER November 2019.
                If these numbers seem low, it's because those kills were gained before November 2019.
                <br />
                If the data does not load, it is most likely because Census is being laggy right now
            </div>

            <div class="flex-grow-1"></div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ToggleButton from "components/ToggleButton";
    import Faction from "components/Faction";
    import FactionImage from "components/FactionImage";

    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/FixedFilter";
    import "filters/FactionNameFilter";
    import "filters/CharacterName";

    import { PsCharacter } from "api/CharacterApi";
    import { ExpandedKillboardEntry, KillboardApi } from "api/KillboardApi";
    import { CharacterHistoryStat, CharacterHistoryStatApi } from "api/CharacterHistoryStatApi";

    type KillboardTableEntry = {
        character: PsCharacter | null;

        characterID: string;
        characterName: string;
        characterFaction: number;
        lastLogin: Date;

        kills: number;
        deaths: number;
        kd: number;
        kdInverse: number;

        recentCharacterKd: number | null;
        recentCharacterKpm: number | null;
        recentCharacterSpm: number | null;
        recentCharacterKills: number | null;
        recentCharacterDeaths: number | null;
    }

    export const CharacterKillboard = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                killboard: Loadable.idle() as Loading<ExpandedKillboardEntry[]>,
                simpleKillboard: Loadable.idle() as Loading<ExpandedKillboardEntry[]>,

                options: {
                    useRecent: true as boolean,
                    showRelative: false as boolean
                }
            }
        },

        created: function(): void {
            this.bind();
        },

        methods: {
            bind: async function(): Promise<void> {
                this.simpleKillboard = Loadable.loading();
                // don't await this, start the main loading too
                KillboardApi.getExpandedByCharacterID(this.character.id, false).then((data: Loading<ExpandedKillboardEntry[]>) => {
                    this.simpleKillboard = data;
                });

                this.killboard = Loadable.loading();
                this.killboard = await KillboardApi.getExpandedByCharacterID(this.character.id, true);
            }
        },

        computed: {

            data: function(): Loading<KillboardTableEntry[]> {

                let data: ExpandedKillboardEntry[] = [];

                if (this.killboard.state == "loaded") {
                    data = this.killboard.data;
                } else if (this.simpleKillboard.state == "loaded") {
                    data = this.simpleKillboard.data;
                } else {
                    return Loadable.rewrap(this.killboard);
                }

                return Loadable.loaded(data.map((iter: ExpandedKillboardEntry) => {

                    // inverse KD is useful for figuring out if you're above or below the average KD against a character
                    // if you have a KD of 0.5 against a character (1 kill, 2 deaths), while the other character has an average of 2KD (2 kills, 1 death)
                    // then if you were that other character, your kills//deaths swap, so you'd have 2 kills and 1 death against yourself, meaning you're average

                    const elem: KillboardTableEntry = {
                        character: iter.character,

                        characterID: iter.entry.otherCharacterID,
                        characterName: iter.character?.name ?? `<missing ${iter.entry.otherCharacterID}>`,
                        characterFaction: iter.character?.factionID ?? 0,
                        lastLogin: iter.character?.dateLastLogin ?? new Date(),

                        kills: iter.entry.kills,
                        deaths: iter.entry.deaths,
                        kd: iter.entry.kills / Math.max(1, iter.entry.deaths),
                        kdInverse: iter.entry.deaths / Math.max(1, iter.entry.kills),

                        recentCharacterKd: null,
                        recentCharacterKpm: null,
                        recentCharacterSpm: null,
                        recentCharacterKills: null,
                        recentCharacterDeaths: null
                    };

                    if (iter.stats != null && iter.stats.length > 0) {
                        const kills: CharacterHistoryStat | null = iter.stats.find(i => i.type == "kills") || null;
                        const deaths: CharacterHistoryStat | null = iter.stats.find(i => i.type == "deaths") || null;
                        const score: CharacterHistoryStat | null = iter.stats.find(i => i.type == "score") || null;
                        const time: CharacterHistoryStat | null = iter.stats.find(i => i.type == "time") || null;

                        function reduce(arr: number[]): number {
                            return arr.reduce((acc, iter) => acc += iter, 0);
                        };

                        if (kills != null) {
                            if (this.options.useRecent == true) {
                                elem.recentCharacterKills = reduce(kills.days);
                            } else {
                                elem.recentCharacterKills = kills.allTime;
                            }
                        }

                        if (deaths != null) {
                            if (this.options.useRecent == true) {
                                elem.recentCharacterDeaths = reduce(deaths.days);
                            } else {
                                elem.recentCharacterDeaths = deaths.allTime;
                            }
                        }

                        if (kills != null && deaths != null) {
                            if (this.options.useRecent == true) {
                                elem.recentCharacterKd = reduce(kills.days) / Math.max(1, reduce(deaths.days));
                            } else {
                                elem.recentCharacterKd = kills.allTime / Math.max(1, deaths.allTime);
                            }
                        }
                        if (kills != null && time != null) {
                            if (this.options.useRecent == true) {
                                elem.recentCharacterKpm = reduce(kills.days) / Math.max(1, reduce(time.days)) * 60;
                            } else {
                                elem.recentCharacterKpm = kills.allTime / Math.max(1, time.allTime) * 60;
                            }
                        }
                        if (score != null && time != null) {
                            if (this.options.useRecent == true) {
                                elem.recentCharacterSpm = reduce(score.days) / Math.max(1, reduce(time.days)) * 60;
                            } else {
                                elem.recentCharacterSpm = score.allTime / Math.max(1, time.allTime) * 60;
                            }
                        }
                    }

                    return elem;
                }));
            }

        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            Busy, InfoHover, ToggleButton, Faction, FactionImage
        }
    });
    export default CharacterKillboard;
</script>