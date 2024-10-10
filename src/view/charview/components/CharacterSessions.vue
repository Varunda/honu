<template>
    <div>
        <div class="border mb-3 mt-0 rounded" @click="showChart = true">
            <collapsible header-text="Graphed session stats" size-class="h4" class="mb-3" :show="false">
                <div v-if="showChart == true">
                    <session-chart v-if="block.state == 'loaded'" :entries="data.data"></session-chart>
                </div>
            </collapsible>
        </div>

        <div class="border mb-3 mt-0 rounded">
            <collapsible header-text="Column selection" size-class="h4" class="mb-0">
                <div class="pl-3">
                    <h4 class="mb-1">
                        General
                        <button class="btn btn-sm btn-secondary btn-outline-light" @click="resetShow" title="Reset shown columns">Reset</button>
                    </h4>

                    <div class="btn-group mb-3">
                        <toggle-button v-model="columns.exp">
                            Exp earned
                        </toggle-button>

                        <toggle-button v-model="columns.expPerMinute">
                            Exp/Min
                        </toggle-button>

                        <toggle-button v-model="columns.kills">
                            Kills
                        </toggle-button>

                        <toggle-button v-model="columns.deaths">
                            Deaths
                            <info-hover text="Revives remove a death"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.kd">
                            K/D
                            <info-hover text="Revives remove a death"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.kpm">
                            KPM
                        </toggle-button>

                        <toggle-button v-model="columns.vkills">
                            Vehicle kills
                        </toggle-button>

                        <toggle-button v-model="columns.vkpm">
                            V.Kills/Min
                        </toggle-button>

                        <toggle-button v-model="columns.spawns">
                            Spawns
                        </toggle-button>

                        <toggle-button v-model="columns.spawnsPerMinute">
                            Spawns/Min
                        </toggle-button>
                    </div>

                    <h4 class="mb-1">
                        <img src="/img/classes/icon_medic.png" height="20" />
                        <span class="align-bottom">Medic</span>
                    </h4>

                    <div class="btn-group mb-3">
                        <toggle-button v-model="columns.heals">
                            Heals
                        </toggle-button>

                        <toggle-button v-model="columns.healsPerMinute">
                            Heals/Min
                            <info-hover text="This is not per minute of medic, but minute of playtime due to data limitations"></info-hover>

                        </toggle-button>

                        <toggle-button v-model="columns.revives">
                            Revives
                        </toggle-button>

                        <toggle-button v-model="columns.revivesPerMinute">
                            Revive/Min
                            <info-hover text="This is not per minute of medic, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.shieldRepairs">
                            Shield reps
                        </toggle-button>

                        <toggle-button v-model="columns.shieldRepairsPerMinute">
                            Shield reps/Min
                            <info-hover text="This is not per minute of medic, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>
                    </div>

                    <h4 class="mb-1">
                        <img src="/img/classes/icon_engi.png" height="20" />
                        <span class="align-bottom">Engineer</span>
                    </h4>

                    <div class="btn-group mb-3">
                        <toggle-button v-model="columns.maxRepairs">
                            MAX repairs
                        </toggle-button>

                        <toggle-button v-model="columns.maxRepairsPerMinute">
                            MAX repairs/Min
                            <info-hover text="This is not per minute of engineer, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="columns.resupplies">
                            Resupplies
                        </toggle-button>

                        <toggle-button v-model="columns.resuppliesPerMinute">
                            Resupplies/Min
                            <info-hover text="This is not per minute of engineer, but minute of playtime due to data limitations"></info-hover>
                        </toggle-button>
                    </div>
                </div>
            </collapsible>
        </div>

        <div v-if="showSummaryNeedsCalculation == true" class="alert alert-dismissible alert-primary text-center">
            Session summary stats have not been calculated for some sessions in this list<br />
            This data is calculated once every hour
            <button type="button" class="close" data-dismiss="alert" aria-lable="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>

        <a-table v-if="showTable" name="session-list"
            display-type="table" row-padding="compact" :striped="false" :hover="true" :show-filters="false"
            default-sort-field="start" default-sort-order="desc"
            :entries="data" :page-sizes="[50, 100, 200, 500]" :default-page-size="200">

            <a-col sort-field="start">
                <a-header>
                    <b>Start</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/s/' + entry.id" title="Open session">
                        {{entry.start | moment("YYYY-MM-DD hh:mm A")}}
                        <!--
                            {{entry.start | momentNoTz("ddd")}}
                        -->
                        ({{entry.start | timeAgo}} ago)
                    </a>

                    <span v-if="entry.summaryCalculated == null">
                        <info-hover text="Session summary stats have not been calculated yet!" icon="exclamation-mark" class="ph-bold ph-exclamation-mark"></info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="end">
                <a-header>
                    <b>Finish</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.end">
                        {{entry.end | moment}}
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="duration">
                <a-header>
                    <b>Duration</b>
                    <button type="button" class="btn btn-sm py-0 ml-2 mr-1 border" @click="toggleShowAll" :class="[ showAll ? 'btn-success' : 'btn-secondary' ]">
                        All
                    </button>

                    <info-hover text="Sessions under 5 minutes are hidden by default. Click 'All' to see all sessions">
                    </info-hover>

                    <span v-if="showAll == false && block.state == 'loaded'" class="text-muted">
                        ({{block.data.sessions.length - data.data.length}} hidden)
                    </span>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.end == null">
                        &lt;in progress&gt;
                    </span>

                    <span v-else>
                        {{entry.duration | mduration}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Outfit</b>

                    <info-hover text="What outfit this character was in during this session. Not 100% accurate!"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.outfitID == null || entry.outfitID == 0" class="text-muted">
                        &lt;no outfit&gt;
                    </span>
                    <span v-else>
                        <a :href="'/o/' + entry.outfitID">
                            <span v-if="entry.outfitName == null">
                                &lt;missing outfit {{entry.outfitID}}&gt;
                            </span>
                            <span v-else>
                                [{{entry.outfitTag}}] {{entry.outfitName}}
                            </span>
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col v-if="columns.exp" sort-field="expGained">
                <a-header>
                    <b>Exp gained</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.expGained | locale(0)}}
                    </session-cell>

                    <span v-if="entry.start < cutoff.exp">
                        <info-hover text="This value is not accurate before 2022-07-31" icon="exclamation-mark" class="ph-fw text-warning"></info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col v-if="columns.expPerMinute" sort-field="expPerMinute">
                <a-header>
                    <b>Exp/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.expPerMinute | locale(2)}}
                    </session-cell>

                    <span v-if="entry.start < cutoff.exp">
                        <info-hover text="This value is not accurate before 2022-07-31" icon="exclamation-mark" class="ph-fw text-warning"></info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col v-if="columns.kills" sort-field="kills">
                <a-header>
                    <b>Kills</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.kills | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.deaths" sort-field="deaths">
                <a-header>
                    <b>Deaths</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.deaths | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.kd" sort-field="kd">
                <a-header>
                    <b>K/D</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.kd | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.kpm" sort-field="kpm">
                <a-header>
                    <b>KPM</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.kpm | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.vkills" sort-field="vkills">
                <a-header>
                    <b>V.Kills</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.vkills | locale(0)}}
                    </session-cell>

                    <span v-if="entry.start < cutoff.vkill">
                        <info-hover text="This data was not collected before 2022-01-18" icon="exclamation-mark" class="ph-fw text-warning"></info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col v-if="columns.vkpm" sort-field="vkpm">
                <a-header>
                    <b>V.KPM</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.vkpm | locale(0)}}
                    </session-cell>

                    <span v-if="entry.start < cutoff.vkill">
                        <info-hover text="This data was not collected before 2022-01-18" icon="exclamation-mark" class="ph-fw text-warning"></info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col v-if="columns.spawns" sort-field="spawns">
                <a-header>
                    <b>Spawns</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.spawns | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.spawnsPerMinute" sort-field="spawnsPerMinute">
                <a-header>
                    <b>Spawns/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.spawnsPerMinute | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.heals" sort-field="heals">
                <a-header>
                    <b>Heals</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.heals | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.healsPerMinute" sort-field="healsPerMinute">
                <a-header>
                    <b>Heals/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.healsPerMinute | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.revives" sort-field="revives">
                <a-header>
                    <b>Revives</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.revives | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.revivesPerMinute" sort-field="revivesPerMinute">
                <a-header>
                    <b>Revives/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.revivesPerMinute | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.shieldRepairs" sort-field="shieldRepairs">
                <a-header>
                    <b>Shield reps</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.shieldRepairs | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.shieldRepairsPerMinute" sort-field="shieldRepairsPerMinute">
                <a-header>
                    <b>Shield reps/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.shieldRepairsPerMinute | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.maxRepairs" sort-field="maxRepairs">
                <a-header>
                    <b>MAX repairs</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.maxRepairs | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.maxRepairsPerMinute" sort-field="maxRepairsPerMinute">
                <a-header>
                    <b>MAX repairs/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.maxRepairsPerMinute | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.resupplies" sort-field="resupplies">
                <a-header>
                    <b>Resupplies</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.resupplies | locale(0)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col v-if="columns.resuppliesPerMinute" sort-field="resuppliesPerMinute">
                <a-header>
                    <b>Resupplies/Min</b>
                </a-header>

                <a-body v-slot="entry">
                    <session-cell :entry="entry">
                        {{entry.resuppliesPerMinute | locale(2)}}
                    </session-cell>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>View</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/s/' + entry.id">
                        View


                    </a>
                </a-body>
            </a-col>

        </a-table>

        <div>
            <a :href="'/api/character/' + character.id + '/sessions-block'" class="btn btn-primary" target="_blank"
                :download="'honu-character-sessions-' + character.name + '-' + character.id + '.json'">

                Export
                <span class="ph-fw ph-bold ph-download"></span>
            </a>
        </div>

        <div class="d-flex">
            <div class="flex-grow-1"></div>
            <div class="flex-grow-1" style="max-width: 1080px">
                Data limitations:
                <ul class="mb-0">
                    <li>
                        Sessions before 2021-07-23 are not tracked
                    </li>
                    <li>
                        Sessions before 2022-01-18 do not contain vehicle destroy events
                    </li>
                    <li>
                        Sessions before 2022-07-31 do not contain all exp events, and the experience gained is not accurate until after this date
                    </li>
                </ul>
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
    import ToggleButton from "components/ToggleButton";
    import Collapsible from "components/Collapsible.vue";

    import SessionChart from "./SessionChart.vue";

    import "filters/LocaleFilter";
    import "filters/FixedFilter";
    import "filters/TimeAgoFilter";

    import UserStorageUtil from "util/UserStorage";

    import { PsCharacter } from "api/CharacterApi";
    import { PsOutfit } from "api/OutfitApi";
    import { Session, SessionBlock, SessionApi } from "api/SessionApi";

    import { FlatSession } from "./common";

    type SessionOutfit = {
        session: Session;
        outfit: PsOutfit | null;
    }

    const SessionCell = Vue.extend({
        props: {
            entry: { type: Object }
        },

        template: `
            <span>
                <span v-if="entry.summaryCalculated == null">
                    --
                </span>

                <slot v-else></slot>
            </span>
        `
    });

    export const CharacterSessions = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                block: Loadable.idle() as Loading<SessionBlock>,

                showAll: true as boolean,
                showTable: true as boolean,

                showChart: false as boolean,

                cutoff: {
                    vkill: new Date(2022, 0, 18) as Date, // month is 0 indexed
                    exp: new Date(2022, 6, 23) as Date
                },

                columns: {
                    exp: true as boolean,
                    expPerMinute: true as boolean,
                    kills: true as boolean,
                    deaths: true as boolean,
                    kd: true as boolean,
                    kpm: true as boolean,

                    vkills: false as boolean,
                    vkpm: false as boolean,

                    spawns: false as boolean,
                    spawnsPerMinute: false as boolean,

                    heals: false as boolean,
                    revives: false as boolean,
                    shieldRepairs: false as boolean,
                    healsPerMinute: false as boolean,
                    revivesPerMinute: false as boolean,
                    shieldRepairsPerMinute: false as boolean,

                    resupplies: false as boolean,
                    maxRepairs: false as boolean,
                    resuppliesPerMinute: false as boolean,
                    maxRepairsPerMinute: false as boolean
                }
            }
        },

        beforeMount: function(): void {
            this.loadFromStorage();
            this.loadSessions();
        },

        methods: {
            loadFromStorage: function(): void {
                if (UserStorageUtil.available() == false) {
                    return;
                }

                const actions: any | null = UserStorageUtil.get<object>("Character.Sessions.Columns");
                console.log("actions from storage", actions);

                if (actions == null) {
                    return;
                }

                for (const key of Object.keys(this.columns)) {
                    console.log("setting column", key, actions[key]);
                    (this.columns as any)[key] = actions[key] == true;
                }
            },

            saveToStorage: function(): void {
                if (UserStorageUtil.available() == false) {
                    return;
                }

                console.log(`saving columngs to storage`, JSON.stringify(this.columns));
                UserStorageUtil.set("Character.Sessions.Columns", this.columns);
            },

            resetShow: function(): void {
                this.columns = {
                    exp: true as boolean,
                    expPerMinute: true as boolean,
                    kills: true as boolean,
                    deaths: true as boolean,
                    kd: true as boolean,
                    kpm: true as boolean,

                    vkills: false as boolean,
                    vkpm: false as boolean,

                    spawns: false as boolean,
                    spawnsPerMinute: false as boolean,

                    heals: false as boolean,
                    revives: false as boolean,
                    shieldRepairs: false as boolean,
                    healsPerMinute: false as boolean,
                    revivesPerMinute: false as boolean,
                    shieldRepairsPerMinute: false as boolean,

                    resupplies: false as boolean,
                    maxRepairs: false as boolean,
                    resuppliesPerMinute: false as boolean,
                    maxRepairsPerMinute: false as boolean
                };
            },

            loadSessions: async function(): Promise<void> {
                this.block = Loadable.loading();
                this.block = await SessionApi.getBlockByCharacterID(this.character.id);
                if (this.block.state == "loaded") {
                    this.block.data.sessions = this.block.data.sessions.sort((a, b) => b.id - a.id);

                    // HACK 2024-06-03: this lets the duration button show how many sessions are hidden
                    this.showTable = false;
                    this.$nextTick(() => {
                        this.showTable = true;
                    });
                }
            },

            toggleShowAll: function(): void {
                console.log(`show all!`);
                this.showAll = !this.showAll;
            }
        },

        computed: {
            filteredSessions: function(): Loading<SessionOutfit[]> {
                if (this.block.state != "loaded") {
                    return Loadable.rewrap(this.block);
                }

                let sessions: Session[] = [];

                if (this.showAll == true) {
                    sessions = this.block.data.sessions;
                } else {
                    sessions = this.block.data.sessions.filter(iter => {
                        // always show in progress sessions, even if it's only say a second old
                        if (iter.end == null) {
                            return true;
                        }

                        const end: number = (iter.end ?? new Date()).getTime();
                        const start: number = iter.start.getTime();

                        return (end - start) > 1000 * 60 * 5;
                    });
                }

                return Loadable.loaded(sessions.map(iter => {
                    if (this.block.state != "loaded") {
                        throw `how did block get unloaded?`;
                    }

                    return {
                        session: iter,
                        outfit: iter.outfitID == null ? null : (this.block.data.outfits.get(iter.outfitID) ?? null)
                    };
                }));
            },

            showSummaryNeedsCalculation: function(): boolean {
                if (this.block.state != "loaded") {
                    return false;
                }

                return this.block.data.sessions.find(iter => iter.summaryCalculated == null) != null;
            },

            data: function(): Loading<FlatSession[]> {
                if (this.block.state != "loaded") {
                    return Loadable.rewrap(this.block);
                }

                let sessions: Session[] = [];

                if (this.showAll == true) {
                    sessions = this.block.data.sessions;
                } else {
                    sessions = this.block.data.sessions.filter(iter => {
                        // always show in progress sessions, even if it's only say a second old
                        if (iter.end == null) {
                            return true;
                        }

                        const end: number = (iter.end ?? new Date()).getTime();
                        const start: number = iter.start.getTime();

                        // show sessions that have events within them, or are more than 5 minutes long
                        return ((end - start) > 1000 * 60 * 5) || iter.kills > 0 || iter.deaths > 0 || iter.experienceGained > 0;
                    });
                }

                const outfits: Map<string, PsOutfit> = this.block.data.outfits;

                return Loadable.loaded(sessions.sort((a, b) => {
                    return a.start.getTime() - b.start.getTime();
                }).map(iter => {
                    let outfit: PsOutfit | undefined;
                    if (iter.outfitID == null || iter.outfitID == "0") {
                        outfit = undefined;
                    } else {
                        outfit = outfits.get(iter.outfitID);
                    }

                    const duration: number = Math.round(((iter.end ?? new Date()).getTime() - iter.start.getTime()) / 1000);

                    const s: FlatSession = {
                        id: iter.id,
                        characterID: iter.characterID,
                        start: iter.start,
                        end: iter.end,
                        outfitID: iter.outfitID,
                        outfitName: outfit?.name ?? null,
                        outfitTag: outfit?.tag ?? null,
                        outfitFaction: outfit?.factionID ?? -1,

                        duration: Math.round(((iter.end ?? new Date()).getTime() - iter.start.getTime()) / 1000),

                        summaryCalculated: iter.summaryCalculated,
                        expGained: iter.experienceGained,
                        expPerMinute: iter.experienceGained / Math.max(1, duration) * 60,

                        kills: iter.kills,
                        vkills: iter.vehicleKills,
                        deaths: iter.deaths,
                        kd: iter.kills / Math.max(1, iter.deaths),
                        kpm: iter.kills / Math.max(1, duration) * 60,
                        vkpm: iter.vehicleKills / Math.max(1, duration) * 60,

                        heals: iter.heals,
                        revives: iter.revives,
                        shieldRepairs: iter.shieldRepairs,

                        healsPerMinute: iter.heals / Math.max(1, duration) * 60,
                        revivesPerMinute: iter.revives / Math.max(1, duration) * 60,
                        shieldRepairsPerMinute: iter.shieldRepairs / Math.max(1, duration) * 60,

                        resupplies: iter.resupplies,
                        maxRepairs: iter.repairs,
                        resuppliesPerMinute: iter.resupplies / Math.max(1, duration) * 60,
                        maxRepairsPerMinute: iter.repairs / Math.max(1, duration) * 60,

                        spawns: iter.spawns,
                        spawnsPerMinute: iter.spawns / Math.max(1, duration) * 60
                    };

                    return s;
                }));
            }
        },

        watch: {
            columns: {
                deep: true,
                handler: async function(): Promise<void> {
                    this.saveToStorage();

                    // force the table to be destroyed then re-created
                    this.showTable = false;
                    await this.$nextTick();
                    this.showTable = true;
                }
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover, ToggleButton, Collapsible,
            SessionCell,
            SessionChart
        }
    });
    export default CharacterSessions;
</script>
