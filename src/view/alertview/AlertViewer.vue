<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/alerts">Alerts</a>
            </li>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Alert
            </li>
        </honu-menu>

        <div v-if="participants.state == 'idle'"></div>

        <div v-else-if="participants.state == 'loading'">
            <busy class="honu-busy"></busy>
        </div>

        <div v-else-if="participants.state == 'loaded'">
            <div class="row">
                <div class="col-6">
                    <h2 class="wt-header">Kills</h2>
                    <alert-kill-board :participants="participants"></alert-kill-board>
                </div>

                <div class="col-6">
                    <h2 class="wt-header">Medic</h2>
                    <alert-medic-board :participants="participants"></alert-medic-board>
                </div>

                <div class="col-6">
                    <h2 class="wt-header">Engineer</h2>
                    <alert-engineer-board :participants="participants"></alert-engineer-board>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { AlertParticipantApi, FlattendParticipantDataEntry } from "api/AlertParticipantApi";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Busy from "components/Busy.vue";

    import AlertKillBoard from "./components/AlertKillBoard.vue";
    import AlertMedicBoard from "./components/AlertMedicBoard.vue";
    import AlertEngineerBoard from "./components/AlertEngineerBoard.vue";

    export const AlertViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                alertID: 1 as number,

                participants: Loadable.idle() as Loading<FlattendParticipantDataEntry[]> 
            }
        },

        mounted: function(): void {
            this.loadAlertData();
        },

        methods: {
            parseAlertIDFromUrl: function(): void {
                this.alertID = 1;
            },

            loadAlertData: async function(): Promise<void> {
                this.participants = Loadable.loading();
                this.participants = await AlertParticipantApi.getByAlertID(this.alertID);
            },

            getFactionColor: function(factionID: number): string {
                return ColorUtils.getFactionColor(factionID) + " !important";
            }

        },

        computed: {

            sources: function() {
                return {
                    factions: [
                        { key: null, value: "All" },
                        { key: 1, value: "VS" },
                        { key: 2, value: "NC" },
                        { key: 3, value: "TR" },
                        { key: 4, value: "NS" },
                    ]
                }
            }

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Busy,
            AlertKillBoard, AlertMedicBoard, AlertEngineerBoard
        }
    });
    export default AlertViewer;
</script>