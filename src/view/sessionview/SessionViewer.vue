<template>
    <div>
        <honu-menu class="flex-grow-1">
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
            <collapsible header-text="Session">
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
                                ({{exp.data.length}})
                            </span>
                        </td>
                    </tr>
                </table>
            </collapsible>

            <collapsible header-text="General">
                <div v-if="exp.state == 'loading' || killsOrDeaths.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-general v-else-if="exp.state == 'loaded' && killsOrDeaths.state == 'loaded'"
                    :session="session.data" :exp="exp.data" :kills="kills" :deaths="deaths">
                </session-viewer-general>
            </collapsible>

            <collapsible header-text="Kills">
                <div v-if="killsOrDeaths.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-kills v-else-if="killsOrDeaths.state == 'loaded'"
                    :kills="kills" :deaths="deaths" :session="session.data">
                </session-viewer-kills>
            </collapsible>

            <collapsible header-text="Experience">
                <div v-if="exp.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-exp v-else-if="exp.state == 'loaded'" :session="session.data" :exp="exp.data"></session-viewer-exp>
            </collapsible>

            <collapsible header-text="Trends">
                <div v-if="exp.state == 'loading' || killsOrDeaths.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-trends v-else-if="exp.state == 'loaded' && killsOrDeaths.state == 'loaded'"
                    :session="session.data" :kills="kills" :deaths="deaths" :exp="exp.data">
                </session-viewer-trends>
            </collapsible>

            <collapsible header-text="Action log">
                <div v-if="exp.state == 'loading' || killsOrDeaths.state == 'loading' || vehicleDestroy.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-action-log v-else-if="exp.state == 'loaded' && killsOrDeaths.state == 'loaded' && vehicleDestroy.state == 'loaded'"
                    :session="session.data" :kills="kills" :deaths="deaths" :exp="exp.data" :vehicle-destroy="vehicleDestroy.data">
                </session-action-log>
            </collapsible>

            <collapsible header-text="Routers & Sunderers">
                <div v-if="exp.state == 'loading'">
                    <busy style="max-height: 1.25rem;"></busy>
                    Loading...
                </div>

                <session-viewer-spawns v-else-if="exp.state == 'loaded'"
                    :session="session.data" :exp="exp.data">
                </session-viewer-spawns>
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
    import ChartTimestamp from "./components/ChartTimestamp.vue";

    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import Collapsible from "components/Collapsible.vue";

    import { ExpandedKillEvent, KillEvent, KillStatApi } from "api/KillStatApi";
    import { Experience, ExpandedExpEvent, ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { ExpandedVehicleDestroyEvent, VehicleDestroyEvent, VehicleDestroyEventApi } from "api/VehicleDestroyEventApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";

    export const SessionViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                sessionID: 0 as number,

                session: Loadable.idle() as Loading<Session>,
                character: Loadable.idle() as Loading<PsCharacter>,

                killsOrDeaths: Loadable.idle() as Loading<ExpandedKillEvent[]>,
                exp: Loadable.idle() as Loading<ExpandedExpEvent[]>,
                vehicleDestroy: Loadable.idle() as Loading<ExpandedVehicleDestroyEvent[]>
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
            },

            bindSession: async function(): Promise<void> {
                this.session = Loadable.loading();
                this.session = await SessionApi.getBySessionID(this.sessionID);

                if (this.session.state == "loaded") {
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
            },

            bindExp: async function(): Promise<void> {
                this.exp = Loadable.loading();
                this.exp = await ExpStatApi.getBySessionID(this.sessionID);
            },

            bindKills: async function(): Promise<void> {
                this.killsOrDeaths = Loadable.loading();
                this.killsOrDeaths = await KillStatApi.getSessionKills(this.sessionID);
            },

            bindVehicleDestroy: async function(): Promise<void> {
                this.vehicleDestroy = Loadable.loading();
                this.vehicleDestroy = await VehicleDestroyEventApi.getBySessionID(this.sessionID);
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
            }
        },

        components: {
            SessionViewerKills, SessionViewerGeneral, SessionViewerExp, SessionViewerTrends, SessionActionLog, SessionViewerSpawns, 
            ChartTimestamp,
            InfoHover,
            Busy,
            Collapsible,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default SessionViewer;
</script>