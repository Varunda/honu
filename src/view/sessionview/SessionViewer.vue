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
                <span v-if="character.state == 'loading'">
                    &lt;loading...&gt;
                </span>

                <a v-else-if="character.state == 'loaded'" :href="'/c/' + character.data.id + '/sessions'">
                    {{character.data.name}}
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
                    Click here to show the full exp data anyways
                </div>
            </div>

            <collapsible header-text="Session" id="session-info">
                <table class="table table-sm w-auto d-inline-block mr-4" style="vertical-align: top;">
                    <tr>
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
                            <a :href="'/o/' + character.data.outfitID">
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
                            <span v-if="session.data.outfitID == null">
                                &lt;No outfit&gt;
                            </span>
                            <span v-else>
                                <a :href="'/o/' + session.data.outfitID">
                                    View
                                </a>
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Start</b></td>
                        <td>{{session.data.start | moment}}</td>
                    </tr>

                    <tr>
                        <td><b>End</b></td>
                        <td>
                            <span v-if="session.data.end == null">
                                &lt;In progress&gt;
                            </span>
                            <span v-else>
                                {{session.data.end | moment}}
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td><b>Duration</b></td>
                        <td>{{durationInSeconds | mduration}}</td>
                    </tr>
                </table>

                <table class="table table-sm w-auto d-inline-block">
                    <tr>
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
                            <span v-if="killsOrDeaths.state == 'loading'" class="text-warning">
                                Loading...
                            </span>
                            <span v-else>
                                {{killsOrDeaths.state}}
                            </span>
                            <span v-if="killsOrDeaths.state == 'loaded'">
                                ({{killsOrDeaths.data.length}})
                            </span>
                        </td>
                    </tr>

                    <tr>
                        <td>Exp</td>
                        <td>
                            <span v-if="exp.state == 'loading'" class="text-warning">
                                Loading...
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
                        <td>Achievements</td>
                        <td>
                            <span v-if="achievementsEarned.state == 'loading'" class="text-warning">
                                Loading...
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
                            Full xp?
                            <info-hover text="Before 2022-07-31, only specific exp events were tracked"></info-hover>
                        </td>
                        <td>
                            <span v-if="showFullExp == true">
                                Yes
                            </span>
                            <span v-else>
                                No
                            </span>
                        </td>
                    </tr>
                </table>
            </collapsible>

            <collapsible header-text="General" id="session-general">
                <div v-if="exp.state == 'loading' || killsOrDeaths.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-general v-else-if="exp.state == 'loaded' && killsOrDeaths.state == 'loaded'"
                    :session="session.data" :exp="exp.data" :kills="kills" :deaths="deaths" :full-exp="showFullExp">
                </session-viewer-general>
            </collapsible>

            <collapsible header-text="Kills" id="session-kills">
                <div v-if="killsOrDeaths.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-kills v-else-if="killsOrDeaths.state == 'loaded'"
                    :kills="kills" :deaths="deaths" :session="session.data" :full-exp="showFullExp">
                </session-viewer-kills>
            </collapsible>

            <collapsible v-if="showFullExp == true" header-text="Experience breakdown" id="session-expb">
                <div v-if="exp.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-exp-breakdown v-else-if="exp.state == 'loaded'" :session="session.data" :exp="exp.data" :full-exp="showFullExp"></session-viewer-exp-breakdown>
            </collapsible>

            <collapsible header-text="Experience" id="session-exp">
                <div v-if="exp.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-exp v-else-if="exp.state == 'loaded'" :session="session.data" :exp="exp.data" :full-exp="showFullExp"></session-viewer-exp>
            </collapsible>

            <collapsible header-text="Achievements earned" id="session-achievement">
                <div v-if="achievementsEarned.state == 'loading'">
                    <busy class="honu-busy"></busy>
                    Loading...
                </div>

                <session-achievements-earned v-else-if="achievementsEarned.state == 'loaded'" :session="session.data" :earned="achievementsEarned.data"></session-achievements-earned>
            </collapsible>

            <collapsible header-text="Trends" id="session-trends">
                <div v-if="exp.state == 'loading' || killsOrDeaths.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-trends v-else-if="exp.state == 'loaded' && killsOrDeaths.state == 'loaded'"
                    :session="session.data" :kills="kills" :deaths="deaths" :exp="exp.data" :full-exp="showFullExp">
                </session-viewer-trends>
            </collapsible>

            <collapsible header-text="Routers & Sunderers" id="session-spawns">
                <div v-if="exp.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-spawns v-else-if="exp.state == 'loaded'"
                    :session="session.data" :exp="exp.data" :full-exp="showFullExp">
                </session-viewer-spawns>
            </collapsible>

            <collapsible header-text="Action log" id="session-action-log">
                <div v-if="exp.state == 'loading' || killsOrDeaths.state == 'loading' || vehicleDestroy.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-action-log v-else-if="exp.state == 'loaded' && killsOrDeaths.state == 'loaded' && vehicleDestroy.state == 'loaded'"
                    :session="session.data" :kills="kills" :deaths="deaths" :exp="exp.data" :vehicle-destroy="vehicleDestroy.data" :full-exp="showFullExp">
                </session-action-log>
            </collapsible>

            <collapsible header-text="Item added" id="session-item-added">
                <session-item-added :session="session.data"></session-item-added>
            </collapsible>

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

    import SessionViewerKills from "./components/SessionViewerKills.vue";
    import SessionViewerGeneral from "./components/SessionViewerGeneral.vue";
    import SessionViewerExp from "./components/SessionViewerExp.vue";
    import SessionViewerTrends from "./components/SessionViewerTrends.vue";
    import SessionActionLog from "./components/SessionActionLog.vue";
    import SessionViewerSpawns from "./components/SessionViewerSpawns.vue";
    import SessionViewerExpBreakdown from "./components/SessionViewerExpBreakdown.vue";
    import SessionAchievementsEarned from "./components/SessionAchievementsEarned.vue";
    import SessionItemAdded from "./components/SessionItemAdded.vue";
    import ChartTimestamp from "./components/ChartTimestamp.vue";

    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import Collapsible from "components/Collapsible.vue";

    import { ExpandedKillEvent, KillEvent, KillStatApi } from "api/KillStatApi";
    import { Experience, ExpandedExpEvent, ExpEvent, ExpStatApi, ExperienceBlock } from "api/ExpStatApi";
    import { ExpandedVehicleDestroyEvent, VehicleDestroyEvent, VehicleDestroyEventApi } from "api/VehicleDestroyEventApi";
    import { AchievementEarnedBlock, AchievementEarnedApi } from "api/AchievementEarnedApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { RealtimeReconnectEntry, RealtimeReconnectApi } from "api/RealtimeReconnectApi";

    export const SessionViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                sessionID: 0 as number,

                showFullExp: false as boolean,

                session: Loadable.idle() as Loading<Session>,
                character: Loadable.idle() as Loading<PsCharacter>,

                killsOrDeaths: Loadable.idle() as Loading<ExpandedKillEvent[]>,
                exp: Loadable.idle() as Loading<ExperienceBlock>,
                vehicleDestroy: Loadable.idle() as Loading<ExpandedVehicleDestroyEvent[]>,
                achievementsEarned: Loadable.idle() as Loading<AchievementEarnedBlock>,

                reconnects: Loadable.idle() as Loading<RealtimeReconnectEntry[]>
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
                this.bindSession();
                // Character is not bound, cause it uses the .characterID field from the session, so it's done when session is bound
                this.bindKills();
                this.bindExp();
                this.bindVehicleDestroy();
                this.bindAchievementsEarned();
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
            },

            bindKills: async function(): Promise<void> {
                this.killsOrDeaths = Loadable.loading();
                this.killsOrDeaths = await KillStatApi.getSessionKills(this.sessionID);
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

            bindReconnects: async function(): Promise<void> {
                if (this.session.state != "loaded" || this.character.state != "loaded") {
                    this.reconnects = Loadable.idle();
                    return;
                }

                this.reconnects = Loadable.loading();
                this.reconnects = await RealtimeReconnectApi.getByInterval(this.session.data.start, this.session.data.end ?? new Date());
                if (this.reconnects.state == "loaded" && this.character.state == "loaded") {
                    this.reconnects.data = this.reconnects.data.filter(iter => iter.worldID == this.character.data.worldID);
                }
            },

            checkAllAndScroll: function(): void {
                if (this.killsOrDeaths.state != "loaded" || this.vehicleDestroy.state != "loaded"
                    || this.achievementsEarned.state != "loaded" || this.exp.state != "loaded") {

                    return;
                }

                // let vue update and render the elements
                this.$nextTick(async () => {
                    // wait 1000ms for all children to render
                    // TODO: there's gotta be a better way to let all children render first lol
                    await new Promise((resolve) => setTimeout(resolve, 1000));
                    const id: string = location.hash.slice(1); // .hash includes the #, remove it
                    document.getElementById(id)?.scrollIntoView();
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

            kills: function(): ExpandedKillEvent[] {
                if (this.killsOrDeaths.state != "loaded" || this.session.state != "loaded") {
                    return [];
                }
                return this.killsOrDeaths.data.filter(iter => {
                    return iter.event.attackerCharacterID == (this.session as any).data.characterID
                        && iter.event.attackerTeamID != iter.event.killedTeamID;
                });
            },

            deaths: function(): ExpandedKillEvent[] {
                if (this.killsOrDeaths.state != "loaded" || this.session.state != "loaded") {
                    return [];
                }
                return this.killsOrDeaths.data.filter(iter => {
                    return iter.event.revivedEventID == null && iter.event.killedCharacterID == (this.session as any).data.characterID;
                });
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
            }
        },

        components: {
            SessionViewerKills, SessionViewerGeneral, SessionViewerExp, SessionViewerTrends, SessionActionLog,
            SessionViewerSpawns, SessionViewerExpBreakdown, SessionAchievementsEarned, SessionItemAdded,
            ChartTimestamp,
            InfoHover,
            Busy,
            Collapsible,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default SessionViewer;
</script>