<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="#">Report</a>
            </h1>
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
            </div>

            <div class="input-group">
                <span class="input-group-prepend input-group-text">
                    End time
                </span>
                <input v-model="periodEndInput" type="datetime-local" class="form-control" />
            </div>

            <div class="input-group">
                <span class="input-group-prepend input-group-text">
                    Tag
                </span>

                <input v-model="search.outfitTag" type="text" class="form-control" @keyup.enter="searchTag" />
            </div>

            <input v-model="generator" type="text" class="form-control" @keyup.enter="start" />

            <button @click="updateGenerator" type="button" class="btn btn-secondary">
                Update
            </button>

            <button @click="start" type="button" class="btn btn-primary" :disabled="connected == true">
                Generate
            </button>
        </div>

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

        <div v-if="isDone == true">
            <report-class-breakdown :report="report"></report-class-breakdown>

            <report-player-list :report="report"></report-player-list>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";

    import { KillEvent, KillStatApi } from "api/KillStatApi";
    import { ExpEvent, ExpStatApi } from "api/ExpStatApi";
    import { PsItem } from "api/ItemApi";
    import { OutfitApi, PsOutfit } from "api/OutfitApi";
    import { PsCharacter } from "api/CharacterApi";
    import { Session } from "api/SessionApi";

    import Report, { PlayerMetadata, PlayerMetadataGenerator } from "./Report";

    import "MomentFilter";

    import ReportClassBreakdown from "./components/ReportClassBreakdown.vue";
    import ReportPlayerList from "./components/ReportPlayerList.vue";

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
                showLogs: true as boolean,

                isNew: true as boolean,
                isDone: false as boolean,
                isMaking: false as boolean,

                periodStartInput: "" as string,
                periodStart: new Date() as Date,
                periodEndInput: "" as string,
                periodEnd: new Date() as Date,

                search: {
                    outfitTag: "" as string
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

                outfits: [] as string[],

                generator: "" as string,
                genB64: "" as string
            }
        },

        created: function(): void {
            this.createConnection();

            this.parseUrl();
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

            parseUrl: function(): void {
                const parts: string[] = location.pathname.split("/").filter(iter => iter != "");
                console.log(parts);

                if (parts.length > 1) {
                    const gen: string = parts[1];
                    console.log(`Loaded generator '${gen}' from URL`);

                    this.generator = atob(gen);
                }
            },

            searchTag: async function(): Promise<void> {
                const outfits: PsOutfit[] = await OutfitApi.getByTag(this.search.outfitTag);

                if (outfits.length == 1) {
                    this.outfits.push(outfits[0].id);
                    this.updateGenerator();
                    this.search.outfitTag = "";
                }
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
                    }, 3000);
                }).catch((err: any) => {
                    console.error(err);
                });
            },

            updateGenerator: function(): void {
                console.log(`Start: ${this.periodStart}`);
                console.log(`End: ${this.periodEnd}`);

                console.log(`Outfits: [${this.outfits.join(", ")}]`);

                this.periodStart = new Date(this.periodStartInput);
                this.periodEnd = new Date(this.periodEndInput);

                const start: number = Math.floor(this.periodStart.getTime() / 1000);
                const end: number = Math.floor(this.periodEnd.getTime() / 1000);

                const gen: string = `${start},${end};${this.outfits.map(iter => `o${iter};`)}`;
                this.generator = gen;
                console.log(gen);
            },

            onSendReport: function(report: Report): void {
                this.report.ID = report.ID;
                this.report.periodEnd = report.periodEnd;
                this.report.periodStart = report.periodStart;
                this.report.timestamp = report.timestamp;
                this.report.teamID = report.teamID;

                this.periodStart = this.report.periodStart;
                this.periodStartInput = this.periodStart.toString();
                this.periodEnd = this.report.periodEnd;
                this.periodEndInput = this.periodEnd.toString();

                this.log(`Got report: ${JSON.stringify(report)}`);
                this.steps.report = true;
            },

            onUpdateCharacterIDs: function(ids: string[]): void {
                this.log(`Including data from ${ids.length} characters`);
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

        components: {
            DateTimePicker,
            InfoHover,
            ReportClassBreakdown,
            ReportPlayerList,
        }

    });
    export default OutfitReport;
</script>