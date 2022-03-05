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
                <span v-if="alert.state == 'loaded'">
                    {{alert.data.worldID}}-{{alert.data.instanceID}}
                </span>
            </li>
        </honu-menu>

        <div>
            <h2 class="wt-header">
                Alert
            </h2>
            
            <div v-if="alert.state == 'idle'"></div>

            <div v-else-if="alert.state == 'loading'">
                <busy class="honu-busy"></busy>
            </div>

            <div v-else-if="alert.state == 'loaded'">
                <alert-general :alert="alert.data"></alert-general>

                <h4 v-if="error.notFinished == true" class="text-warning text-center">
                    Alert has not finished, stats have not been generated yet
                    <br />
                    Come back after {{alert.data.end | moment}}
                </h4>
            </div>

            <div v-else-if="alert.state == 'error'" class="text-danger">
                Error loading alert: {{alert.message}}
            </div>

            <div v-else class="text-danger">
                Unchecked state of alert: {{alert.state}}
            </div>
        </div>

        <div v-if="participants.state == 'idle'"></div>

        <div v-else-if="participants.state == 'loading'">
            <h2 class="wt-header">Stats</h2>
            <busy class="honu-busy"></busy>
        </div>

        <div v-else-if="participants.state == 'loaded'">
            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Kills</h2>
                </div>

                <div class="col-6">
                    <alert-kill-board :participants="participants"></alert-kill-board>
                </div>

                <div class="col-6">
                    <alert-outfit-kill-board :outfits="outfits" :alert="alert.data"></alert-outfit-kill-board>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Medic</h2>
                </div>

                <div class="col-6">
                    <alert-medic-board :participants="participants"></alert-medic-board>
                </div>

                <div class="col-6">
                    <alert-outfit-medic-board :outfits="outfits"></alert-outfit-medic-board>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Engineer</h2>
                </div>

                <div class="col-6">
                    <alert-engineer-board :participants="participants"></alert-engineer-board>
                </div>
            </div>
        </div>

        <div v-else-if="participants.state == 'error'" class="text-danger">
            Error loading stats: {{participants.message}}
        </div>

        <div v-else class="text-danger">
            Unchecked state of participants: {{participants.state}}
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { AlertParticipantApi, FlattendParticipantDataEntry } from "api/AlertParticipantApi";
    import { PsAlert, AlertApi } from "api/AlertApi";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Busy from "components/Busy.vue";

    import AlertGeneral from "./components/AlertGeneral.vue";
    import AlertKillBoard from "./components/AlertKillBoard.vue";
    import AlertOutfitKillBoard from "./components/AlertOutfitKillBoard.vue";
    import AlertMedicBoard from "./components/AlertMedicBoard.vue";
    import AlertOutfitMedicBoard from "./components/AlertOutfitMedicBoard.vue";
    import AlertEngineerBoard from "./components/AlertEngineerBoard.vue";

    class OutfitDataEntry {
        public outfitID: string = "";
        public outfitTag: string | null = null;
        public outfitName: string = "";
        public outfitDisplay: string = "";
        public factionID: number = 0;

        public secondsOnline: number = 0;
        public members: number = 0;

        public kills: number = 0;
        public deaths: number = 0;
        public vehicleKills: number = 0;
        public spawns: number = 0;

        public kpm: number = 0;
        public kd: number = 0;

        public medicPlaytime: number = 0;
        public heals: number = 0;
        public revives: number = 0;
        public shieldRepairs: number = 0;

        public engineerPlaytime: number = 0;
        public resupplies: number = 0;
        public repairs: number = 0;
    }

    export const AlertViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                alertID: 1 as number,

                outfitMap: new Map() as Map<string, OutfitDataEntry>,
                outfits: Loadable.idle() as Loading<OutfitDataEntry[]>,

                error: {
                    notFinished: false as boolean
                },

                alert: Loadable.idle() as Loading<PsAlert>,
                participants: Loadable.idle() as Loading<FlattendParticipantDataEntry[]> 
            }
        },

        created: function(): void {
            document.title = `Honu / Alert / <loading...>`;
        },

        mounted: function(): void {
            this.parseAlertIDFromUrl();
            if (this.alertID > 0) {
                this.loadAlert();
            }
        },

        methods: {
            parseAlertIDFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/").filter(iter => iter.length > 0);

                const alertID: number = Number.parseInt(parts[1]);
                if (Number.isNaN(alertID)) {
                    console.error(`failed to parse ${parts[1]} into a valid interger`);
                } else {
                    this.alertID = alertID;
                }
            },

            loadAlert: async function(): Promise<void> {
                this.alert = Loadable.loading();
                this.alert = await AlertApi.getByID(this.alertID);

                if (this.alert.state == "loaded") {
                    document.title = `Honu / Alert / ${this.alert.data.worldID}-${this.alert.data.instanceID}`;

                    if (new Date() < this.alert.data.end) {
                        this.error.notFinished = true;
                    } else {
                        this.loadAlertData();
                    }
                }
            },

            loadAlertData: async function(): Promise<void> {
                this.participants = Loadable.loading();
                this.participants = await AlertParticipantApi.getByAlertID(this.alertID);

                if (this.participants.state == "loaded") {
                    this.makeOutfitData();
                }
            },

            getFactionColor: function(factionID: number): string {
                return ColorUtils.getFactionColor(factionID) + " !important";
            },

            makeOutfitData: function(): void {
                this.outfitMap.clear();
                this.outfits = Loadable.idle();

                if (this.participants.state != "loaded") {
                    console.warn(`cannot make outfit data, participants.state is not loaded, currently ${this.participants.state}`);
                    return;
                }

                this.outfits = Loadable.loading();

                for (const entry of this.participants.data) {
                    if (entry.outfitID == null) {
                        continue;
                    }

                    if (this.outfitMap.has(entry.outfitID) == false) {
                        const outfitEntry: OutfitDataEntry = new OutfitDataEntry();
                        outfitEntry.outfitID = entry.outfitID;
                        outfitEntry.outfitTag = entry.outfitTag;
                        outfitEntry.outfitName = entry.outfitName ?? `<missing ${entry.outfitID}>`;
                        outfitEntry.outfitDisplay = `[${entry.outfitTag}] ${entry.outfitName}`;
                        outfitEntry.factionID = entry.factionID;

                        this.outfitMap.set(entry.outfitID, outfitEntry);
                    }

                    const outfitEntry: OutfitDataEntry = this.outfitMap.get(entry.outfitID)!;

                    outfitEntry.kills += entry.kills;
                    outfitEntry.deaths += entry.deaths;
                    outfitEntry.spawns += entry.spawns;
                    outfitEntry.vehicleKills += entry.vehicleKills;
                    outfitEntry.secondsOnline += entry.secondsOnline;

                    if (entry.heals > 10 || entry.revives > 10 || entry.shieldRepairs > 10) {
                        outfitEntry.heals += entry.heals;
                        outfitEntry.revives += entry.revives;
                        outfitEntry.shieldRepairs += entry.shieldRepairs;
                        outfitEntry.medicPlaytime += entry.secondsOnline;
                    }

                    if (entry.repairs > 10 || entry.resupplies > 10) {
                        outfitEntry.repairs += entry.repairs;
                        outfitEntry.resupplies += entry.resupplies;
                        outfitEntry.engineerPlaytime += entry.secondsOnline;
                    }

                    ++outfitEntry.members;

                    if (outfitEntry.factionID == 4) {
                        outfitEntry.factionID = entry.factionID;
                    }
                }

                const outfits: OutfitDataEntry[] = Array.from(this.outfitMap.values()).filter(iter => iter.members >= 5);
                for (const outfit of outfits) {
                    outfit.kd = outfit.kills / Math.max(1, outfit.deaths);
                    outfit.kpm = outfit.kills / Math.max(1, outfit.secondsOnline) * 60;
                }

                this.outfits = Loadable.loaded(outfits);
            },

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
            AlertGeneral,
            AlertKillBoard, AlertMedicBoard, AlertEngineerBoard, AlertOutfitKillBoard, AlertOutfitMedicBoard
        }
    });
    export default AlertViewer;
</script>