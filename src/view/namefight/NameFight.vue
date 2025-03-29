
<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/namefight">Final Fight tracker</a>
            </li>
        </honu-menu>

        <div class="container">
            <h1>
                Final Fight tracker
            </h1>

            <p class="mb-2">
                This page is to track the <a href="https://forums.daybreakgames.com/ps2/index.php?threads/the-grand-server-merge-event-forum-discussion.263925/">Final Fight event</a>,
                where the faction with the <a href="https://forums.daybreakgames.com/ps2/index.php?threads/the-grand-server-merge-event-forum-discussion.263925/page-2#post-3606239">most alert wins</a> per region decides the new name of the server
            </p>

            <p class="mb-2">
                Alert wins between <b>{{ startTime | moment }}</b> and <b>{{ endTime | moment }}</b> count towards the new name (these times are in your timezone)
            </p>

            <div class="mb-4">
                This page refreshes every minute. Last updated
                <span v-if="lastUpdated == null">
                    never!
                </span>
                <span v-else>
                    {{ lastUpdated | moment }}
                </span>

                <button title="refresh" class="btn btn-link p-0" @click="bind" :class=" {'spin': entries.state != 'loaded'}">
                    <span>&#x21bb;</span>
                </button>

            </div>

            <div class="mb-5 mt-4">
                <h1 class="wt-header">NA server (Connery and Emerald)</h1>

                <h2 class="mb-0">
                    <img src="/img/logo_vs.png" height="48" width="48">
                    VS - Helios
                </h2>

                <div class="progress mb-4" style="height: 3rem">
                    <div class="progress-bar bg-vs" :style="{ 'width': naWinsVs / Math.max(1, naWinner) * 100 + '%' }">
                        <h3 class="mb-0">{{ naWinsVs }}</h3>
                    </div>
                </div>

                <h2 class="mb-0">
                    <img src="/img/logo_nc.png" height="48" width="48">
                    NC - Osprey
                </h2>

                <div class="progress mb-4" style="height: 3rem">
                    <div class="progress-bar bg-nc" :style="{ 'width': naWinsNc / Math.max(1, naWinner) * 100 + '%' }">
                        <h3 class="mb-0">{{ naWinsNc }}</h3>
                    </div>
                </div>

                <h2 class="mb-0">
                    <img src="/img/logo_tr.png" height="48" width="48">
                    TR - LithCorp
                </h2>

                <div class="progress mb-4" style="height: 3rem">
                    <div class="progress-bar bg-tr" :style="{ 'width': naWinsTr / Math.max(1, naWinner) * 100 + '%' }">
                        <h3 class="mb-0">{{ naWinsTr }}</h3>
                    </div>
                </div>
            </div>

            <div class="mb-5">
                <h1 class="wt-header">EU server (Miller and Cobalt)</h1>

                <h2 class="mb-0">
                    <img src="/img/logo_vs.png" height="48" width="48">
                    VS - Erebus
                </h2>
                <div class="progress mb-4" style="height: 3rem">
                    <div class="progress-bar bg-vs" :style="{ 'width': euWinsVs / Math.max(1, euWinner) * 100 + '%' }">
                        <h3 class="mb-0">{{ euWinsVs }}</h3>
                    </div>
                </div>

                <h2 class="mb-0">
                    <img src="/img/logo_nc.png" height="48" width="48">
                    NC - Excavion
                </h2>
                <div class="progress mb-4" style="height: 3rem">
                    <div class="progress-bar bg-nc" :style="{ 'width': euWinsNc / Math.max(1, euWinner) * 100 + '%' }">
                        <h3 class="mb-0">{{ euWinsNc }}</h3>
                    </div>
                </div>

                <h2 class="mb-0">
                    <img src="/img/logo_tr.png" height="48" width="48">
                    TR - Wainwright
                </h2>
                <div class="progress mb-4" style="height: 3rem">
                    <div class="progress-bar bg-tr" :style="{ 'width': euWinsTr / Math.max(1, euWinner) * 100 + '%' }">
                        <h3 class="mb-0">{{ euWinsTr }}</h3>
                    </div>
                </div>
            </div>

            <p class="mb-2">
                This tracker is not officially endorsed, and is not a source of truth. This is a community made tracker that may be incorrect!
            </p>
        </div>
    </div>

</template>

<style scoped>

    .bg-vs {
        background-color: var(--color-bg-1);
    }
    .bg-nc {
        background-color: var(--color-bg-2);
    }
    .bg-tr {
        background-color: var(--color-bg-3);
    }

</style>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import { NameFightApi, NameFightEntry } from "api/NameFightApi";

    import "MomentFilter";

    import WorldUtils from "util/World";

    export const NameFight = Vue.extend({
        props: {

        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<NameFightEntry[]>,

                startTime: new Date(Date.parse("2025-03-28T07:00Z")) as Date,
                endTime: new Date(Date.parse("2025-03-31T06:59Z")) as Date
            }
        },

        mounted: function(): void {
            this.bind();
            setInterval(async () => {
                await this.bind();
            }, 1000 * 60);
        },

        methods: {
            bind: async function(): Promise<void> {
                this.entries = Loadable.loading();
                this.entries = await NameFightApi.getEntry();
            }
        },

        computed: {

            whenEvent: function(): string {
                const now: Date = new Date();

                if (this.startTime.getTime() > now.getTime()) {
                    return "future";
                }

                if (this.startTime.getTime() < now.getTime() && this.endTime.getTime() > now.getTime()) {
                    return "now";
                }
                
                return "past";
            },

            lastUpdated: function(): Date | null {
                if (this.entries.state != "loaded") {
                    return null;
                }

                if (this.entries.data.length == 0) {
                    return null;
                }

                return this.entries.data[0].timestamp;
            },

            naAlerts: function(): NameFightEntry[] {
                if (this.entries.state != "loaded") {
                    return [];
                }

                return this.entries.data.filter(iter => iter.worldID == WorldUtils.Connery || iter.worldID == WorldUtils.Emerald);
            },

            naAlertTotal: function(): number {
                return this.naAlerts.reduce((acc, iter) => acc += iter.total, 0);
            },

            naWinner: function(): number {
                return Math.max(this.naWinsVs, this.naWinsNc, this.naWinsTr);
            },

            naWinsVs: function(): number {
                return this.naAlerts.reduce((acc, iter) => acc += iter.winsVs, 0);
            },
            naWinsNc: function(): number {
                return this.naAlerts.reduce((acc, iter) => acc += iter.winsNc, 0);
            },
            naWinsTr: function(): number {
                return this.naAlerts.reduce((acc, iter) => acc += iter.winsTr, 0);
            },

            euAlerts: function(): NameFightEntry[] {
                if (this.entries.state != "loaded") {
                    return [];
                }

                return this.entries.data.filter(iter => iter.worldID == WorldUtils.Miller || iter.worldID == WorldUtils.Cobalt);
            },

            euAlertTotal: function(): number {
                return this.euAlerts.reduce((acc, iter) => acc += iter.total, 0);
            },

            euWinsVs: function(): number {
                return this.euAlerts.reduce((acc, iter) => acc += iter.winsVs, 0);
            },
            euWinsNc: function(): number {
                return this.euAlerts.reduce((acc, iter) => acc += iter.winsNc, 0);
            },
            euWinsTr: function(): number {
                return this.euAlerts.reduce((acc, iter) => acc += iter.winsTr, 0);
            },
            euWinner: function(): number {
                return Math.max(this.euWinsVs, this.euWinsNc, this.euWinsTr);
            },

        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
        }

    });
    export default NameFight;

</script>