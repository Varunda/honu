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
                    <span v-if="alert.data.name.length > 0">
                        {{alert.name}}
                    </span>
                    <span v-else>
                        {{alert.data.worldID}}-{{alert.data.instanceID}}
                    </span>
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
            <div class="row mb-3" v-if="alert.state == 'loaded' && alert.data.zoneID != 0">
                <div class="col-12">
                    <hr class="border" />
                    <h4 class="text-center">Territory control</h4>
                </div>

                <div v-if="alert.state == 'idle'"></div>

                <div v-else-if="alert.state == 'loading'" class="col-12">
                    <busy class="honu-busy"></busy>
                </div>

                <div v-if="alert.state == 'loaded' && alert.data.victorFactionID != -1 && alert.data.zoneID != 0 && alert.data.countVS != null" class="col-12">
                    <alert-winner :alert="alert.data"></alert-winner>
                </div>
                <div v-else-if="alert.state == 'loaded'" class="col-12">
                    Territory data not available
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Faction stats</h2>
                </div>

                <div class="col-12 col-lg-4">
                    <alert-faction-stats :alert="alert.data" :data="vsStats"></alert-faction-stats>
                </div>
                <div class="col-12 col-lg-4">
                    <alert-faction-stats :alert="alert.data" :data="ncStats"></alert-faction-stats>
                </div>
                <div class="col-12 col-lg-4">
                    <alert-faction-stats :alert="alert.data" :data="trStats"></alert-faction-stats>
                </div>
            </div>

            <div v-if="alert.state == 'loaded'" class="row">
                <div class="col-12">
                    <h2 class="wt-header">
                        Population

                        <toggle-button v-model="options.showPopTotal">
                            Show total pop
                        </toggle-button>
                    </h2>
                </div>

                <div class="col-12">
                    <alert-population-graph :alert="alert.data" :show-total="options.showPopTotal"></alert-population-graph>
                </div>
            </div>

            <div v-if="showControlComponent" class="row">
                <div class="col-12">
                    <h2 class="wt-header">Capture and Defense (Work in progress)</h2>
                </div>

                <div class="col-6">
                    <span class="text-warning text-center align-middle">
                        map soon
                    </span>
                </div>

                <div class="col-6">
                    <alert-control-events v-if="control.state == 'loaded'" :control="control.data"></alert-control-events>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Kills</h2>
                </div>

                <div class="col-6">
                    <alert-kill-board :alert="alert.data" :participants="participants"></alert-kill-board>
                </div>

                <div class="col-6">
                    <alert-outfit-kill-board :outfits="outfits" :alert="alert.data" :participants="participants"></alert-outfit-kill-board>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Medic</h2>
                </div>

                <div class="col-12">
                    <alert-medic-board :participants="participants" :alert="alert.data"></alert-medic-board>
                </div>

                <div class="col-12">
                    <alert-outfit-medic-board :outfits="outfits" :participants="participants" :alert="alert.data"></alert-outfit-medic-board>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <h2 class="wt-header">Engineer</h2>
                </div>

                <div class="col-12">
                    <alert-engineer-board :participants="participants" :alert="alert.data"></alert-engineer-board>
                </div>

                <div class="col-12">
                    <alert-outfit-engineer-board :outfits="outfits" :alert="alert.data" :participants="participants"></alert-outfit-engineer-board>
                </div>
            </div>
        </div>

        <div v-else-if="participants.state == 'error'" class="text-danger">
            Error loading stats: {{participants.message}}
        </div>

        <div v-else class="text-danger">
            Unchecked state of participants: {{participants.state}}
        </div>

        <popper-modal :value="modalData"></popper-modal>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { AlertParticipantApi, FlattendParticipantDataEntry, AlertPlayerProfileData } from "api/AlertParticipantApi";
    import { PsAlert, AlertApi } from "api/AlertApi";
    import { ExpandedFacilityControlEvent, FacilityControlEventApi } from "api/FacilityControlEventApi";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";
    import EventBus from "EventBus";
    import { PopperModalData } from "popper/PopperModalData";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Busy from "components/Busy.vue";
    import ToggleButton from "components/ToggleButton";
    import PopperModal from "components/PopperModal.vue";

    import AlertFactionStats from "./components/AlertFactionStats.vue";
    import AlertGeneral from "./components/AlertGeneral.vue";
    import AlertKillBoard from "./components/AlertKillBoard.vue";
    import AlertOutfitKillBoard from "./components/AlertOutfitKillBoard.vue";
    import AlertMedicBoard from "./components/AlertMedicBoard.vue";
    import AlertOutfitMedicBoard from "./components/AlertOutfitMedicBoard.vue";
    import AlertEngineerBoard from "./components/AlertEngineerBoard.vue";
    import AlertOutfitEngineerBoard from "./components/AlertOutfitEngineerBoard.vue";
    import AlertWinner from "./components/AlertWinner.vue";
    import AlertControlEvents from "./components/AlertControlEvents.vue";
    import AlertPopulationGraph from "./components/AlertPopulationGraph.vue";

    class OutfitDataEntry {
        public outfitID: string = "";
        public outfitTag: string | null = null;
        public outfitName: string = "";
        public outfitDisplay: string = "";
        public factionID: number = 0;
        public facilityCount: number = 0;

        public secondsOnline: number = 0;
        public members: number = 0;

        public kills: number = 0;
        public killScore: number = 0;
        public deaths: number = 0;
        public vehicleKills: number = 0;
        public spawns: number = 0;
        public kpm: number = 0;
        public kd: number = 0;

        public medicKills: number = 0;
        public medicDeaths: number = 0;
        public medicTimeAs: number = 0;
        public medicPlayers: number = 0;
        public medicKPM: number = 0;
        public medicKD: number = 0;
        public medicHeals: number = 0;
        public medicRevives: number = 0;
        public medicHealsPerMinute: number = 0;
        public medicRevivesPerMinute: number = 0;
        public medicKRD: number = 0;

        public engKills: number = 0;
        public engDeaths: number = 0;
        public engTimeAs: number = 0;
        public engPlayers: number = 0;
        public engKPM: number = 0;
        public engKD: number = 0;
        public engResupplies: number = 0;
        public engRepairs: number = 0;
        public engResuppliesPerMinute: number = 0;
        public engRepairsPerMinute: number = 0;
    }

    export const AlertViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                alertID: 0 as number,

                outfitMap: new Map() as Map<string, OutfitDataEntry>,
                outfits: Loadable.idle() as Loading<OutfitDataEntry[]>,

                showControlComponent: true as boolean,
                modalData: new PopperModalData() as PopperModalData,

                error: {
                    notFinished: false as boolean
                },

                options: {
                    showPopTotal: false as boolean
                },

                alert: Loadable.idle() as Loading<PsAlert>,
                participants: Loadable.idle() as Loading<FlattendParticipantDataEntry[]>,
                control: Loadable.idle() as Loading<ExpandedFacilityControlEvent[]>,

                vsStats: new OutfitDataEntry() as OutfitDataEntry,
                ncStats: new OutfitDataEntry() as OutfitDataEntry,
                trStats: new OutfitDataEntry() as OutfitDataEntry,
            }
        },

        created: function(): void {
            document.title = `Honu / Alert / <loading...>`;

            this.vsStats.factionID = 1;
            this.ncStats.factionID = 2;
            this.trStats.factionID = 3;

            EventBus.$on("set-modal-data", (modalData: PopperModalData) => {
                this.modalData = modalData;
            });
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
                    document.title = `Honu / Alert / ${this.alert.data.displayID}`;

                    if (new Date() < this.alert.data.end) {
                        this.error.notFinished = true;
                    } else {
                        this.loadAlertData();
                        this.loadControlEvents();
                    }
                }
            },

            loadAlertData: async function(): Promise<void> {
                this.participants = Loadable.loading();
                this.participants = await AlertParticipantApi.getByAlertID(this.alertID);

                if (this.participants.state == "loaded") {
                    this.makeOutfitData();
                    this.makeFactionData();
                }
            },

            loadControlEvents: async function(): Promise<void> {
                this.control = Loadable.loading();
                this.control = await FacilityControlEventApi.getByAlertID(this.alertID);
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

                let vsKills: number = 0;
                let ncKills: number = 0;
                let trKills: number = 0;
                let totalKills: number = 0;

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

                    if (outfitEntry.factionID == 1) {
                        vsKills += entry.kills;
                    } else if (outfitEntry.factionID == 2) {
                        ncKills += entry.kills;
                    } else if (outfitEntry.factionID == 3) {
                        trKills += entry.kills;
                    }
                    totalKills += entry.kills;

                    const medicProfile: AlertPlayerProfileData | undefined = entry.profiles.find(iter => iter.profileID == 4);
                    if (medicProfile != undefined && medicProfile.timeAs > 60) {
                        outfitEntry.medicKills += medicProfile.kills;
                        outfitEntry.medicDeaths += medicProfile.deaths;
                        outfitEntry.medicTimeAs += medicProfile.timeAs;
                        outfitEntry.medicHeals += entry.heals;
                        outfitEntry.medicRevives += entry.revives;
                        outfitEntry.medicPlayers += 1;
                    }

                    const engProfile: AlertPlayerProfileData | undefined = entry.profiles.find(iter => iter.profileID == 5);
                    if (engProfile != undefined && engProfile.timeAs > 60) {
                        outfitEntry.engKills += engProfile.kills;
                        outfitEntry.engDeaths += engProfile.deaths;
                        outfitEntry.engTimeAs += engProfile.timeAs;
                        outfitEntry.engResupplies += entry.resupplies;
                        outfitEntry.engRepairs += entry.repairs;
                        outfitEntry.engPlayers += 1;
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
                    outfit.medicKD = outfit.medicKills / Math.max(1, outfit.medicDeaths);
                    outfit.medicKPM = outfit.medicKills / Math.max(1, outfit.medicTimeAs) * 60;
                    outfit.medicKRD = (outfit.medicKills + outfit.medicRevives) / Math.max(1, outfit.medicDeaths);
                    outfit.medicHealsPerMinute = outfit.medicHeals / Math.max(1, outfit.medicTimeAs) * 60;
                    outfit.medicRevivesPerMinute = outfit.medicRevives / Math.max(1, outfit.medicTimeAs) * 60;
                    outfit.engKD = outfit.engKills / Math.max(1, outfit.engDeaths);
                    outfit.engKPM = outfit.engKills / Math.max(1, outfit.engTimeAs) * 60;
                    outfit.engResuppliesPerMinute = outfit.engResupplies / Math.max(1, outfit.engTimeAs) * 60;
                    outfit.engRepairsPerMinute = outfit.engRepairs / Math.max(1, outfit.engTimeAs) * 60;

                    let factionKills = vsKills;
                    if (outfit.factionID == 1) {
                        factionKills = vsKills;
                    } else if (outfit.factionID == 2) {
                        factionKills = ncKills;
                    } else if (outfit.factionID == 3) {
                        factionKills = trKills;
                    } else if (outfit.factionID == 4) {
                        continue;
                    } else {
                        throw `Unhandled faction ID ${outfit.factionID}`;
                    }

                    //outfit.killScore = outfit.kills / factionKills / outfit.members * 100;
                    outfit.killScore = outfit.kills / factionKills / outfit.members * (totalKills / 3);
                }

                console.log(`AlertViewer> Built outfits data`);

                this.outfits = Loadable.loaded(outfits);
            },

            makeFactionData: function(): void {
                this.vsStats = new OutfitDataEntry();
                this.vsStats.factionID = 1;
                this.ncStats = new OutfitDataEntry();
                this.ncStats.factionID = 2;
                this.trStats = new OutfitDataEntry();
                this.trStats.factionID = 3;

                if (this.participants.state != "loaded") {
                    console.warn(`cannot make faction data: participants is not loaded`);
                    return;
                }

                for (const player of this.participants.data) {
                    let stats: OutfitDataEntry | undefined = undefined;

                    if (player.factionID == 1) {
                        stats = this.vsStats;
                    } else if (player.factionID == 2) {
                        stats = this.ncStats;
                    } else if (player.factionID == 3) {
                        stats = this.trStats;
                    }

                    if (stats == undefined) {
                        continue;
                    }

                    stats.kills += player.kills;
                    stats.deaths += player.deaths;
                    stats.spawns += player.spawns;
                    stats.vehicleKills += player.vehicleKills;
                    stats.secondsOnline += player.secondsOnline;

                    const medicProfile: AlertPlayerProfileData | undefined = player.profiles.find(iter => iter.profileID == 4);
                    if (medicProfile != undefined && medicProfile.timeAs > 60) {
                        stats.medicKills += medicProfile.kills;
                        stats.medicDeaths += medicProfile.deaths;
                        stats.medicTimeAs += medicProfile.timeAs;
                        stats.medicHeals += player.heals;
                        stats.medicRevives += player.revives;
                        stats.medicPlayers += 1;
                    }

                    const engProfile: AlertPlayerProfileData | undefined = player.profiles.find(iter => iter.profileID == 5);
                    if (engProfile != undefined && engProfile.timeAs > 60) {
                        stats.engKills += engProfile.kills;
                        stats.engDeaths += engProfile.deaths;
                        stats.engTimeAs += engProfile.timeAs;
                        stats.engResupplies += player.resupplies;
                        stats.engRepairs += player.repairs;
                        stats.engPlayers += 1;
                    }

                    ++stats.members;
                }

                if (this.alert.state == "loaded") {
                    this.vsStats.facilityCount = this.alert.data.countVS || 0;
                    this.ncStats.facilityCount = this.alert.data.countNC || 0;
                    this.trStats.facilityCount = this.alert.data.countTR || 0;
                }
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

        watch: {
            outfits: {
                deep: true,
                handler: function(): void {
                    console.log(`AlertViewer> outfits updated`);
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Busy, ToggleButton, PopperModal,
            AlertWinner, AlertControlEvents, AlertFactionStats, AlertGeneral, AlertPopulationGraph,
            AlertKillBoard, AlertMedicBoard, AlertEngineerBoard, AlertOutfitKillBoard, AlertOutfitMedicBoard, AlertOutfitEngineerBoard
        }
    });
    export default AlertViewer;
</script>