<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="#">Report</a>
            </h1>

            <div v-if="isDone == true">
                <table class="table table-sm">
                    <tr>
                        <td><b>Start</b></td>
                        <td>{{report.periodStart | moment}}</td>
                    </tr>

                    <tr>
                        <td><b>End</b></td>
                        <td>{{report.periodEnd | moment}}</td>
                    </tr>

                    <tr>
                        <td><b>Duration</b></td>
                        <td>{{(report.periodEnd.getTime() - report.periodStart.getTime()) / 1000 | mduration}}</td>
                    </tr>
                </table>
            </div>
        </div>

        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <div class="btn-group w-100 mb-2">
            <button @click="showLogs = !showLogs" type="button" class="btn btn-secondary">
                Logs
            </button>

            <button @click="isNew = !isNew" type="button" class="btn btn-secondary">
                Is new
            </button>

            <a href="/report" class="btn btn-secondary">
                Reset
            </a>
        </div>

        <div v-if="showLogs == true" style="height: 300px; overflow-y: scroll;" class="container-fluid">
            <div v-for="msg in logs" class="row">
                <div class="col-2" style="font-family: monospace;">
                    {{msg.when | moment}}
                </div>

                <div class="col-10">
                    {{msg.message}}
                </div>
            </div>
        </div>

        <div v-if="isNew == true" class="mb-2">
            <h2 class="wt-header">
                Generator settings
            </h2>

            <div class="input-group">
                <span class="input-group-prepend input-group-text">
                    Start time
                </span>
                <input v-model="periodStartInput" type="datetime-local" class="form-control" />
                <div class="input-group-append">
                    <button @click="setRelativeStart(2, 0)" type="button" class="btn btn-primary input-group-addon">
                        -2 hours
                    </button>
                </div>
            </div>

            <div class="input-group">
                <span class="input-group-prepend input-group-text">
                    End time
                </span>
                <input v-model="periodEndInput" type="datetime-local" class="form-control" />
                <div class="input-group-append">
                    <button @click="zeroHourEnd" type="button" class="btn btn-primary input-group-addon">
                        Set to current hour
                    </button>
                </div>
            </div>

            <div class="input-group">
                <span class="input-group-prepend input-group-text">
                    Outfit tag
                </span>

                <input v-model="search.outfitTag" type="text" class="form-control" @keyup.enter="searchTag" />
                
                <button @click="searchTag" class="btn btn-primary input-group-append">Add</button>
            </div>

            <div class="input-group">
                <span class="input-group-prepend input-group-text">
                    Character
                </span>

                <input v-model="search.characterName" type="text" class="form-control" @keyup.enter="searchCharacter" />

                <button @click="searchCharacter" class="btn btn-primary input-group-append">Add</button>
            </div>

            <input v-model="generator" type="text" class="form-control" @keyup.enter="start" />

            <button @click="updateGenerator" type="button" class="btn btn-secondary">
                Update
            </button>

            <button @click="start" type="button" class="btn btn-primary" :disabled="connected == true">
                Generate
            </button>

            <hr />

            <div>
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
        </div>

        <div id="generation-progress" class="collapse">
            <table v-if="isMaking == true" class="table table-sm">
                <tr class="table-secondary">
                    <td>Step</td>
                    <td>Done?</td>
                </tr>

                <tr :class="[ !steps.report ? 'table-warning' : 'table-success' ]">
                    <td>Submitting request</td>
                    <td>{{steps.report}}</td>
                </tr>

                <tr :class="[ !steps.kills ? 'table-warning' : 'table-success' ]">
                    <td>Getting kills</td>
                    <td>{{steps.kills}}</td>
                </tr>

                <tr :class="[ !steps.deaths ? 'table-warning' : 'table-success' ]">
                    <td>Getting deaths</td>
                    <td>{{steps.deaths}}</td>
                </tr>

                <tr :class="[ !steps.exp ? 'table-warning' : 'table-success' ]">
                    <td>Getting exp events</td>
                    <td>{{steps.exp}}</td>
                </tr>

                <tr :class="[ !steps.chars ? 'table-warning' : 'table-success' ]">
                    <td>Getting characters</td>
                    <td>{{steps.chars}}</td>
                </tr>

                <tr :class="[ !steps.outfits ? 'table-warning' : 'table-success' ]">
                    <td>Getting outfits</td>
                    <td>{{steps.outfits}}</td>
                </tr>

                <tr :class="[ !steps.items ? 'table-warning' : 'table-success' ]">
                    <td>Getting items</td>
                    <td>{{steps.items}}</td>
                </tr>

                <tr v-if="isDone == true" class="table-success">
                    <td colspan="2">
                        All done!
                    </td>
                </tr>
            </table>
        </div>

        <div v-if="isDone == true">
            <report-class-breakdown :report="report"></report-class-breakdown>

            <report-weapon-breakdown :report="report"></report-weapon-breakdown>

            <report-support-breakdown :report="report"></report-support-breakdown>

            <report-winter :report="report"></report-winter>

            <report-outfit-versus :report="report"></report-outfit-versus>

            <report-player-list :report="report"></report-player-list>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";
    import { Loading, Loadable } from "Loading";

    import { KillEvent, KillStatApi } from "api/KillStatApi";
    import { ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { PsItem } from "api/ItemApi";
    import { OutfitApi, PsOutfit } from "api/OutfitApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { Session } from "api/SessionApi";

    import Report, { PlayerMetadata, PlayerMetadataGenerator } from "./Report";

    import "MomentFilter";
    import DateUtil from "util/Date";

    import ReportClassBreakdown from "./components/ReportClassBreakdown.vue";
    import ReportPlayerList from "./components/ReportPlayerList.vue";
    import ReportOutfitVersus from "./components/ReportOutfitVersus.vue";
    import ReportWeaponBreakdown from "./components/ReportWeaponBreakdown.vue";
    import ReportSupportBreakdown from "./components/ReportSupportBreakdown.vue";
    import ReportWinter from "./components/ReportWinter.vue";

    import DateTimePicker from "components/DateTimePicker.vue";
    import InfoHover from "components/InfoHover.vue";

    type Message = {
        when: Date;
        message: string;
    };

    export const OutfitReport = Vue.extend({
        data: function() {
            return {
                logs: [] as Message[],
                showLogs: false as boolean,

                isNew: true as boolean,
                isDone: false as boolean,
                isMaking: false as boolean,

                periodStartInput: "" as string,
                periodStart: new Date() as Date,
                periodEndInput: "" as string,
                periodEnd: new Date() as Date,

                search: {
                    outfitTag: "" as string,
                    characterName: "" as string
                },

                steps: {
                    report: false as boolean,
                    kills: false as boolean,
                    deaths: false as boolean,
                    exp: false as boolean,
                    chars: false as boolean,
                    outfits: false as boolean,
                    items: false as boolean,
                    sessions: false as boolean,
                },

                connection: null as sR.HubConnection | null,
                connected: false as boolean,

                report: new Report() as Report,

                outfits: [] as PsOutfit[],
                characters: [] as PsCharacter[],

                generator: "" as string,
                genB64: "" as string
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                $("#generation-progress").collapse();
                this.createConnection();

                this.parseUrl();

                this.periodStart.setMilliseconds(0);
                this.periodStart.setSeconds(0);
                this.periodStart.setMinutes(0);

                this.periodEnd.setMilliseconds(0);
                this.periodEnd.setSeconds(0);
                this.periodEnd.setMinutes(0);

                this.periodStartInput = DateUtil.getLocalDateString(this.periodStart);
                this.periodEndInput = DateUtil.getLocalDateString(this.periodEnd);
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
            },

            zeroHourEnd: function(): void {
                this.periodEnd.setMilliseconds(0);
                this.periodEnd.setSeconds(0);
                this.periodEnd.setMinutes(0);
                this.periodEndInput = DateUtil.getLocalDateString(this.periodEnd);
                this.updateGenerator();
            },

            setRelativeStart: function(hours: number, minutes: number): void {
                this.periodStart.setMilliseconds(this.periodEnd.getMilliseconds());
                this.periodStart.setSeconds(this.periodEnd.getSeconds());
                this.periodStart.setMinutes(this.periodEnd.getMinutes() - minutes);
                this.periodStart.setHours(this.periodEnd.getHours() - hours);
                this.periodStartInput = DateUtil.getLocalDateString(this.periodStart);
                this.updateGenerator();
            },

            parseUrl: function(): void {
                const parts: string[] = location.pathname.split("/").filter(iter => iter != "");
                console.log(parts);

                if (parts.length > 1) {
                    const gen: string = parts[1];
                    console.log(`Loaded generator '${gen}' from URL`);

                    this.isNew = false;
                    this.generator = atob(gen);
                }
            },

            searchTag: async function(): Promise<void> {
                const outfits: Loading<PsOutfit[]> = await OutfitApi.getByTag(this.search.outfitTag);
                if (outfits.state != "loaded") {
                    this.log(``);
                    return;
                }

                if (outfits.data.length == 0) {
                    this.log(`Found 0 outfits with the tag ${this.search.outfitTag}`);
                    return;
                }

                outfits.data.sort((a, b) => b.id.localeCompare(a.id));
                this.outfits.push(outfits.data[0]);
                this.search.outfitTag = "";
                this.updateGenerator();
            },

            searchCharacter: async function(): Promise<void> {
                const characters: Loading<PsCharacter[]> = await CharacterApi.getByName(this.search.characterName);
                if (characters.state != "loaded") {
                    this.log(`Failed to search for ${this.search.characterName}, got state ${characters.state} from Honu API`);
                    return;
                }

                if (characters.data.length == 0) {
                    this.log(`Found 0 characters with name ${this.search.characterName}`);
                    return;
                }

                characters.data.sort((a, b) => b.id.localeCompare(a.id));

                this.characters.push(characters.data[0]);
                this.search.characterName = "";
                this.updateGenerator();
            },

            removeOutfit: function(outfitID: string): void {
                this.outfits = this.outfits.filter(iter => iter.id != outfitID);
            },

            removeCharacter: function(charID: string): void {
                this.characters = this.characters.filter(iter => iter.id != charID);
            },

            createConnection: function(): void {
                document.title = `Honu / Outfit Report`;
                this.log(`Starting signalR connection`);

                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/report")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .configureLogging(sR.LogLevel.Information)
                    .build();

                this.log(`Connecting...`);

                this.connection.on("SendReport", this.onSendReport);
                this.connection.on("SendError", this.onSendError);
                this.connection.on("UpdateCharacterIDs", this.onUpdateCharacterIDs);
                this.connection.on("UpdateKills", this.onUpdateKills);
                this.connection.on("UpdateDeaths", this.onUpdateDeaths);
                this.connection.on("UpdateExp", this.onUpdateExp);
                this.connection.on("UpdateItems", this.onUpdateItems);
                this.connection.on("UpdateOutfits", this.onUpdateOutfits);
                this.connection.on("UpdateCharacters", this.onUpdateCharacters);
                this.connection.on("UpdateSessions", this.onUpdateSessions);

                this.connection.start().then(() => {
                    if (this.generator != "") {
                        this.log(`Connected! Generator string is set, starting`);
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

            start: function(): void {
                if (this.connection == null) {
                    return this.log(`connection is null, cannot start generation`);
                }

                this.log(`Sending generator string: '${this.generator}'`);
                this.genB64 = btoa(this.generator);
                this.report.generator = this.generator;
                history.pushState({}, "", `/report/${this.genB64}`);

                this.isMaking = true;
                this.isDone = false;

                this.connection.invoke("GenerateReport", this.generator).then((response: any) => {
                    this.isDone = true;
                    this.isNew = false;

                    const metadatas: PlayerMetadata[] = PlayerMetadataGenerator.generate(this.report);
                    for (const metadata of metadatas) {
                        this.report.playerMetadata.set(metadata.ID, metadata);
                    }

                    setTimeout(() => {
                        this.showLogs = false;
                        this.isMaking = false;
                    }, 2000);
                }).catch((err: any) => {
                    console.error(err);
                });
            },

            updateGenerator: function(): void {
                console.log(`Start: ${this.periodStart} = ${this.periodStart.toISOString()}`);
                console.log(`End: ${this.periodEnd} = ${this.periodStart.toISOString()}`);

                console.log(`Outfits: [${this.outfits.join(", ")}]`);

                this.periodStart = new Date(this.periodStartInput);
                this.periodEnd = new Date(this.periodEndInput);

                const start: number = Math.floor(this.periodStart.getTime() / 1000);
                const end: number = Math.floor(this.periodEnd.getTime() / 1000);

                const outfits: string = this.outfits.map(iter => `o${iter.id};`).join("");
                const chars: string = this.characters.map(iter => `+${iter.id};`).join("");

                const gen: string = `${start},${end};${outfits}${chars}`;
                this.generator = gen;
                console.log(gen);
            },

            onSendError: function(err: string): void {
                this.log("ERROR: " + err);
            },

            onSendReport: function(report: Report): void {
                this.report.ID = report.ID;
                // No idea why, but these dates don't include the Z, while the timestamp does
                this.report.periodEnd = new Date(report.periodEnd + "Z");
                this.report.periodStart = new Date(report.periodStart + "Z");
                this.report.timestamp = new Date(report.timestamp);
                this.report.teamID = report.teamID;

                this.periodStart = new Date(this.report.periodStart);
                this.periodStartInput = DateUtil.getLocalDateString(this.periodStart);
                this.periodEnd = new Date(this.report.periodEnd);
                this.periodEndInput = DateUtil.getLocalDateString(this.periodEnd);

                console.log(this.report);
                this.log(`Got report: ${JSON.stringify(this.report)}`);
                this.steps.report = true;
            },

            onUpdateCharacterIDs: function(ids: string[]): void {
                this.log(`Including data from ${ids.length} characters`);
                this.report.players = ids.filter((iter, index, arr) => arr.indexOf(iter) == index);
            },

            onUpdateKills: function(ev: KillEvent[]): void {
                this.report.kills = ev.map(iter => KillStatApi.parseKillEvent(iter));
                this.log(`Loaded ${this.report.kills.length} kill`);
                this.steps.kills = true;
            },

            onUpdateDeaths: function(ev: KillEvent[]): void {
                this.report.deaths = ev.map(iter => KillStatApi.parseKillEvent(iter));
                this.log(`Loaded ${this.report.deaths.length} deaths`);
                this.steps.deaths = true;
            },

            onUpdateExp: function(ev: ExpEvent[]): void {
                this.report.experience = ev.map(iter => ExpStatApi.parseExpEvent(iter));
                this.log(`Loaded ${this.report.experience.length} experience events`);
                this.steps.exp = true;
            },

            onUpdateItems: function(items: PsItem[]): void {
                for (const item of items) {
                    this.report.items.set(item.id, item);
                }
                this.log(`Loaded ${this.report.items.size} items`);
                this.steps.items = true;
            },

            onUpdateCharacters: function(chars: PsCharacter[]): void {
                for (const c of chars) {
                    this.report.characters.set(c.id, c);
                }
                this.log(`Loaded ${this.report.characters.size} characters`);
                this.steps.chars = true;
            },

            onUpdateOutfits: function(outfits: PsOutfit[]): void {
                for (const outfit of outfits) {
                    this.report.outfits.set(outfit.id, outfit);
                }
                this.log(`Loaded ${this.report.outfits.size} outfits`);
                this.steps.outfits = true;
            },

            onUpdateSessions: function(sessions: Session[]): void {
                this.report.sessions = sessions;
                this.log(`Loaded ${this.report.sessions.length} sessions`);
                this.steps.sessions = true;
            }
        },

        watch: {
            periodStartInput: function(): void {
                this.periodStart = new Date(this.periodStartInput);
            },

            periodEndInput: function(): void {
                this.periodEnd = new Date(this.periodEndInput);
            }
        },

        components: {
            DateTimePicker,
            InfoHover,
            ReportClassBreakdown,
            ReportPlayerList,
            ReportOutfitVersus,
            ReportWeaponBreakdown,
            ReportSupportBreakdown,
            ReportWinter
        }

    });
    export default OutfitReport;
</script>