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

        <div style="height: 300px; overflow-y: scroll;" class="container-fluid">
            <div v-for="msg in logs" class="row">
                <div class="col-2">
                    {{msg.when | moment}}
                </div>

                <div class="col-10">
                    {{msg.message}}
                </div>
            </div>
        </div>

        <div v-if="isNew == true">
            <input v-model="generator" type="text" class="form-control" @keyup.enter="start" />

            <button @click="start" type="button" class="btn btn-primary">
                Generate
            </button>

            <div>
                {{genB64}}
            </div>

        </div>
        
        <div v-else>

        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";

    import { KillEvent } from "api/KillStatApi";

    import "MomentFilter";

    type Message = {
        when: Date;
        message: string;
    };

    class Report {
        public ID: number = 0;
        public generator: string = "";
        public timestamp: Date = new Date();
        public periodStart: Date = new Date();
        public periodEnd: Date = new Date();
        public characterIDs: string[] = [];
        public killDeaths: KillEvent[] = [];
    }

    export const OutfitReport = Vue.extend({
        data: function() {
            return {
                logs: [] as Message[],

                isNew: true as boolean,

                connection: null as sR.HubConnection | null,
                report: new Report() as Report,

                generator: "" as string,
                genB64: "" as string
            }
        },

        created: function(): void {
            this.createConnection();

            const start: number = Math.floor(new Date().getTime() / 1000);
            const end: number = Math.floor(new Date().getTime() / 1000) - 20000;

            const t1deID: string = "37567362753122235";

            this.generator = `${start},${end};o${t1deID};`;
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

            createConnection: async function(): Promise<void> {
                document.title = `Honu / Outfit Report`;
                this.log(`Starting signalR connection`);

                if (this.connection != null) {
                    await this.connection.stop();
                    this.connection = null;
                }

                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/report")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .build();

                this.log(`Connecting...`);

                this.connection.on("SendReport", this.onSendReport);
                this.connection.on("UpdateCharacterIDs", this.onUpdateCharacterIDs);
                this.connection.on("UpdateKillDeaths", this.onUpdateKillDeaths);

                this.connection.start().then(() => {
                    this.log(`Connected! Waiting for generator string`);
                }).catch(err => {
                    console.error(err);
                });

                this.connection.onreconnected(() => {
                    console.log(`reconnected`);
                });

                this.connection.onclose((err?: Error) => {
                    if (err) {
                        console.error("onclose: ", err);
                    }
                });

                this.connection.onreconnecting((err?: Error) => {
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
                this.connection.invoke("GenerateReport", this.generator)
            },

            onSendReport: function(report: Report): void {
                this.report = report;
                this.log(`Got report: ${JSON.stringify(report)}`);
            },

            onUpdateCharacterIDs: function(report: Report): void {
                this.report = report;
                this.log(`Including data from ${report.characterIDs.length} characters`);
            },

            onUpdateKillDeaths: function(report: Report): void {
                this.report = report;
                this.log(`Loaded ${report.killDeaths.length} kill/deaths`);
            }

        },

        components: {

        }

    });
    export default OutfitReport;
</script>