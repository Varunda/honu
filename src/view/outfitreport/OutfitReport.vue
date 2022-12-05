<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <a href="/report">Report</a>
                </li>
            </honu-menu>

            <div>
                <table class="table table-sm mr-2">
                    <tr>
                        <th>Controls</th>
                        <td>
                            <a v-if="show.controls == true" @click="show.controls = false" href="#">
                                Hide
                            </a>

                            <a v-else @click="show.controls = true" href="#">
                                Show
                            </a>
                        </td>
                    </tr>

                    <tr>
                        <th></th>
                        <td>
                            <a href="/report">New</a>
                        </td>
                    </tr>

                    <tr>
                        <td>&nbsp;</td>
                        <td></td>
                    </tr>
                </table>

            </div>

            <div v-if="isDone == true">
                <table class="table table-sm">
                    <tr>
                        <td><b>Start</b></td>
                        <td>{{parameters.periodStart | moment}}</td>
                    </tr>

                    <tr>
                        <td><b>End</b></td>
                        <td>{{parameters.periodEnd | moment}}</td>
                    </tr>

                    <tr>
                        <td><b>Duration</b></td>
                        <td>{{(parameters.periodEnd.getTime() - parameters.periodStart.getTime()) / 1000 | mduration}}</td>
                    </tr>

                    <tr>
                    </tr>
                </table>
            </div>
        </div>

        <div class="btn-group w-100 mb-2" v-if="show.controls == true">
            <button @click="show.logs = !show.logs" type="button" class="btn border" :class="[ show.logs ? 'btn-primary' : 'btn-secondary' ]">
                Logs
            </button>

            <button @click="show.gen = !show.gen" type="button" class="btn border" :class="[ show.gen ? 'btn-primary' : 'btn-secondary' ]">
                Show generator
            </button>

            <button @click="show.debug = !show.debug" type="button" class="btn border" :class="[ show.debug ? 'btn-primary' : 'btn-secondary' ]">
                Show debug
            </button>

            <a href="/report" class="btn btn-warning border">
                Reset
            </a>
        </div>

        <div v-if="show.logs == true" style="height: 300px; overflow-y: scroll;" class="container-fluid">
            <div v-for="msg in logs" class="row">
                <div class="col-2" style="font-family: monospace;">
                    {{msg.when | moment}}
                </div>

                <div class="col-10">
                    {{msg.message}}
                </div>
            </div>
        </div>

        <div v-if="show.gen == true" class="mb-2">
            <h2 class="wt-header">
                Generator settings
            </h2>

            <div class="mb-3">
                <h5>Time range</h5>

                <div v-if="errors.badTime" class="alert alert-danger text-center">
                    The start time cannot come after the end time
                </div>

                <div v-if="errors.longTime" class="alert alert-danger text-center">
                    The time range cannot be larger than 8 hours
                </div>

                <div class="input-grid-col3" style="grid-template-columns:min-content 1fr min-content;">
                    <div class="input-cell input-group-text input-group-prepend">
                        Start time
                        <info-hover text="When the time period the report covers will start"></info-hover>
                    </div>

                    <div class="input-cell">
                        <date-time-input v-model="periodStart" class="form-control" :class="{ 'is-invalid': errors.badTime }"></date-time-input>
                    </div>

                    <div class="input-cell px-2">
                        <button @click="setRelativeStart(2, 0)" type="button" class="btn btn-primary">
                            -2 hours
                        </button>
                    </div>

                    <div class="input-cell input-group-text input-group-prepend">
                        End time
                        <info-hover text="When the time period the report covers will end. Must be after the start time"></info-hover>
                    </div>

                    <div class="input-cell">
                        <date-time-input v-model="periodEnd" class="form-control" :class="{ 'is-invalid': errors.badTime }"></date-time-input>
                    </div>

                    <div class="input-cell px-2">
                        <button @click="zeroHourEnd" type="button" class="btn btn-primary">
                            Set to current hour
                        </button>
                    </div>
                </div>
            </div>

            <div class="mb-3">
                <h5>Add outfits/characters</h5>

                <div v-if="errors.noPlayers" class="alert alert-danger text-center">
                    No players have been given. Add an outfit or character
                </div>

                <div style="grid-template-columns: min-content 1fr min-content" class="input-grid-col3">

                    <div class="input-cell">
                        Add outfits
                    </div>
                    <div class="input-cell"></div>
                    <div class="input-cell"></div>

                    <!-- Outfit by tag -->
                    <div class="input-cell input-group-text input-group-prepend">
                        Outfit by tag
                        <info-hover text="Add an outfit by it's tag"></info-hover>
                    </div>

                    <div class="input-cell">
                        <input v-model="search.outfitTag" type="text" class="form-control" @keyup.enter="searchOutfitTag" />
                    </div>

                    <div class="input-cell px-2">
                        <button @click="searchOutfitTag" type="button" class="btn btn-primary">
                            Add
                        </button>
                    </div>

                    <!-- Outfit of character -->
                    <div class="input-cell input-group-text input-group-prepend">
                        Outfit of character
                        <info-hover text="Add the outfit of a character"></info-hover>
                    </div>

                    <div class="input-cell">
                        <input v-model="search.outfitOfCharacter" type="text" class="form-control" @keyup.enter="searchOutfitOfCharacter" />
                    </div>

                    <div class="input-cell px-2">
                        <button @click="searchOutfitOfCharacter" type="button" class="btn btn-primary">
                            Add
                        </button>
                    </div>

                    <!-- outfit by id -->
                    <div class="input-cell input-group-text input-group-prepend">
                        Outfit by ID
                        <info-hover text="Add a outfit directly by ID"></info-hover>
                    </div>

                    <div class="input-cell">
                        <input v-model="search.outfitID" type="text" class="form-control" @keyup.enter="searchOutfitID" />
                    </div>

                    <div class="input-cell px-2">
                        <button @click="searchOutfitID" type="button" class="btn btn-primary">
                            Add
                        </button>
                    </div>

                    <div class="input-cell mt-3">
                        Add characters
                    </div>
                    <div class="input-cell"></div>
                    <div class="input-cell"></div>

                    <!-- character by name -->
                    <div class="input-cell input-group-text input-group-prepend">
                        Character by name
                        <info-hover text="Add a character by their name"></info-hover>
                    </div>

                    <div class="input-cell">
                        <input v-model="search.characterName" type="text" class="form-control" @keyup.enter="searchCharacterName" />
                    </div>

                    <div class="input-cell px-2">
                        <button @click="searchCharacterName" type="button" class="btn btn-primary">
                            Add
                        </button>
                    </div>

                    <!-- character by id -->
                    <div class="input-cell input-group-text input-group-prepend">
                        Character by ID
                        <info-hover text="Add a character directly by ID"></info-hover>
                    </div>

                    <div class="input-cell">
                        <input v-model="search.characterID" type="text" class="form-control" @keyup.enter="searchCharacterID" />
                    </div>

                    <div class="input-cell px-2">
                        <button @click="searchCharacterID" type="button" class="btn btn-primary">
                            Add
                        </button>
                    </div>
                </div>
            </div>

            <div class="mb-3">
                <h5>Set faction (for NSO)</h5>

                <div class="input-group">
                    <select v-model.number="teamID" class="form-control">
                        <option :value="1">VS</option>
                        <option :value="2">NC</option>
                        <option :value="3">TR</option>
                    </select>
                </div>
            </div>

            <div v-if="show.debug" class="mb-3 input-group">
                <input v-model="generator" type="text" class="form-control" @keyup.enter="start" />
                <button @click="updateGenerator" type="button" class="btn btn-secondary input-group-append">
                    Update
                </button>
            </div>
            
            <div>
                <h5>
                    Outfits and characters used
                </h5>

                <table class="table table-sm">
                    <tr class="table-secondary">
                        <td colspan="2">Outfits</td>
                    </tr>

                    <tr v-for="outfit in outfits">
                        <td>
                            [{{outfit.tag}}]
                            {{outfit.name}}
                        </td>
                        <td>
                            <a @click="removeOutfit(outfit.id)">
                                &times;
                            </a>
                        </td>
                    </tr>
                </table>

                <table class="table table-sm">
                    <tr class="table-secondary">
                        <td colspan="2">Characters</td>
                    </tr>

                    <tr v-for="char in characters">
                        <td>
                            <span v-if="char.outfitID != null">
                                [{{char.outfitTag}}]
                            </span>
                            {{char.name}}
                        </td>
                        <td>
                            <a @click="removeCharacter(char.id)">
                                &times;
                            </a>
                        </td>
                    </tr>
                </table>
            </div>

            <div class="mb-3 w-100 border-top pt-3">
                <div v-if="hasErrors" class="text-center">
                    There are errors above. Fix them to generate the report
                </div>

                <button @click="start" type="button" class="btn btn-lg btn-primary w-100" :disabled="connected == true || hasErrors">
                    Generate with {{outfits.length}} outfits and {{characters.length}} characters
                </button>
            </div>
        </div>

        <div id="generation-progress" class="collapse">
            <div v-if="isMaking == true" class="text-center">
                <h2 v-if="reportState == 'not_stated'">
                    <busy class="honu-busy-lg"></busy>
                    Pending report createion
                </h2>

                <h2 v-else-if="reportState == 'parsing_generator'">
                    <busy class="honu-busy-lg"></busy>
                    Parsing report 
                </h2>

                <h2 v-else-if="reportState == 'getting_sessions'">
                    <busy class="honu-busy-lg"></busy>
                    Loading session data
                </h2>

                <div v-else-if="reportState == 'getting_killdeaths' || reportState == 'getting_exp' || reportState == 'getting_vehicle_destroy' || reportState == 'getting_player_control'">
                    <h2>
                        <busy class="honu-busy-lg"></busy>
                        Loading events:
                        <span v-if="reportState == 'getting_killdeaths'">
                            kill and death
                        </span>
                        <span v-else-if="reportState == 'getting_exp'">
                            experience
                        </span>
                        <span v-else-if="reportState == 'getting_vehicle_destroy'">
                            vehicle destroy
                        </span>
                        <span v-else-if="reportState == 'getting_player_control'">
                            player capture/defend
                        </span>
                    </h2>

                    <h5>
                        Kills: {{report.kills.length | locale}};
                        Deaths: {{report.deaths.length | locale}};
                        Exp: {{report.experience.length | locale}};
                        Vehicle: {{report.vehicleDestroy.length | locale}};
                        Control: {{report.playerControl.length | locale}}
                    </h5>

                    <progress-bar :total="trackedCharacters.length" :progress="progress.killdeath" :color="progressKillDeathColor">
                        Kill/Death:
                    </progress-bar>
                    <progress-bar :total="trackedCharacters.length" :progress="progress.exp" :color="progressExpColor">
                        Experience:
                    </progress-bar>
                    <progress-bar :total="trackedCharacters.length" :progress="progress.vehicleDestroy" :color="progressVehicleDestroyColor">
                        Vehicle destroy:
                    </progress-bar>
                    <progress-bar :total="trackedCharacters.length" :progress="progress.playerControl" :color="progressPlayerControlColor">
                        Player capture/defend:
                    </progress-bar>

                    <h5 v-if="trackedCharacters.length > 50" class="mt-2">
                        Loading data for {{trackedCharacters.length}} characters
                        <template v-if="trackedCharacters.length > 20">
                            <br />
                            <span v-if="trackedCharacters.length > 100">This is a lot of characters. This may take up to 10 minutes</span>
                            <span v-else-if="trackedCharacters.length > 50">This is a lot of characters. This may take 5 minutes</span>
                            <span v-else-if="trackedCharacters.length > 25">This is a lot of characters. This may take a couple minutes</span>
                        </template>
                    </h5>
                </div>

                <h2 v-else-if="reportState == 'getting_facility_control'">
                    <busy class="honu-busy-lg"></busy>
                    Loading facility capture/defend events
                </h2>

                <h2 v-else-if="reportState == 'getting_characters'">
                    <busy class="honu-busy-lg"></busy>
                    Caching characters
                </h2>

                <h2 v-else-if="reportState == 'getting_outfits'">
                    <busy class="honu-busy-lg"></busy>
                    Caching outfits
                </h2>

                <h2 v-else-if="reportState == 'getting_facilities'">
                    <busy class="honu-busy-lg"></busy>
                    Caching facilities
                </h2>

                <h2 v-else-if="reportState == 'getting_items'">
                    <busy class="honu-busy-lg"></busy>
                    Caching items
                </h2>

                <h2 v-else-if="reportState == 'getting_reconnects'">
                    <busy class="honu-busy-lg"></busy>
                    Loading reconnects
                </h2>
            </div>
        </div>

        <div v-if="isDone == true">
            <div v-if="report.sessions.length == 0" class="text-center text-danger">
                No activity recorded during selected time period
            </div>

            <div v-else>
                <report-header :report="report" :parameters="parameters"></report-header>

                <report-population :report="report" :parameters="parameters"></report-population>

                <report-per-minute-graph :report="report" :parameters="parameters"></report-per-minute-graph>

                <report-class-breakdown :report="report" :parameters="parameters"></report-class-breakdown>

                <report-control-breakdown :report="report" :parameters="parameters"></report-control-breakdown>

                <report-support-breakdown :report="report" :parameters="parameters"></report-support-breakdown>

                <report-exp-breakdown :report="report" :parameters="parameters"></report-exp-breakdown>

                <report-winter :report="report" :parameters="parameters"></report-winter>

                <report-outfit-versus :report="report" :parameters="parameters"></report-outfit-versus>

                <report-weapon-breakdown :report="report" :parameters="parameters"></report-weapon-breakdown>

                <report-player-list :report="report" :parameters="parameters"></report-player-list>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";
    import { Loading, Loadable } from "Loading";

    import { KillEvent, KillStatApi } from "api/KillStatApi";
    import { ExperienceType, ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { ItemApi, PsItem } from "api/ItemApi";
    import { OutfitApi, PsOutfit } from "api/OutfitApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { Session } from "api/SessionApi";
    import { FacilityControlEvent, FacilityControlEventApi } from "api/FacilityControlEventApi";
    import { PlayerControlEvent, PlayerControlEventApi } from "api/PlayerControlEventApi";
    import { PsFacility, MapApi } from "api/MapApi";
    import { RealtimeReconnectEntry } from "api/RealtimeReconnectapi";
    import { ItemCategory } from "api/ItemCategoryApi";
    import { VehicleDestroyEvent, VehicleDestroyEventApi } from "api/VehicleDestroyEventApi";

    import Report, { ReportParameters, PlayerMetadata, PlayerMetadataGenerator } from "./Report";

    import "MomentFilter";
    import DateUtil from "util/Date";

    import ReportClassBreakdown from "./components/ReportClassBreakdown.vue";
    import ReportPlayerList from "./components/ReportPlayerList.vue";
    import ReportOutfitVersus from "./components/ReportOutfitVersus.vue";
    import ReportWeaponBreakdown from "./components/ReportWeaponBreakdown.vue";
    import ReportSupportBreakdown from "./components/ReportSupportBreakdown.vue";
    import ReportWinter from "./components/ReportWinter.vue";
    import ReportControlBreakdown from "./components/ReportControlBreakdown.vue";
    import ReportHeader from "./components/ReportHeader.vue";
    import ReportPopulation from "./components/ReportPopulation.vue";
    import ReportPerMinuteGraph from "./components/ReportPerMinuteGraph.vue";
    import ReportExpBreakdown from "./components/ReportExpBreakdown.vue";
    import ProgressBar from "./components/ProgressBar.vue";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import DateTimeInput from "components/DateTimeInput.vue";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";

    import { InfantryDamage, InfantryDamageEntry } from "./InfantryDamage";

    type Message = {
        when: Date;
        message: string;
    };

    export const OutfitReport = Vue.extend({
        data: function() {
            return {
                logs: [] as Message[],

                show: {
                    controls: true as boolean,
                    logs: false as boolean,
                    debug: false as boolean,
                    gen: false as boolean,
                },

                makeOnConnection: false as boolean,
                hasErrored: false as boolean,

                isDone: false as boolean,
                isMaking: false as boolean,

                periodStart: new Date() as Date,
                periodEnd: new Date() as Date,
                teamID: null as number | null,

                search: {
                    outfitTag: "" as string,
                    outfitName: "" as string,
                    outfitID: "" as string,
                    outfitOfCharacter: "" as string,
                    characterName: "" as string,
                    characterID: "" as string
                },

                errors: {
                    badTime: false as boolean,
                    longTime: false as boolean,
                    noPlayers: false as boolean,
                },

                progress: {
                    killdeath: 0 as number,
                    exp: 0 as number,
                    vehicleDestroy: 0 as number,
                    playerControl: 0 as number
                },

                connection: null as sR.HubConnection | null,
                connected: false as boolean,

                report: new Report() as Report,
                parameters: new ReportParameters() as ReportParameters,
                trackedCharacters: [] as string[],
                reportState: "" as string,

                outfits: [] as PsOutfit[],
                characters: [] as PsCharacter[],

                generator: "" as string
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                $("#generation-progress").collapse();
                this.parseUrl();

                // Only set the defaults if no connection string has already been provided
                if (this.makeOnConnection == false) {
                    this.periodStart = DateUtil.zeroParts(this.periodStart, { minutes: true, seconds: true, milliseconds: true });
                    this.periodEnd = DateUtil.zeroParts(this.periodEnd, { minutes: true, seconds: true, milliseconds: true });

                    this.setRelativeStart(2, 0);
                }

                this.createConnection();
            });
        },

        methods: {
            log: function(msg: string): void {
                this.logs.unshift({
                    when: new Date(),
                    message: msg
                });

                if (this.logs.length > 100) {
                    this.logs = this.logs.slice(0, 100);
                }

                console.log("LOG: " + msg);
            },

            zeroHourEnd: function(): void {
                this.periodEnd = DateUtil.zeroParts(this.periodEnd, { minutes: true, seconds: true, milliseconds: true });
                this.updateGenerator();
            },

            setRelativeStart: function(hours: number, minutes: number): void {
                this.periodStart.setMonth(this.periodEnd.getMonth());
                this.periodStart.setDate(this.periodEnd.getDate());
                this.periodStart.setHours(this.periodEnd.getHours() - hours);
                this.periodStart.setMinutes(this.periodEnd.getMinutes() - minutes);
                this.periodStart.setSeconds(this.periodEnd.getSeconds());
                this.periodStart.setMilliseconds(this.periodEnd.getMilliseconds());
                this.periodStart = new Date(this.periodStart); // Needed as .setX isn't reactive
                this.updateGenerator();
            },

            parseUrl: function(): void {
                const parts: string[] = location.pathname.split("/").filter(iter => iter != "");
                console.log(parts);

                if (parts.length > 1) {
                    const gen: string = atob(parts[1]);
                    console.log(`Loaded generator '${gen}' from URL`);

                    this.show.gen = false;
                    this.show.controls = false;
                    this.generator = gen;
                    this.makeOnConnection = true;
                } else {
                    this.show.gen = true;
                }
            },

            searchOutfitTag: async function(): Promise<void> {
                const outfits: Loading<PsOutfit[]> = await OutfitApi.getByTag(this.search.outfitTag);
                if (outfits.state != "loaded") {
                    this.log(`failed to a single outfit with [${this.search.outfitTag}]`);
                    return;
                }

                if (outfits.data.length == 0) {
                    this.log(`Found 0 outfits with the tag ${this.search.outfitTag}`);
                    return;
                }

                outfits.data.sort((a, b) => b.id.localeCompare(a.id));
                this.addOutfits(outfits.data[0]);
                this.search.outfitTag = "";
            },

            searchOutfitID: async function(): Promise<void> {
                const outfit: Loading<PsOutfit> = await OutfitApi.getByID(this.search.outfitID);

                if (outfit.state == "loaded") {
                    this.addOutfits(outfit.data);
                    this.search.outfitID = "";
                }
            },

            searchOutfitOfCharacter: async function(): Promise<void> {
                const characters: Loading<PsCharacter[]> = await CharacterApi.getByName(this.search.outfitOfCharacter);

                if (characters.state != "loaded" || characters.data.length == 0) {
                    this.log(`failed to find characters with name ${this.search.outfitOfCharacter}`);
                    return;
                }

                characters.data.sort((a, b) => b.id.localeCompare(a.id));
                const character: PsCharacter = characters.data[0];

                if (character.outfitID == null) {
                    this.log(`Character ${character.name} is not in an outfit`);
                    return;
                }

                const outfits: Loading<PsOutfit> = await OutfitApi.getByID(character.outfitID);
                if (outfits.state != "loaded") {
                    this.log(`Failed to get outfit ID ${character.outfitID}`);
                    return;
                }

                this.addOutfits(outfits.data);
                this.search.outfitOfCharacter = "";
            },

            searchCharacterName: async function(): Promise<void> {
                const characters: Loading<PsCharacter[]> = await CharacterApi.getByName(this.search.characterName);
                if (characters.state != "loaded") {
                    this.log(`Failed to search for ${this.search.characterName}, got state ${characters.state} from Honu API`);
                    return;
                }

                if (characters.data.length == 0) {
                    this.log(`Found 0 characters with name ${this.search.characterName}`);
                    return;
                }

                characters.data.sort((a, b) => b.dateLastLogin.getTime() - a.dateLastLogin.getTime());
                this.addCharacters(characters.data[0]);
                this.search.characterName = "";
            },

            searchCharacterID: async function(): Promise<void> {
                throw `searchCharacterID not done yet`;
            },

            /**
             * Add outfits to the generator 
             * @param outfits Rest param of the outfits to add
             */
            addOutfits: function(...outfits: PsOutfit[]): void {
                for (const outfit of outfits) {
                    this.outfits.push(outfit);

                    if (this.teamID == null) {
                        this.teamID = outfit.factionID;
                    }
                }

                this.outfits.sort((a, b) => a.name.localeCompare(b.name));
                this.updateGenerator();
            },

            /**
             * Add characters to the generator
             * @param characters Rest param of the characters to add
             */
            addCharacters: function(...characters: PsCharacter[]): void {
                for (const c of characters) {
                    this.characters.push(c);

                    if (this.teamID == null) {
                        this.teamID = c.factionID;
                    }
                }

                this.characters.sort((a, b) => a.name.localeCompare(b.name));
                this.updateGenerator();
            },

            /**
             * Remove an outfit from the generator
             * @param outfitID
             */
            removeOutfit: function(outfitID: string): void {
                this.outfits = this.outfits.filter(iter => iter.id != outfitID);
            },

            /**
             * Remove a character from the generator
             * @param charID
             */
            removeCharacter: function(charID: string): void {
                this.characters = this.characters.filter(iter => iter.id != charID);
            },

            /**
             * Initalize the connection to the signalR hub
             */
            createConnection: function(): void {
                document.title = `Honu / Outfit Report`;
                this.log(`Starting signalR connection`);

                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/report")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .configureLogging(sR.LogLevel.Information)
                    .build();

                this.log(`Connecting...`);

                this.connection.on("SendParameters", this.onSendParameters);
                this.connection.on("SendError", this.onSendError);
                this.connection.on("UpdateState", this.onUpdateState);
                this.connection.on("SendCharacterIDs", this.onSendCharacterIDs);
                this.connection.on("SendKills", this.onSendKills);
                this.connection.on("SendDeaths", this.onSendDeaths);
                this.connection.on("SendExp", this.onSendExp);
                this.connection.on("SendVehicleDestroy", this.onSendVehicleDestroy);
                this.connection.on("SendPlayerControl", this.onSendPlayerControl);
                this.connection.on("UpdateKills", this.onUpdateKills);
                this.connection.on("UpdateDeaths", this.onUpdateDeaths);
                this.connection.on("UpdateExp", this.onUpdateExp);
                this.connection.on("UpdateVehicleDestroy", this.onUpdateVehicleDestroy);
                this.connection.on("UpdateItems", this.onUpdateItems);
                this.connection.on("UpdateItemCategories", this.onUpdateItemCategories);
                this.connection.on("UpdateOutfits", this.onUpdateOutfits);
                this.connection.on("UpdateCharacters", this.onUpdateCharacters);
                this.connection.on("UpdateSessions", this.onUpdateSessions);
                this.connection.on("UpdateControls", this.onUpdateControls);
                this.connection.on("UpdatePlayerControls", this.onUpdatePlayerControls);
                this.connection.on("UpdateFacilities", this.onUpdateFacilities);
                this.connection.on("UpdateReconnects", this.onUpdateReconnect);
                this.connection.on("UpdateExperienceTypes", this.onUpdateExperienceTypes);

                this.connection.start().then(() => {
                    if (this.makeOnConnection == true && this.generator != "") {
                        this.log(`Connected! Generator string is set, starting`);
                        console.log(`Generator is set: ${this.generator}`);
                        this.start();
                    } else {
                        this.log(`Connected! Waiting for generator string`);
                    }
                }).catch(err => {
                    console.error(err);
                });

                this.connection.onreconnected(() => {
                    this.log(`Reconnected`);
                    this.connected = true;
                    console.log(`reconnected`);
                });

                this.connection.onclose((err?: Error) => {
                    this.log(`Connection closed: ${err}`);
                    if (err) {
                        console.error("onclose: ", err);
                    }
                });

                this.connection.onreconnecting((err?: Error) => {
                    this.log(`Reconnecting... ${err}`);
                    if (err) {
                        console.error("onreconnecting: ", err);
                    }
                });
            },

            /**
             * Start the generation process, sending the request to the hub by calling 'GenerateReport'
             */
            start: function(): void {
                if (this.connection == null) {
                    return this.log(`connection is null, cannot start generation`);
                }

                this.log(`Sending generator string: '${this.generator}'`);
                this.parameters.generator = this.generator;
                history.pushState({}, "", `/report/${this.generator64}`);

                this.isMaking = true;
                this.isDone = false;
                this.show.gen = false;
                this.show.logs = true;

                this.connection.invoke("GenerateReport", this.generator).then((response: any) => {
                    this.isDone = true;

                    //this.report.generator = `#${this.report.id};`;
                    this.generator = `#${this.parameters.id};`;
                    console.log(`ID of report '${this.parameters.id}'`);
                    history.pushState({}, "", `/report/${this.generator64}`);

                    const metadatas: PlayerMetadata[] = PlayerMetadataGenerator.generate(this.report);
                    for (const metadata of metadatas) {
                        this.report.playerMetadata.set(metadata.ID, metadata);
                    }

                    const damage: InfantryDamageEntry[] = InfantryDamage.get(this.report);
                    for (const d of damage) {
                        this.report.playerInfantryDamage.set(d.characterID, d);
                        //const c: PsCharacter | undefined = this.report.characters.get(d.characterID);
                        //console.log(`${d.characterID}/${c?.name} dealt ${d.totalDamage} damage; kills:`, d.kills, "; assists: ", d.assists);
                    }

                    //this.closeConnection();

                    setTimeout(() => {
                        if (this.hasErrored == false) {
                            this.show.logs = false;
                        }
                        this.isMaking = false;
                    }, 2000);
                }).catch((err: any) => {
                    console.error(err);
                });
            },

            /**
             * Close the connection to the signalR hub, prevents future requests from being made unless the connection is re-opened
             */
            closeConnection: function(): void {
                if (this.connection != null) {
                    this.connection.stop().then(() => {
                        this.connection = null;
                    }).catch((err: any) => {
                        console.error(`Failed to close connection: ${err}`);
                    });
                }
            },

            /**
             * Update the generator string based on the parameters used
             */
            updateGenerator: function(): void {
                console.log(`Start: ${this.periodStart} = ${this.periodStart.toISOString()}`);
                console.log(`End: ${this.periodEnd} = ${this.periodStart.toISOString()}`);
                console.log(`Outfits: [${this.outfits.join(", ")}]`);

                const start: number = Math.floor(this.periodStart.getTime() / 1000);
                const end: number = Math.floor(this.periodEnd.getTime() / 1000);

                this.errors.longTime = end - start > 60 * 60 * 12;

                this.errors.badTime = start > end;
                this.errors.noPlayers = (this.outfits.length == 0 && this.characters.length == 0);

                const outfits: string = this.outfits.map(iter => `o${iter.id};`).join("");
                const chars: string = this.characters.map(iter => `+${iter.id};`).join("");
                const teamID: number = this.teamID ?? -1;

                const gen: string = `${start},${end},${teamID};${outfits}${chars}`;
                this.generator = gen;
                console.log(`Generator used: ${gen}`);
            },

            /**
             * Take the user to a page where they can start a new report
             */
            newReport: function(): void {
                const conf: boolean = confirm(`Are you sure you want to leave this report and make a new one?`);

                if (conf == true) {
                    location.href = "/report";
                }
            },

            onSendError: function(err: string): void {
                this.log("ERROR: " + err);
                this.show.logs = true;
                this.hasErrored = true;
            },

            onUpdateState: function(state: string): void {
                this.log("STATE: " + state);
                this.reportState = state;
            },

            onSendCharacterIDs: function(ids: string[]): void {
                this.log(`Loaded data from ${ids.length} characters`);
                this.trackedCharacters = ids;
            },

            onSendParameters: function(parms: ReportParameters): void {
                this.parameters = {
                    ...parms,
                    timestamp: new Date(parms.timestamp + "Z"),
                    periodStart: new Date(parms.periodStart + "Z"),
                    periodEnd: new Date(parms.periodEnd + "Z")
                };
                console.log(`set generator to ${this.parameters.generator} from ${parms.generator}`);

                this.periodStart = new Date(this.parameters.periodStart);
                this.periodEnd = new Date(this.parameters.periodEnd);

                console.log(this.parameters);
                this.report.parameters = this.parameters;
                this.log(`Got parameters: ${JSON.stringify(this.parameters)}`);
            },

            onSendKills: function(charID: string, events: KillEvent[]): void {
                events = events.map(iter => KillStatApi.parseKillEvent(iter));
                this.report.kills.push(...events);
                ++this.progress.killdeath;
            },

            onSendDeaths: function(charID: string, events: KillEvent[]): void {
                events = events.map(iter => KillStatApi.parseKillEvent(iter));
                this.report.deaths.push(...events);
            },

            onSendExp: function(charID: string, events: ExpEvent[]): void {
                events = events.map(iter => ExpStatApi.parseExpEvent(iter));
                this.report.experience.push(...events);
                ++this.progress.exp;
            },

            onSendVehicleDestroy: function(charID: string, events: VehicleDestroyEvent[]): void {
                events = events.map(iter => VehicleDestroyEventApi.parse(iter));
                this.report.vehicleDestroy.push(...events);
                ++this.progress.vehicleDestroy;
            },

            onSendPlayerControl: function(charID: string, events: PlayerControlEvent[]): void {
                events = events.map(iter => PlayerControlEventApi.parse(iter));
                this.report.playerControl.push(...events);
                ++this.progress.playerControl;
            },

            onUpdateKills: function(ev: KillEvent[]): void {
                this.report.kills = ev.map(iter => KillStatApi.parseKillEvent(iter));
                this.log(`Loaded ${this.report.kills.length} kill`);
            },

            onUpdateDeaths: function(ev: KillEvent[]): void {
                this.report.deaths = ev.map(iter => KillStatApi.parseKillEvent(iter));
                this.log(`Loaded ${this.report.deaths.length} deaths`);
            },

            onUpdateExp: function(ev: ExpEvent[]): void {
                this.report.experience = ev.map(iter => ExpStatApi.parseExpEvent(iter));
                this.log(`Loaded ${this.report.experience.length} experience events`);
                this.progress.exp = this.trackedCharacters.length;
            },

            onUpdateVehicleDestroy: function(ev: VehicleDestroyEvent[]): void {
                this.report.vehicleDestroy = ev.map(iter => VehicleDestroyEventApi.parse(iter));
                this.log(`Loaded ${this.report.vehicleDestroy.length} vehicle destroy events`);
            },

            onUpdatePlayerControls: function(ev: PlayerControlEvent[]): void {
                this.report.playerControl = ev.map(iter => PlayerControlEventApi.parse(iter));
            },

            onUpdateControls: function(ev: FacilityControlEvent[]): void {
                this.report.control = ev.map(iter => FacilityControlEventApi.parse(iter));
            },

            onUpdateFacilities: function(ev: PsFacility[]): void {
                for (const fac of ev) {
                    this.report.facilities.set(fac.facilityID, fac);
                }
                this.log(`Loaded ${this.report.facilities.size} facilities`);
            },

            onUpdateItems: function(items: PsItem[]): void {
                for (const item of items) {
                    this.report.items.set(item.id, item);
                }
                this.log(`Loaded ${this.report.items.size} items`);
            },

            onUpdateItemCategories: function(cats: ItemCategory[]): void {
                for (const cat of cats) {
                    this.report.itemCategories.set(cat.id, cat);
                }
                this.log(`Loaded ${this.report.itemCategories.size} item categories`);
            },

            onUpdateCharacters: function(chars: PsCharacter[]): void {
                for (const c of chars) {
                    this.report.characters.set(c.id, c);
                }
                this.log(`Loaded ${this.report.characters.size} characters`);
            },

            onUpdateOutfits: function(outfits: PsOutfit[]): void {
                for (const outfit of outfits) {
                    this.report.outfits.set(outfit.id, outfit);
                }
                this.log(`Loaded ${this.report.outfits.size} outfits`);
            },

            onUpdateSessions: function(sessions: Session[]): void {
                for (const session of sessions) {
                    session.start = new Date(session.start);
                    session.end = (session.end == null) ? null : new Date(session.end);
                }
                this.report.sessions = sessions;

                this.report.trackedCharacters = sessions.map(iter => iter.characterID)
                    .filter((v, i, arr) => arr.indexOf(v) == i);

                this.log(`Loaded ${this.report.sessions.length} sessions`);
            },

            onUpdateReconnect: function(entries: RealtimeReconnectEntry[]): void {
                for (const entry of entries) {
                    entry.timestamp = new Date(entry.timestamp);
                }
                this.report.reconnects = entries;
                this.log(`Loaded ${this.report.reconnects.length} reconnects`);
            },

            onUpdateExperienceTypes: function(types: ExperienceType[]): void {
                for (const entry of types) {
                    this.report.experienceTypes.set(entry.id, entry);
                }
            }
        },

        watch: {
            periodStart: function(): void {
                if (this.isMaking == true) {
                    console.log(`currently making a report, not updating the generator, current gen: ${this.parameters.generator}`);
                    return;
                }
                this.updateGenerator();
            },

            periodEnd: function(): void {
                if (this.isMaking == true) {
                    console.log(`currently making a report, not updating the generator, current gen: ${this.parameters.generator}`);
                    return;
                }
                this.updateGenerator();
            },

            teamID: function(): void {
                if (this.isMaking == true) {
                    console.log(`currently making a report, not updating the generator, current gen: ${this.parameters.generator}`);
                    return;
                }
                this.updateGenerator();
            }
        },

        computed: {
            hasErrors: function(): boolean {
                return this.errors.noPlayers || this.errors.badTime || this.errors.longTime;
            },

            generator64: function(): string {
                return btoa(this.generator);
            },

            progressKillDeathColor: function(): string {
                if (this.reportState == "getting_killdeaths") {
                    return "primary";
                }
                if (this.progress.killdeath >= this.trackedCharacters.length) {
                    return "success";
                }
                return "info";
            },

            progressExpColor: function(): string {
                if (this.reportState == "getting_exp") {
                    return "primary";
                }
                if (this.progress.exp >= this.trackedCharacters.length) {
                    return "success";
                }
                return "info";
            },

            progressVehicleDestroyColor: function(): string {
                if (this.reportState == "getting_vehicle_destroy") {
                    return "primary";
                }
                if (this.progress.vehicleDestroy >= this.trackedCharacters.length) {
                    return "success";
                }
                return "info";
            },

            progressPlayerControlColor: function(): string {
                if (this.reportState == "getting_player_control") {
                    return "primary";
                }
                if (this.progress.playerControl >= this.trackedCharacters.length) {
                    return "success";
                }
                return "info";
            }

        },

        components: {
            DateTimeInput, InfoHover, Busy,
            ReportClassBreakdown, ReportPlayerList, ReportOutfitVersus, ReportWeaponBreakdown, ReportPerMinuteGraph, ReportExpBreakdown,
            ReportSupportBreakdown, ReportWinter, ReportControlBreakdown, ReportHeader, ReportPopulation,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ProgressBar
        }
    });
    export default OutfitReport;
</script>