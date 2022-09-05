<template>
    <div style="height: 1080px; width: 1920px; max-width: 1920px">
        <div v-if="view == 'list'">
            Active alerts:

            <div class="list-group">
                <div v-for="alert in list.alerts" class="list-group-item d-flex">
                    <span class="flex-grow-1">
                        {{alert.timestamp | moment}} - {{alert.worldID | world}} instance {{alert.zoneID >> 16 & 0x0000FFFF}} ({{alert.worldID}}.{{alert.zoneID}})
                    </span>

                    <button class="btn btn-primary" @click="subscribe(alert.worldID, alert.zoneID)">
                        View
                    </button>
                </div>
            </div>
        </div>

        <div v-else-if="view == 'alert'">
            <!--
            <img v-if="alert.showExample" src="/img/ow_example.png" width="1920" height="1080" style="position: fixed; z-index: -10;" />
            -->

            <img src="/img/overlaybackgroundwicons.png" style="position: fixed; z-index: -5; left: 50%; transform: translateX(-50%);" width="695" height="112" />

            <div class="ps2-text position-fixed" style="font-size: 24pt; left: 50%; transform: translateX(-50%)">
                {{alert.worldID | world}}
            </div>

            <div v-if="alert.data != null && alert.data.tr != null" style="font-size: 14pt; text-align: right; position: fixed; top: 62px; right: 1212px; font-family: ps2; line-height: 1;">
                <realtime-alert-team-view :team="alert.data.tr"></realtime-alert-team-view>
            </div>

            <team-icon v-if="alert.outfitTR != null" class="position-fixed" style="top: 93px; left: 862px;"
                :team-id="3" :outfit="alert.outfitTR">
            </team-icon>

            <div v-if="alert.outfitTR != null" class="ps2-text position-fixed text-right" style="font-size: 16pt; right: 1060px; top: 93px;">
                [{{alert.outfitTR.tag}}]
            </div>

            <div v-if="alert.outfitNC != null" class="ps2-text position-fixed text-left" style="font-size: 16pt; left: 1060px; top: 93px;">
                [{{alert.outfitNC.tag}}]
            </div>

            <team-icon v-if="alert.outfitNC != null" class="position-fixed" style="top: 93px; right: 862px;"
                :team-id="2" :outfit="alert.outfitNC">
            </team-icon>

            <div v-if="alert.data != null && alert.data.nc != null" style="font-size: 14pt; text-align: left; position: fixed; top: 62px; left: 1212px; font-family: ps2; line-height: 1;">
                <realtime-alert-team-view :team="alert.data.nc"></realtime-alert-team-view>
            </div>
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
    import TeamIcon from "./components/TeamIcon.vue";

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

                intervalID: 0 as number,

                alert: {
                    worldID: null as number | null,
                    zoneID: null as number | null,

                    showExample: true as boolean,

                    data: null as RealtimeAlert | null,
                    full: null as RealtimeAlert | null,
                    outfitNC: null as PsOutfit | null,
                    outfitTR: null as PsOutfit | null
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

                this.intervalID = setInterval(() => {
                    if (this.alert.outfitNC == null || this.alert.outfitTR == null) {
                        if (this.connection != null) {
                            console.log(`an outfit is null, requesting full`);
                            this.connection.send("GetFull", this.alert.worldID, this.alert.zoneID);
                        }
                    }
                }, 1000 * 10) as unknown as number;
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

            /**
             * Tell the hub what alert we're subscribing to
             * @param worldID ID of the world the alert is happening on
             * @param zoneID ID of the zone the alert is happening on
             */
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

            /**
             * Callback when the hub sends all the alerts to us
             * @param data
             */
            onSendAll: function(data: any[]): void {
                const alerts: RealtimeAlert[] = data.map(iter => RealtimeAlertApi.parse(iter));
                this.list.alerts = alerts;
                console.log(alerts);
            },

            /**
             * Get the NC outfit
             */
            getOutfitNC: async function(): Promise<void> {
                if (this.alert.full != null) {
                    this.alert.outfitNC = await this.getOutfit(this.alert.full.nc, 2);
                }
            },

            /**
             * Get the TR outfit
             */
            getOutfitTR: async function(): Promise<void> {
                if (this.alert.full != null) {
                    this.alert.outfitTR = await this.getOutfit(this.alert.full.tr, 3);
                }
            },

            /**
             * Get an outfit based on the events that team has gotten
             * @param team
             * @param teamID
             */
            getOutfit: async function(team: RealtimeAlertTeam, teamID: number): Promise<PsOutfit | null> {
                let charID: string | null = null;

                for (const ev of team.killDeathEvents) {
                    if (ev.attackerTeamID == teamID) {
                        charID = ev.attackerCharacterID;
                        break;
                    }
                    if (ev.killedTeamID == teamID) {
                        charID = ev.killedCharacterID;
                        break;
                    }
                }

                if (charID == null) {
                    for (const ev of team.expEvents) {
                        if (ev.teamID == teamID) {
                            charID = ev.sourceID;
                            break;
                        }
                    }
                }

                if (charID == null) {
                    console.log(`cannot get outfit for team ${teamID}, charID is null`);
                    return null;
                }

                console.log(`loading outfit of ${charID} for team ${teamID}`);
                const char: Loading<PsCharacter> = await CharacterApi.getByID(charID);
                if (char.state != "loaded" || char.data.outfitID == null || char.data.outfitID == "") {
                    return null;
                }

                const outfit: Loading<PsOutfit> = await OutfitApi.getByID(char.data.outfitID);
                if (outfit.state == "loaded") {
                    return outfit.data;
                }

                return null;
            },

            /**
             * Callback when the hub sends all the data about an alert
             * @param data
             */
            onFullSend: function(data: any): void {
                const alert: RealtimeAlert = RealtimeAlertApi.parse(data);
                this.alert.full = alert;

                if (this.alert.outfitNC == null) {
                    this.getOutfitNC();
                }

                if (this.alert.outfitTR == null) {
                    this.getOutfitTR();
                }
            },

            /**
             * Callback when the hub sends the small update for an alert
             * @param data
             */
            onUpdateAlert: function(data: any): void {
                const alert: RealtimeAlert = RealtimeAlertApi.parse(data);
                this.alert.data = alert;
            }

        },

        components: {
            RealtimeAlertTeamView,
            TeamIcon
        }

    });
    export default RealtimeAlertView;
</script>