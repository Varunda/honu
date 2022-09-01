<template>
    <div style="height: 1080px; width: 1920px; max-width: 1920px">
        <div v-if="view == 'list'">
            Active alerts:

            <div class="list-group">
                <div v-for="alert in list.alerts" class="list-group-item d-flex">
                    <span class="flex-grow-1">
                        {{alert.timestamp | moment}} - {{alert.worldID | world}} instance {{alert.zoneID >> 16 && 0x0000FFFF}} ({{alert.worldID}}.{{alert.zoneID}})
                    </span>

                    <button class="btn btn-primary" @click="subscribe(alert.worldID, alert.zoneID)">
                        View
                    </button>
                </div>
            </div>

        </div>

        <div v-else-if="view == 'alert'">
            <img v-if="alert.showExample" src="/img/ow_example.png" width="1920" height="1080" style="position: fixed; z-index: -10;" />

            <realtime-alert-team-view v-if="alert.data != null" :team="alert.data.nc" :name="alert.outfitNC" style="left: 490px; top: 5px; position: fixed;">
            </realtime-alert-team-view>

            <realtime-alert-team-view v-if="alert.data != null" :team="alert.data.tr" :name="alert.outfitTR" style="left: 1220px; top: 5px; position: fixed;">
            </realtime-alert-team-view>
        </div>

        <div v-else class="text-danger">
            Unchecked state of view: {{view}}
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading } from "Loading";
    import * as sR from "signalR";

    import { RealtimeAlert, RealtimeAlertTeam, RealtimeAlertApi } from "api/RealtimeAlertApi";
    import { PsOutfit, OutfitApi } from "api/OutfitApi";
    import { CharacterApi, PsCharacter } from "api/CharacterApi";

    import RealtimeAlertTeamView from "./components/RealtimeAlertTeam.vue";

    import "MomentFilter";
    import "filters/WorldNameFilter";

    export const RealtimeAlertView = Vue.extend({
        props: {

        },

        data: function() {
            return {
                connection: null as sR.HubConnection | null,

                view: "list" as "list" | "alert",

                list: {
                    alerts: [] as RealtimeAlert[]
                },

                alert: {
                    worldID: null as number | null,
                    zoneID: null as number | null,

                    showExample: true as boolean,

                    data: null as RealtimeAlert | null,
                    outfitNC: null as string | null,
                    outfitTR: null as string | null
                }
            }
        },

        mounted: function(): void {
            this.connect();
        },

        methods: {
            /**
             * Connect to the signalR Hub, stopping the previous connection if needed
             */
            connect: function(): void {
                if (this.connection != null) {
                    this.connection.stop();
                    this.connection = null;
                }

                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/realtime-alert")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .build();

                this.connection.on("SendAll", this.onSendAll);
                this.connection.on("UpdateAlert", this.onUpdateAlert);
                this.connection.on("SendFull", this.onFullSend);

                this.connection.onreconnected(() => {
                    if (this.alert.worldID != null && this.alert.zoneID != null) {
                        this.subscribe(this.alert.worldID, this.alert.zoneID);
                    }
                });

                this.connection.start().then(() => {
                    console.log(`socket> connected`);
                    this.parseUrl();
                }).catch(err => {
                    console.error(err);
                });
            },

            /**
             * Parse the URL for if we're gonna be listed the realtime alerts or listening to them
             */
            parseUrl: function(): void {
                if (this.connection == null) {
                    return console.warn(`cannot parseURL: connection is null`);
                }

                this.alert.worldID = null;
                this.alert.zoneID = null;

                const params: URLSearchParams = new URLSearchParams(location.search);
                console.log(params);

                if (params.has("worldID")) {
                    const worldID: number = Number.parseInt(params.get("worldID")!);
                    if (Number.isNaN(worldID) == true) {
                        console.warn(`Failed to parse ${params.get("worldID")} to a valid int`);
                    } else {
                        this.alert.worldID = worldID;
                        console.log(`setting`);
                    }
                }

                if (params.has("zoneID")) {
                    const zoneID: number = Number.parseInt(params.get("zoneID")!);
                    if (Number.isNaN(zoneID) == true) {
                        console.warn(`Failed to parse ${params.get("zoneID")} to a valid int`);
                    } else {
                        this.alert.zoneID = zoneID;
                    }
                }

                if (this.alert.worldID != null && this.alert.zoneID != null) {
                    this.view = "alert";
                    this.subscribe(this.alert.worldID, this.alert.zoneID);
                } else {
                    this.view = "list";
                    this.connection.send("GetList");
                }
            },

            subscribe: function(worldID: number, zoneID: number): void {
                this.alert.worldID = worldID;
                this.alert.zoneID = zoneID;

                this.view = "alert";
                const url = new URL(location as any); // it's safe
                url.searchParams.set("worldID", `${this.alert.worldID}`);
                url.searchParams.set("zoneID", `${this.alert.zoneID}`);
                window.history.pushState(null, "", url.toString());

                if (this.connection != null) {
                    this.connection.send("Subscribe", this.alert.worldID, this.alert.zoneID);
                }
            },

            onSendAll: function(data: any[]): void {
                const alerts: RealtimeAlert[] = data.map(iter => RealtimeAlertApi.parse(iter));
                this.list.alerts = alerts;
                console.log(alerts);
            },

            onFullSend: function(data: any): void {
                const alert: RealtimeAlert = RealtimeAlertApi.parse(data);
                this.alert.data = alert;

                if (this.alert.outfitNC == null) {
                    for (const ev of this.alert.data.nc.killDeathEvents) {
                        if (ev.attackerTeamID == 2) {
                            CharacterApi.getByID(ev.attackerCharacterID).then((char: Loading<PsCharacter>) => {
                                if (char.state == "loaded") {
                                    this.alert.outfitNC = char.data.outfitName ?? "";
                                }
                            });
                            break;
                        }
                    }
                }

                if (this.alert.outfitTR == null) {
                    for (const ev of this.alert.data.tr.killDeathEvents) {
                        if (ev.attackerTeamID == 3) {
                            CharacterApi.getByID(ev.attackerCharacterID).then((char: Loading<PsCharacter>) => {
                                if (char.state == "loaded") {
                                    this.alert.outfitTR = char.data.outfitName ?? "";
                                }
                            });
                            break;
                        }
                    }
                }
            },

            onUpdateAlert: function(data: any): void {
                const alert: RealtimeAlert = RealtimeAlertApi.parse(data);
                this.alert.data = alert;
            }

        },

        components: {
            RealtimeAlertTeamView
        }

    });
    export default RealtimeAlertView;
</script>