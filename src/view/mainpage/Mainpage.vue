<template>
    <div>
        <div class="d-flex align-items-center">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>
            </honu-menu>

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

        <hr class="border" />

        <h1 class="d-inline-block mr-2">Realtime activity</h1>

        <div class="mb-4">
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

        <hr class="border" />

        <div>
            <h1 class="d-inline-block mr-2">
                <a href="/character">
                    Character Viewer
                </a>
            </h1>
        </div>

        <hr class="border" />

        <div>
            <h1 class="d-inline-block mr-2">
                Outfits
            </h1>

            <h2 class="d-inline-block mr-2">
                &bull;

                <a href="/outfitfinder">
                    Search
                </a>

                -

                <a href="/report">
                    Report
                </a>

                -

                <a href="/outfitpop">
                    Pop
                </a>
            </h2>
        </div>

        <hr class="border" />

        <div>
            <h1 class="d-inline-block mr-2">
                <a href="/alerts">
                    Alerts
                </a>
            </h1>
            <h3 class="d-inline-block">
                <small class="text-muted">View current and past alerts</small>
            </h3>
        </div>

        <hr class="border" />

        <div>
            <h1 class="d-inline-block mr-2">
                <a href="/ledger">
                    Ledger
                </a>
            </h1>
            <h3 class="d-inline-block">
                <small class="text-muted">Base capture/defend stats</small>
            </h3>
        </div>

        <hr class="border" />

        <div>
            <h1 class="d-inline-block mr-2">
                <a href="/realtimemap">
                    Realtime Map
                </a>
            </h1>
            <h3 class="d-inline-block">
                <small class="text-muted">Who owns what facility?</small>
            </h3>
        </div>

        <hr class="border" />

        <div>
            <h1 class="d-inline-block mr-2">
                <a href="/population">
                    Population
                </a>
            </h1>
            <h3 class="d-inline-block">
                <small class="text-muted">Historical population lookup</small>
            </h3>
        </div>

        <hr class="border" />

        <div class="mb-4">
            <h4>Contact</h4>

            <div>
                Discord: varunda
            </div>

            <div>
                GitHub: <a href="https://github.com/varunda/honu">https://github.com/varunda/honu</a>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";

    import { ZoneState, ZoneStateApi } from "api/ZoneStateApi";
    import { WorldOverview as WorldOverviewData } from "api/WorldOverviewApi";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";

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
                this.lastUpdate = new Date();

                this.worlds.clear();
                for (const datum of data) {
                    const overview: WorldOverviewData = this.parse(datum);
                    this.worlds.set(datum.worldID, overview);
                }

                //console.log(Array.from(this.worlds.values()));

                this.cobalt = this.worlds.get(13) || null;
                this.connery = this.worlds.get(1) || null;
                this.emerald = this.worlds.get(17) || null;
                this.jaeger = this.worlds.get(19) || null;
                this.miller = this.worlds.get(10) || null;
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

            document.title = "Honu / Homepage";
        },

        data: function() {
            return {
                socketState: "unconnected" as string,
                connection: null as sR.HubConnection | null,
                lastUpdate: null as Date | null,
                worlds: new Map() as Map<number, WorldOverviewData>,
                cobalt: null as any | null,
                connery: null as any | null,
                emerald: null as any | null,
                jaeger: null as any | null,
                miller: null as any | null,
                soltech: null as any | null
            }
        },

        methods: {

            parse(elem: any): WorldOverviewData {
                return {
                    worldID: elem.worldID,
                    worldName: elem.worldName,
                    playersOnline: elem.playersOnline,
                    zones: elem.zones.map((iter: any) => ZoneStateApi.parse(iter))
                };
            }

        },

        components: {
            WorldOverview,
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });

    export default Mainpage;
</script>