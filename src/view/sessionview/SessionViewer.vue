<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/character">Characters</a>
            </li>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <span v-if="character.state == 'loading' || character.state == 'idle'">
                    &lt;loading...&gt;
                </span>

                <a v-else-if="character.state == 'loaded'" :href="'/c/' + character.data.id + '/sessions'">
                    {{character.data.name}}
                </a>

                <a v-else-if="session.state == 'loaded' && (character.state == 'nocontent' || character.state == 'error')" :href="'/c/' + session.data.characterID + '/sessions'">
                    &lt;missing {{session.data.characterID}}&gt;
                </a>
            </li>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Session {{sessionID}}
            </li>
        </honu-menu>

        <div v-if="session.state == 'loading'">
            Loading session...
        </div>

        <div v-else-if="session.state == 'loaded'">
            <div v-if="badStreams.length > 0" class="alert alert-warning text-center h5">
                <div>
                    Honu reconnected to the Planetside 2 API due to a bad realtime event stream during this session, this caused:
                </div>

                <ul class="d-inline-block text-left mb-0">
                    <li v-for="stream in badStreams">
                        {{stream.secondsMissed | tduration}} of {{stream.streamType}} events to be missed
                    </li>
                </ul>
            </div>

            <div v-if="showFullExp == false" class="alert alert-primary text-center h5" @click="showFullExp = true">
                <div>
                    This session was created before all exp events were tracked, and some data will be hidden.
                </div>
                <div class="mt-2">
                    <span class="wt-link">
                        Click here
                    </span>
                    to show the full exp data anyways
                </div>
            </div>

            <div v-if="showEventProcessLag == true" class="alert alert-danger text-center h4">
                Honu is {{processLag.data.processLag | mduration}} behind on events, and not all events in this session have been processed yet

                <div class="h5 mt-1">
                    Most recent event: {{processLag.data.mostRecentEvent | moment("YYYY-MM-DD hh:mm:ss")}}
                </div>
            </div>

            <collapsible header-text="Session" id="session-info">
                <table class="table table-sm w-auto d-inline-block mr-4" style="vertical-align: top;">
                    <tr class="table-secondary">
                        <td><b>ID</b></td>
                        <td>{{session.data.id}}</td>
                    </tr>

                    <tr v-if="character.state == 'loaded'">
                        <td><b>Character</b></td>
                        <td>
                            <a :href="'/c/' + character.data.id">
                                <span v-if="character.data.outfitID != null">
                                    [{{character.data.outfitTag}}]
                                </span>
                                {{character.data.name}}
                            </a>
                        </td>
                    </tr>

                    <tr v-if="character.state == 'loaded'">
                        <td><b>World</b></td>
                        <td>
                            <a :href="'/view/' + character.data.worldID">
                                {{character.data.worldID | world}}
                            </a>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Faction</b></td>
                        <td>
                            {{session.data.teamID | faction}}
                        </td>
                    </tr>

                    <tr v-if="character.state == 'loaded'">
                        <td>
                            <b>Outfit (current)</b>
                            <info-hover text="What outfit the character is currently in"></info-hover>
                        </td>
                        <td>
                            <span v-if="character.data.outfitID == '0' || character.data.outfitID == null">
                                &lt;no outfit&gt;
                            </span>

                            <a v-else :href="'/o/' + character.data.outfitID">
                                <span v-if="character.data.outfitTag != null">
                                    [{{character.data.outfitTag}}]
                                </span>
                                {{character.data.outfitName}}
                            </a>
                        </td>
                    </tr>

                    <tr>
                        <td>
                            <b>Outfit (session)</b>
                            <info-hover text="What outfit the character was in when this session was started"></info-hover>
                        </td>
                        <td>
                            <span v-if="session.data.outfitID == null || session.data.outfitID == '0'">
                                &lt;no outfit&gt;
                            </span>
                            <span v-else>
                                <a :href="'/o/' + session.data.outfitID">
                                    view
                                </a>
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Start</b></td>
                        <td>{{session.data.start | moment("YYYY-MM-DD hh:mm:ss")}}</td>
                    </tr>

                    <tr>
                        <td><b>End</b></td>
                        <td>
                            <span v-if="session.data.end == null">
                                &lt;In progress&gt;
                            </span>
                            <span v-else>
                                {{session.data.end | moment("YYYY-MM-DD hh:mm:ss")}}
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Duration</b></td>
                        <td>{{durationInSeconds | mduration}}</td>
                    </tr>
                </table>

                <table class="table table-sm w-auto d-inline-block">
                    <tr class="table-secondary">
                        <th>Data</th>
                        <th>State</th>
                    </tr>

                    <tr>
                        <td>Session</td>
                        <td>{{session.state}}</td>
                    </tr>

                    <tr>
                        <td>Character</td>
                        <td>{{character.state}}</td>
                    </tr>

                    <tr>
                        <td>Kills</td>
                        <td>
                            <span v-if="fullKills.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="fullKills.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{fullKills.state}}
                            </span>
                            <span v-if="fullKills.state == 'loaded'">
                                ({{fullKills.data.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>Deaths</td>
                        <td>
                            <span v-if="fullDeaths.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="fullDeaths.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{fullDeaths.state}}
                            </span>
                            <span v-if="fullDeaths.state == 'loaded'">
                                ({{deaths.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>Exp</td>
                        <td>
                            <span v-if="exp.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="exp.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{exp.state}}
                            </span>
                            <span v-if="exp.state == 'loaded'">
                                ({{exp.data.events.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>Exp (target)</td>
                        <td>
                            <span v-if="expOther.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="expOther.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{expOther.state}}
                            </span>
                            <span v-if="expOther.state == 'loaded'">
                                ({{expOther.data.events.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>Vehicle destroy</td>
                        <td>
                            <span v-if="vehicleDestroy.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="vehicleDestroy.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{vehicleDestroy.state}}
                            </span>
                            <span v-if="vehicleDestroy.state == 'loaded'">
                                ({{vehicleDestroy.data.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>
                            Achievements
                            <info-hover text="Not available before 2022-08-01"></info-hover>
                        </td>
                        <td>
                            <span v-if="achievementsEarned.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="achievementsEarned.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{achievementsEarned.state}}
                            </span>
                            <span v-if="achievementsEarned.state == 'loaded'">
                                ({{achievementsEarned.data.events.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>
                            Item Added
                        </td>

                        <td>
                            <span v-if="itemAddedBlock.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else-if="itemAddedBlock.state == 'loaded'" class="text-info">
                                loaded
                            </span>
                            <span v-else>
                                {{itemAddedBlock.state}}
                            </span>
                            <span v-if="itemAddedBlock.state == 'loaded'">
                                ({{itemAddedBlock.data.events.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>
                            Full xp?
                            <info-hover text="Before 2022-07-31, only specific exp events were tracked"></info-hover>
                        </td>
                        <td>
                            <span v-if="showFullExp == true" class="text-success">
                                Yes
                            </span>
                            <span v-else class="text-warning">
                                No
                            </span>
                        </td>
                    </tr>
                </table>

                <div v-if="range.show == true" class="py-3 px-5 border rounded">
                    <div id="range-slider" class="mb-3"></div>

                    <div v-if="session.state == 'loaded'" class="mb-3 text-center">
                        <span>
                            showing events from {{range.start | moment}}
                        </span>
                        <span>
                            to {{range.end | moment}}
                        </span>
                    </div>
                </div>
            </collapsible>

            <div v-if="showCharts == true">
                <collapsible header-text="Summary" id="session-general" class="mb-3">
                    <div v-if="exp.state == 'loading' || fullKills.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-viewer-general v-else-if="exp.state == 'loaded' && fullKills.state == 'loaded'"
                        :session="session.data" :exp="exp.data" :kills="kills" :deaths="deaths" :full-exp="showFullExp" :duration="durationInSeconds2">
                    </session-viewer-general>
                </collapsible>

                <collapsible header-text="Kills and deaths" id="session-kills" class="mb-3">
                    <div v-if="fullKills.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-viewer-kills v-else-if="fullKills.state == 'loaded'"
                        :kills="kills" :deaths="deaths" :teamkills="teamkills" :session="session.data" :full-exp="showFullExp">
                    </session-viewer-kills>
                </collapsible>

                <collapsible v-if="showFullExp == true" header-text="Experience breakdown" id="session-expb" class="mb-3">
                    <div v-if="exp.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-viewer-exp-breakdown v-else-if="exp.state == 'loaded'"
                        :session="session.data" :exp="exp.data" :full-exp="showFullExp">
                    </session-viewer-exp-breakdown>
                </collapsible>

                <collapsible header-text="Experience" id="session-exp" class="mb-3">
                    <div v-if="exp.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-viewer-exp v-else-if="exp.state == 'loaded'" :session="session.data" :exp="exp.data" :full-exp="showFullExp">
                    </session-viewer-exp>
                </collapsible>

                <collapsible header-text="Supported by" id="session-supported-by" class="mb-3">
                    <div v-if="expOther.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-supported-by v-else-if="expOther.state == 'loaded'" :session="session.data" :exp="expOther.data" :full-exp="showFullExp">
                    </session-supported-by>
                </collapsible>

                <collapsible header-text="Achievements earned" id="session-achievement" class="mb-3">
                    <div v-if="achievementsEarned.state == 'loading'">
                        <busy class="honu-busy"></busy>
                        Loading...
                    </div>

                    <session-achievements-earned v-else-if="achievementsEarned.state == 'loaded'"
                        :session="session.data" :earned="achievementsEarned.data">
                    </session-achievements-earned>
                </collapsible>

                <collapsible header-text="Trends" id="session-trends" class="mb-3">
                    <div v-if="exp.state == 'loading' || fullKills.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-viewer-trends v-else-if="exp.state == 'loaded' && fullKills.state == 'loaded'"
                        :session="session.data" :kills="kills" :deaths="deaths" :exp="exp.data" :full-exp="showFullExp">
                    </session-viewer-trends>
                </collapsible>

                <collapsible header-text="Routers & Sunderers" id="session-spawns" class="mb-3">
                    <div v-if="exp.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-viewer-spawns v-else-if="exp.state == 'loaded'"
                        :session="session.data" :exp="exp.data" :full-exp="showFullExp">
                    </session-viewer-spawns>
                </collapsible>

                <collapsible header-text="Action log" id="session-action-log" class="mb-3">
                    <div v-if="exp.state == 'loading' || fullKills.state == 'loading' || vehicleDestroy.state == 'loading'">
                        <busy style="max-height: 1.25rem;"></busy>
                        Loading...
                    </div>

                    <session-action-log v-else-if="exp.state == 'loaded' && fullKills.state == 'loaded' && vehicleDestroy.state == 'loaded' && expOther.state == 'loaded'"
                        :session="session.data" :character="character.data"
                        :kills="kills" :deaths="deaths" :teamkills="teamkills" :item-added="itemAddedEvents"
                        :exp="exp.data" :exp-other="expOther.data" :vehicle-destroy="vehicleDestroyEvents" :full-exp="showFullExp">
                    </session-action-log>
                </collapsible>

                <collapsible header-text="Item added" id="session-item-added" class="mb-3">
                    <session-item-added :session="session.data"></session-item-added>
                </collapsible>
            </div>

        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "filters/WorldNameFilter";

    import TimeUtils from "util/Time";

    import SessionViewerKills from "./components/SessionViewerKills.vue";
    import SessionViewerGeneral from "./components/SessionViewerGeneral.vue";
    import SessionViewerExp from "./components/SessionViewerExp.vue";
    import SessionViewerTrends from "./components/SessionViewerTrends.vue";
    import SessionActionLog from "./components/SessionActionLog.vue";
    import SessionViewerSpawns from "./components/SessionViewerSpawns.vue";
    import SessionViewerExpBreakdown from "./components/SessionViewerExpBreakdown.vue";
    import SessionAchievementsEarned from "./components/SessionAchievementsEarned.vue";
    import SessionItemAdded from "./components/SessionItemAdded.vue";
    import SessionSupportedBy from "./components/SessionSupportedBy.vue";
    import ChartTimestamp from "./components/ChartTimestamp.vue";

    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import Collapsible from "components/Collapsible.vue";

    import { ExpandedKillEvent, KillDeathBlock, KillEvent, KillStatApi } from "api/KillStatApi";
    import { Experience, ExpandedExpEvent, ExpEvent, ExpStatApi, ExperienceBlock } from "api/ExpStatApi";
    import { ExpandedVehicleDestroyEvent, VehicleDestroyEvent, VehicleDestroyEventApi } from "api/VehicleDestroyEventApi";
    import { AchievementEarnedBlock, AchievementEarnedApi } from "api/AchievementEarnedApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { RealtimeReconnectEntry, RealtimeReconnectApi } from "api/RealtimeReconnectApi";
    import { ItemCategory } from "api/ItemCategoryApi";
    import { PsItem } from "api/ItemApi";
    import { HonuHealthApi, HealthEventProcessLag } from "api/HonuHealthApi";
    import { ItemAddedEventBlock, ItemAddedEventApi, ItemAddedEvent } from "api/ItemAddedEventApi";

    import * as ds from "node_modules/nouislider/dist/nouislider";
    import "node_modules/nouislider/dist/nouislider.css";

    type FullKillEvent = ExpandedKillEvent & { itemCategory: ItemCategory | null };

    type ExpandedItemAddedEvent = {
        event: ItemAddedEvent;
        item: PsItem | null;
    }

    // 2024-06-17 TODO: yeah this code is kinda bad

    export const SessionViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                sessionID: 0 as number,

                showFullExp: false as boolean,

                showCharts: true as boolean,

                range: {
                    show: false as boolean,

                    min: 0 as number,
                    max: 0 as number,
                    // these values are set to cover all possible events so if the kills
                    // of a session are loaded before the session itself, the shown kills is all events
                    start: new Date(0) as Date,
                    end: new Date(8640000000000000) as Date,

                    timeout: 0 as number
                },

                session: Loadable.idle() as Loading<Session>,
                character: Loadable.idle() as Loading<PsCharacter>,

                killBlock: Loadable.idle() as Loading<KillDeathBlock>,
                fullKills: Loadable.idle() as Loading<FullKillEvent[]>,
                fullDeaths: Loadable.idle() as Loading<FullKillEvent[]>,
                exp: Loadable.idle() as Loading<ExperienceBlock>,
                allExp: [] as ExpEvent[],
                expOther: Loadable.idle() as Loading<ExperienceBlock>,
                allOtherExp: [] as ExpEvent[],
                vehicleDestroy: Loadable.idle() as Loading<ExpandedVehicleDestroyEvent[]>,
                achievementsEarned: Loadable.idle() as Loading<AchievementEarnedBlock>,
                itemAddedBlock: Loadable.idle() as Loading<ItemAddedEventBlock>,

                reconnects: Loadable.idle() as Loading<RealtimeReconnectEntry[]>,
                processLag: Loadable.idle() as Loading<HealthEventProcessLag>
            }
        },

        created: function(): void {
            document.title = `Honu / Session / <loading...>`;
        },

        beforeMount: function(): void {
            this.getSessionIDFromUrl();
            this.bindAll();
        },

        methods: {
            getSessionIDFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                const sessionID: number = Number.parseInt(parts[2]);
                if (Number.isNaN(sessionID) == false) {
                    this.sessionID = sessionID;
                    console.log(`Session ID is ${this.sessionID}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${sessionID}`;
                }

                document.title = `Honu / Session / ${this.sessionID}`;
            },

            bindAll: function(): void {
                this.bindEventProcessLag();
                this.bindSession();
                // Character is not bound, cause it uses the .characterID field from the session, so it's done when session is bound
                this.bindKills();
                this.bindExp();
                this.bindExpOther();
                this.bindVehicleDestroy();
                this.bindAchievementsEarned();
                this.bindItemAdded();
            },

            bindSession: async function(): Promise<void> {
                this.session = Loadable.loading();
                this.session = await SessionApi.getBySessionID(this.sessionID);

                if (this.session.state == "loaded") {
                    const fullStart: Date = new Date("2022-07-31T00:00");
                    this.showFullExp = this.session.data.start.getTime() > fullStart.getTime();

                    if (this.showFullExp == true) {
                        console.log(`SessionViewer> showing full EXP`);
                    }

                    this.bindCharacter();

                    this.range.start = this.session.data.start;
                    this.range.max = ((this.session.data.end ?? new Date()).getTime() - this.session.data.start.getTime()) / 1000;
                    this.range.end = new Date(this.session.data.start.getTime() + (this.range.max * 1000));

                    this.$nextTick(() => {
                        const slider: HTMLElement | null = document.getElementById("range-slider");
                        if (slider == null) {
                            throw `failed to find #range-slider`;
                        }

                        const sliderObj = ds.create(slider, {
                            range: {
                                min: this.range.min,
                                max: this.range.max
                            },
                            start: [0, this.range.max],
                            connect: true,
                            tooltips: {
                                to: (value) => {
                                    return TimeUtils.format(new Date(this.range.start.getTime() + value * 1000));
                                },
                            }
                        });
                        console.log(`SessionViewer: created range slider`);

                        sliderObj.on("set", (values, handle) => {
                            clearTimeout(this.range.timeout);

                            this.range.timeout = setTimeout(() => {
                                if (this.session.state != "loaded") {
                                    throw `cannot change range, session is not loaded`;
                                }

                                const startv: number = typeof values[0] == "string" ? Number.parseInt(values[0]) : values[0];
                                this.range.start = new Date(this.session.data.start.getTime() + (startv * 1000));

                                const endv: number = typeof values[1] == "string" ? Number.parseInt(values[1]) : values[1];
                                this.range.end = new Date(this.session.data.start.getTime() + (endv * 1000));

                                console.log(`SessionViewer> range changed to ${this.range.start} to ${this.range.end}`);

                                this.showCharts = false;
                                this.updateExp();
                                this.$nextTick(() => {
                                    this.showCharts = true;
                                });
                            }, 500) as unknown as number;
                        });
                    });
                }
            },

            bindCharacter: async function(): Promise<void> {
                if (this.session.state != "loaded") {
                    this.character = Loadable.idle();
                    return;
                }

                this.character = Loadable.loading();
                this.character = await CharacterApi.getByID(this.session.data.characterID);
                if (this.character.state == "loaded") {
                    this.bindReconnects();
                }
            },

            bindExp: async function(): Promise<void> {
                this.exp = Loadable.loading();
                this.exp = await ExpStatApi.getBySessionID(this.sessionID);
                this.checkAllAndScroll();

                if (this.exp.state == "loaded") {
                    this.allExp = this.exp.data.events;
                } else {
                    this.allExp = [];
                }
            },

            updateExp: function(): void {
                if (this.exp.state == "loaded") {
                    this.exp.data.events = this.allExp.filter(iter => {
                        return iter.timestamp.getTime() >= this.range.start.getTime()
                            && iter.timestamp.getTime() <= this.range.end.getTime();
                    });
                }

                if (this.expOther.state == "loaded") {
                    this.expOther.data.events = this.allOtherExp.filter(iter => {
                        return iter.timestamp.getTime() >= this.range.start.getTime()
                            && iter.timestamp.getTime() <= this.range.end.getTime();
                    });
                }
            },

            bindExpOther: async function(): Promise<void> {
                this.expOther = Loadable.loading();
                this.expOther = await ExpStatApi.getOtherBySessionID(this.sessionID);

                if (this.expOther.state == "loaded") {
                    this.allOtherExp = this.expOther.data.events;
                } else {
                    this.allOtherExp = [];
                }
            },

            bindKills: async function(): Promise<void> {
                this.fullKills = Loadable.loading();
                this.fullDeaths = Loadable.loading();

                const block: Loading<KillDeathBlock> = await KillStatApi.getSessionBlock(this.sessionID);
                this.killBlock = block;
                if (block.state != "loaded") {
                    this.fullKills = Loadable.rewrap(block);
                    this.fullDeaths = Loadable.rewrap(block);
                    return;
                }

    type ExpandedItemAddedEvent = {
        event: ItemAddedEvent;
        item: PsItem | null;
    }
                this.fullKills = Loadable.loaded(block.data.kills.map((iter: KillEvent): FullKillEvent => {
                    const item: PsItem | null = block.data.weapons.get(iter.weaponID) ?? null;
                    return {
                        event: iter,
                        killed: block.data.characters.get(iter.killedCharacterID) ?? null,
                        attacker: block.data.characters.get(iter.attackerCharacterID) ?? null,
                        item: item,
                        fireGroupToFireMode: block.data.fireModes.get(iter.attackerFireModeID) ?? null,
                        itemCategory: (item == null) ? null : block.data.itemCategories.get(item.categoryID) ?? null
                    };
                }));

                this.fullDeaths = Loadable.loaded(block.data.deaths.map((iter: KillEvent): FullKillEvent => {
                    const item: PsItem | null = block.data.weapons.get(iter.weaponID) ?? null;
                    return {
                        event: iter,
                        killed: block.data.characters.get(iter.killedCharacterID) ?? null,
                        attacker: block.data.characters.get(iter.attackerCharacterID) ?? null,
                        item: item,
                        fireGroupToFireMode: block.data.fireModes.get(iter.attackerFireModeID) ?? null,
                        itemCategory: (item == null) ? null : block.data.itemCategories.get(item.categoryID) ?? null
                    };
                }));

                this.checkAllAndScroll();
            },

            bindVehicleDestroy: async function(): Promise<void> {
                this.vehicleDestroy = Loadable.loading();
                this.vehicleDestroy = await VehicleDestroyEventApi.getBySessionID(this.sessionID);
                this.checkAllAndScroll();
            },

            bindAchievementsEarned: async function(): Promise<void> {
                this.achievementsEarned = Loadable.loading();
                this.achievementsEarned = await AchievementEarnedApi.getBlockBySessionID(this.sessionID);
                this.checkAllAndScroll();
            },

            bindItemAdded: async function(): Promise<void> {
                this.itemAddedBlock = Loadable.loading();
                this.itemAddedBlock = await ItemAddedEventApi.getBySessionID(this.sessionID);
            },

            bindReconnects: async function(): Promise<void> {
                if (this.session.state != "loaded" || this.character.state != "loaded") {
                    this.reconnects = Loadable.idle();
                    return;
                }

                this.reconnects = Loadable.loading();
                this.reconnects = await RealtimeReconnectApi.getByInterval(this.session.data.start, this.session.data.end ?? new Date());
                if (this.reconnects.state == "loaded" && this.character.state == "loaded") {
                    const worldID: number = this.character.data.worldID;
                    this.reconnects.data = this.reconnects.data.filter(iter => iter.worldID == worldID);
                }
    type ExpandedItemAddedEvent = {
        event: ItemAddedEvent;
        item: PsItem | null;
    }
            },

            bindEventProcessLag: async function(): Promise<void> {
                this.processLag = Loadable.loading();
                this.processLag = await HonuHealthApi.getEventProcessLag();
            },

            checkAllAndScroll: function(): void {
                if (this.fullKills.state != "loaded" || this.fullDeaths.state != "loaded" || this.vehicleDestroy.state != "loaded"
                    || this.achievementsEarned.state != "loaded" || this.exp.state != "loaded") {

                    return;
                }

                // let vue update and render the elements
                this.$nextTick(async () => {
                    // wait 1000ms for all children to render
                    // TODO: there's gotta be a better way to let all children render first lol
                    await new Promise((resolve) => setTimeout(resolve, 1000));
                    const id: string = location.hash.slice(1); // .hash includes the #, remove it
                    if (id.length > 0) {
                        document.getElementById(id)?.scrollIntoView();
                    }
                });
            }

        },

        computed: {
            durationInSeconds: function(): number {
                if (this.session.state != "loaded") {
                    return -1;
                }
                return ((this.session.data.end || new Date()).getTime() - this.session.data.start.getTime()) / 1000;
            },

            durationInSeconds2: function(): number {
                return (this.range.start.getTime() - this.range.end.getTime()) / 1000;
            },

            badStreams: function(): any[] {
                if (this.reconnects.state != "loaded") {
                    return [];
                }

                const cutoff: Date = new Date(new Date().getTime() - (1000 * 60 * 60 * 2));

                // Because the timestamp represents the end of an outage, and the duration can extend into before the current interval
                //		it's possible that a 2 hour outage 1 hour ago will instead show at 2 hours. To prevent this, the duration of the
                //		reconnect is adjusted to only include the period the realtime is for
                for (const reconnect of this.reconnects.data) {
                    const outageStart: Date = new Date(reconnect.timestamp.getTime() - (reconnect.duration * 1000));
                    const startDiff: number = outageStart.getTime() - cutoff.getTime();
                    const diff: number = -1 * Math.floor(startDiff / 1000);
                    if (startDiff < 0) {
                        //console.log(`outage at ${reconnect.timestamp.toISOString()} from a duration of ${reconnect.duration} - ${diff} = ${reconnect.duration - diff}`);
                        reconnect.duration -= diff;
                    }
                }

                const expCount: number = this.reconnects.data.filter(iter => iter.streamType == "exp").reduce((acc, i) => acc += i.duration, 0);

                const arr: any[] = [];

                const deathCount: number = this.reconnects.data.filter(iter => iter.streamType == "death").reduce((acc, i) => acc += i.duration, 0);
                if (deathCount > 0) {
                    arr.push({ streamType: "death", secondsMissed: deathCount });
                }

                if (expCount > 0) {
                    arr.push({ streamType: "exp", secondsMissed: expCount });
                }

                return arr;
            },

            teamkills: function(): FullKillEvent[] {
                if (this.fullKills.state != "loaded") {
                    return [];
                }

                return this.fullKills.data.filter(iter =>
                    iter.event.timestamp.getTime() >= this.range.start.getTime()
                    && iter.event.timestamp.getTime() <= this.range.end.getTime()
                    && iter.event.attackerTeamID == iter.event.killedTeamID);
            },

            kills: function(): FullKillEvent[] {
                if (this.fullKills.state != "loaded") {
                    return [];
                }

                return this.fullKills.data.filter(iter => iter.event.timestamp.getTime() >= this.range.start.getTime()
                    && iter.event.timestamp.getTime() <= this.range.end.getTime()
                    && iter.event.attackerTeamID != iter.event.killedTeamID);
            },

            deaths: function(): FullKillEvent[] {
                if (this.fullDeaths.state != "loaded") {
                    return [];
                }

                return this.fullDeaths.data.filter(iter =>
                    iter.event.timestamp.getTime() >= this.range.start.getTime()
                    && iter.event.timestamp.getTime() <= this.range.end.getTime()
                    && iter.event.revivedEventID == null);
            },

            // includes revived deaths, used in action-log for showing who revived a death
            allDeaths: function(): FullKillEvent[] {
                if (this.fullDeaths.state != "loaded") {
                    return [];
                }

                return this.fullDeaths.data.filter(iter =>
                    iter.event.timestamp.getTime() >= this.range.start.getTime()
                    && iter.event.timestamp.getTime() <= this.range.end.getTime());
            },

            vehicleDestroyEvents: function(): ExpandedVehicleDestroyEvent[] {
                if (this.vehicleDestroy.state != "loaded") {
                    return [];
                }

                return this.vehicleDestroy.data.filter(iter => iter.event.timestamp.getTime() >= this.range.start.getTime()
                    && iter.event.timestamp.getTime() <= this.range.end.getTime());
            },

            itemAddedEvents: function(): ExpandedItemAddedEvent[] {
                if (this.itemAddedBlock.state != "loaded") {
                    return [];
                }

                return this.itemAddedBlock.data.events.map((iter: ItemAddedEvent): ExpandedItemAddedEvent => {
                    if (this.itemAddedBlock.state != "loaded") {
                        throw `how is itemAddedBlock not loaded here?`;
                    }
                    return {
                        event: iter,
                        item: this.itemAddedBlock.data.items.find(i => i.id == iter.itemID) ?? null
                    };
                }).sort((a, b) => a.event.timestamp.getTime() - b.event.timestamp.getTime());
            },

            showEventProcessLag: function(): boolean {
                if (this.session.state != "loaded") {
                    return false;
                }

                if (this.processLag.state != "loaded") {
                    return false;
                }

                if (this.session.data.end == null && this.processLag.data.processLag > 30) {
                    return true;
                }

                if (this.session.data.end != null && this.processLag.data.mostRecentEvent.getTime() < this.session.data.end.getTime()) {
                    return true;
                }

                return false;
            },

        },

        components: {
            SessionViewerKills, SessionViewerGeneral, SessionViewerExp, SessionViewerTrends, SessionActionLog,
            SessionViewerSpawns, SessionViewerExpBreakdown, SessionAchievementsEarned, SessionItemAdded, SessionSupportedBy,
            ChartTimestamp,
            InfoHover,
            Busy,
            Collapsible,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default SessionViewer;
</script>