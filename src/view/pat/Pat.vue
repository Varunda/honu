<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/pat">Pat</a>
            </li>
        </honu-menu>

        <hr class="border" />

        <div class="container d-flex flex-column justify-content-center align-items-center">
            <div>
                <button type="button" class="btn btn-primary w-100 mb-2" @click="pat" :disabled="disableButton">
                    Pat
                </button>

                <br>

                <div v-if="loaded">
                    A certain cat <!-- silzz --> has been patted {{ count | locale(0) }} times
                </div>
                <div v-else>
                    <busy class="honu-busy"></busy>
                </div>

            </div>

            <img v-show="show.smol" width="52" src="/img/wtf_smol.png">

            <img v-show="show.wtf" src="/img/wtf.png" width="256">
        </div>


    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import * as sR from "signalR";
    import { Loadable, Loading } from "Loading";

    import "MomentFilter";
    import "filters/LocaleFilter";

    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Collapsible from "components/Collapsible.vue";
    import Busy from "components/Busy.vue";
    import ApiError from "components/ApiError";

    export const Pat = Vue.extend({
        props: {

        },

        data: function() {
            return {
                connection: null as sR.HubConnection | null,
                lastUpdate: new Date() as Date,

                loaded: false as boolean,
                count: 0 as number,

                show: {
                    wtf: false as boolean,
                    smol: false as boolean
                }
            }
        },

        created: function(): void {
            document.title = `Honu / Pat`;
        },

        beforeMount: function(): void {

        },

        mounted: function(): void {
            this.connect();

        },

        methods: {
            connect: function(): void {
                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/pat")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .build();

                this.connection.on("SendValue", (value: number) => {
                    this.lastUpdate = new Date();
                    this.count = value;
                });

                this.connection.start().then(() => {
                    console.log(`connected to signalR hub`);
                    this.connection?.invoke("GetValue").then((value: number) => {
                        console.log(`loaded pat value: ${value}`);
                        this.count = value;
                        this.loaded = true;
                    });
                }).catch(err => {
                    console.error(err);
                });

                this.connection.onreconnected(() => {
                    console.log(`reconnected to signalR`);
                    this.loaded = true;
                });

                this.connection.onclose((err?: Error) => {
                    this.loaded = false;
                    if (err) {
                        console.error("onclose: ", err);
                    } else {
                        console.error(`connection closed`);
                    }
                });

                this.connection.onreconnecting((err?: Error) => {
                    if (err) {
                        console.error("onreconnecting: ", err);
                    } else {
                        console.error(`reconnecting`);
                    }
                });
            },

            pat: function(): void {
                if (this.connection == null) {
                    console.warn(`cannot pat, connection is null`);
                    return;
                }

                if (this.loaded == false) {
                    console.log(`cannot pat: loaded is false`);
                    return;
                }

                this.connection.invoke("Press").then((value: number) => {
                    this.count = value;

                    if (this.count % 100 == 0 && this.count % 1000 != 0) {
                        this.show.smol = true;
                        setTimeout(() => { this.show.smol = false; }, 2000);
                    }

                    if (this.count % 1000 == 0) {
                        this.show.wtf = true;
                        setTimeout(() => { this.show.wtf = false; }, 5000);
                    }
                });
            }

        },

        computed: {
            disableButton: function(): boolean {
                return this.loaded == false;
            }
        },

        components: {
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Collapsible,
            Busy,
            ApiError
        }

    });
    export default Pat;
</script>