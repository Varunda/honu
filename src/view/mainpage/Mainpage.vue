﻿<template>
    <div class="container-fluid">
        <div class="d-flex align-items-center">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">
                    Honu
                </a>
            </h1>

            <div>
                <table class="table table-sm">
                    <tr>
                        <td>Socket status:</td>
                        <td>{{socketState}}</td>
                    </tr>

                    <tr>
                        <td>
                            Last socket update:
                            <info-hover text="When the last data was received from the server"></info-hover>
                        </td>
                        <td>
                            {{lastUpdate | moment("YYYY-MM-DD hh:mm:ssA ZZ")}}
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div>
            <h4>Select server</h4>

            <div class="mainpage-grid">
                <world-overview name="cobalt" display-name="Cobalt" :data="cobalt"></world-overview>
                <div class="mp-grid-line"></div>
                <world-overview name="connery" display-name="Connery" :data="connery"></world-overview>
                <div class="mp-grid-line"></div>
                <world-overview name="emerald" display-name="Emerald" :data="emerald"></world-overview>
                <div class="mp-grid-line"></div>
                <world-overview name="jaeger" display-name="Jaeger" :data="jaeger"></world-overview>
                <div class="mp-grid-line"></div>
                <world-overview name="miller" display-name="Miller" :data="miller"></world-overview>
                <div class="mp-grid-line"></div>
                <world-overview name="soltech" display-name="SolTech" :data="soltech"></world-overview>
            </div>
        </div>

        <hr />

        <div>
            <h4>Ledger</h4>

            <a href="/ledger">View</a>
        </div>

        <div>
            <h4>Contact</h4>
            
            <div>
                Discord: Hdt#1468
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import * as sR from "signalR";

    import WorldOverview from "./components/WorldOverview.vue";

    export const Mainpage = Vue.extend({
        props: {

        },

        created: function(): void {
            this.connection = new sR.HubConnectionBuilder()
                .withUrl("/ws/overview")
                .withAutomaticReconnect([5000, 10000, 20000, 20000])
                .build();

            this.connection.on("UpdateData", (data: any) => {
                console.log(data);
                this.lastUpdate = new Date();

                this.worlds.clear();

                for (const datum of data) {
                    this.worlds.set(datum.worldID, datum);
                }

                this.cobalt = this.worlds.get(10) || null;
                this.connery = this.worlds.get(1) || null;
                this.emerald = this.worlds.get(17) || null;
                this.jaeger = this.worlds.get(19) || null;
                this.miller = this.worlds.get(13) || null;
                this.soltech = this.worlds.get(40) || null;
            });

            this.connection.start().then(() => {
                this.socketState = "opened";
                console.log(`connected`);
            }).catch(err => {
                console.error(err);
            });

            this.connection.onreconnected(() => {
                console.log(`reconnected`);
                this.socketState = "opened";
            });

            this.connection.onclose((err?: Error) => {
                this.socketState = "closed";
                if (err) {
                    console.error("onclose: ", err);
                }
            });

            this.connection.onreconnecting((err?: Error) => {
                this.socketState = "reconnecting";
                if (err) {
                    console.error("onreconnecting: ", err);
                }
            });
        },

        data: function() {
            return {
                socketState: "unconnected" as string,
                connection: null as sR.HubConnection | null,
                lastUpdate: null as Date | null,

                worlds: new Map() as Map<number, any>,

                cobalt: null as any | null,
                connery: null as any | null,
                emerald: null as any | null,
                jaeger: null as any | null,
                miller: null as any | null,
                soltech: null as any | null
            }
        },

        components: {
            WorldOverview
        }
    });
    export default Mainpage;
</script>