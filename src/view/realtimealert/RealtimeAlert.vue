<template>
    <div style="height: 1080px; width: 1920px; max-width: 1920px">
        <div v-if="view == 'setup'">
            <h2 class="wt-header">Overlay setup</h2>

            <div class="mb-2">
                <div>
                    Select an alert to view:
                </div>

                <div class="list-group border" style="height: 600px; max-height: 600px; overflow-y: scroll;">
                    <div v-for="alert in list.alerts" class="list-group-item d-flex">
                        <span class="flex-grow-1">
                            {{alert.timestamp | moment}} - {{alert.worldID | world}} instance {{alert.zoneID >> 16 & 0x0000FFFF}} ({{alert.worldID}}.{{alert.zoneID}})
                        </span>

                        <button class="btn btn-primary" @click="selectAlert(alert.worldID, alert.zoneID)">
                            Select
                        </button>
                    </div>
                </div>
            </div>

            <div v-if="hasError" class="alert alert-warning">
                <div v-if="alert.worldID == null">
                    Missing world ID. Select an alert above
                </div>

                <div v-if="alert.zoneID == null">
                    Missing zoneID. Select an alert above
                </div>
            </div>

            <div class="mb-2">
                <div>
                    Do you want someone else to be able to control the overlay?
                    <info-hover text="Remote control will let someone else control the overlay, toggling different elements, etc."></info-hover>
                </div>
                <toggle-button v-model="setup.allowRemoteControl">
                    Allow remote control
                </toggle-button>
            </div>

            <div>
                Streamer URL (use this as your Browser source):
                <div class="input-group">
                    <input :value="streamURL" class="form-control" readonly />
                    <span class="input-group-append input-group-addon">
                        <button class="btn btn-success" @click="copyText(streamURL)">
                            Copy
                        </button>
                        <button class="btn btn-secondary" @click="openInNewTab(streamURL)">
                            Open
                        </button>
                    </span>
                </div>
            </div>

            <div v-if="setup.allowRemoteControl">
                Control URL (give this to someone to remote control your overlay):
                <div class="input-group">
                    <input :value="controlURL" class="form-control" />
                    <span class="input-group-append input-group-addon">
                        <button class="btn btn-success" @click="copyText(controlURL)">
                            Copy
                        </button>
                        <button class="btn btn-secondary" @click="openInNewTab(controlURL)">
                            Open
                        </button>
                    </span>
                </div>
            </div>

        </div>

        <div v-else-if="view == 'alert'">
            <img v-if="alert.showExample" src="/img/ow_example.png" width="1920" height="1080" style="position: fixed; z-index: -10;" />

            <img src="/img/overlaybackgroundwicons.png" style="position: fixed; z-index: -5; left: 50%; transform: translateX(-50%);" width="695" height="112" />

            <div class="ps2-text position-fixed" style="font-size: 24pt; left: 50%; transform: translateX(-50%);">
                {{alert.worldID | world}}
            </div>

            <div v-if="alert.data != null && alert.data.tr != null" style="font-size: 14pt; text-align: right; position: fixed; top: 62px; right: 1212px; font-family: ps2; line-height: 1;">
                <realtime-alert-team-view :team="alert.data.tr"></realtime-alert-team-view>
            </div>

            <div :class="[ alert.showPanels ? 'slide-left-in' : 'slide-left-out', 'slider' ]">
                <chart-block-list v-if="stats.componentName == 'list'"
                    :data="stats.block.tr" :clipped-amount="5" :left-title="stats.block.leftTitle" :right-title="stats.block.rightTitle">
                </chart-block-list>
                <chart-block-pie-chart v-else-if="stats.componentName == 'pie'"
                    :data="stats.block.tr" :clipped-amount="5" :left-title="stats.block.leftTitle" :right-title="stats.block.rightTitle"
                    :label-value="false" :show-percent="true" label-position="bottom">
                </chart-block-pie-chart>
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

            <div :class="[ alert.showPanels ? 'slide-right-in' : 'slide-right-out', 'slider' ]">
                <chart-block-list v-if="stats.componentName == 'list'"
                    :data="stats.block.nc" :clipped-amount="5" :left-title="stats.block.leftTitle" :right-title="stats.block.rightTitle">
                </chart-block-list>
                <chart-block-pie-chart v-else-if="stats.componentName == 'pie'"
                    :data="stats.block.nc" :clipped-amount="5" :left-title="stats.block.leftTitle" :right-title="stats.block.rightTitle"
                    :label-value="false" :show-percent="true" label-position="bottom">
                </chart-block-pie-chart>
            </div>

            <div v-if="alert.data != null && alert.data.nc != null" style="font-size: 14pt; text-align: left; position: fixed; top: 62px; left: 1212px; font-family: ps2; line-height: 1;">
                <realtime-alert-team-view :team="alert.data.nc"></realtime-alert-team-view>
            </div>

            <div v-if="alert.showControls" class="position-fixed" style="left: 50%; top: 200px; transform: translateX(-50%); display: grid; column-gap: 0.5rem; align-content: start; align-items: start; grid-template-columns: 1fr 1fr 1fr 1fr;">
                <div class="btn-group btn-group-vertical">
                    <button class="btn btn-primary" @click="remoteControlCall('remoteTopKillers')">
                        Top killers
                    </button>

                    <button class="btn btn-primary" @click="remoteControlCall('remoteTopVehicleKills')">
                        Top vkills
                    </button>

                    <button class="btn btn-primary" @click="remoteControlCall('remoteTopRevives')">
                        Top revives
                    </button>
                </div>

                <div class="btn-group btn-group-vertical">
                    <button class="btn btn-primary" @click="remoteControlCall('remoteTest')">
                        Send test event
                    </button>

                    <button class="btn btn-primary" @click="remoteControlCall('remoteTogglePanels')">
                        Toggle panels
                    </button>

                    <button class="btn btn-primary" @click="remoteControlCall('remoteToggleExample')">
                        Toggle example
                    </button>
                </div>

                <div class="btn-group btn-group-vertical">
                    <button class="btn btn-primary" @click="remoteControlCall('remoteTopWeapons')">
                        Top weapons
                    </button>

                    <button class="btn btn-primary" @click="remoteControlCall('remoteDomainKills')">
                        Domain kills
                    </button>
                </div>

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
    import { Experience, ExpStatApi } from "api/ExpStatApi";
    import { ItemApi, PsItem } from "api/ItemApi";

    import RealtimeAlertTeamView from "./components/RealtimeAlertTeam.vue";
    import TeamIcon from "./components/TeamIcon.vue";

    import ToggleButton from "components/ToggleButton";
    import InfoHover from "components/InfoHover.vue";

    import { Block, BlockEntry } from "breakdown/common";
    import ChartBlockList from "breakdown/ChartBlockList.vue";
    import ChartBlockPieChart from "breakdown/ChartBlockPieChart.vue";

    import CharacterUtils from "util/Character";
    import ItemUtils from "util/ItemUtils";
    import ColorUtils from "util/Color";

    import "MomentFilter";
    import "filters/WorldNameFilter";

    export const RealtimeAlertView = Vue.extend({
        props: {

        },

        data: function() {
            return {
                connection: null as sR.HubConnection | null,

                view: "setup" as "setup" | "alert",

                list: {
                    alerts: [] as RealtimeAlert[]
                },

                intervalID: 0 as number,

                controlCode: "" as string,

                characterCache: new Map() as Map<string, PsCharacter>,

                setup: {
                    allowRemoteControl: false as boolean
                },

                stats: {
                    componentName: "" as string,

                    block: {
                        nc: new Block() as Block,
                        tr: new Block() as Block,
                        leftTitle: "" as string,
                        rightTitle: "" as string
                    }
                },

                alert: {
                    worldID: null as number | null,
                    zoneID: null as number | null,

                    showControls: false as boolean,
                    showExample: false as boolean,

                    showPanels: false as boolean,
                    panelAnimation: false as boolean,

                    data: null as RealtimeAlert | null,
                    full: null as RealtimeAlert | null,

                    skipForOutfit: new Set() as Set<string>,

                    outfitNC: null as PsOutfit | null,
                    outfitTR: null as PsOutfit | null
                }
            }
        },

        mounted: function(): void {
            this.controlCode = `${1_000_000_000 + Math.floor(Math.random() * (1_000_000_000))}`;
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

                this.connection.on("UpdateAlert", this.onUpdateAlert);
                this.connection.on("RemoteCall", this.onRemoteCall);

                this.connection.onreconnected(() => {
                    if (this.alert.worldID != null && this.alert.zoneID != null) {
                        this.subscribe(this.alert.worldID, this.alert.zoneID);
                    }
                    if (this.controlCode != "") {
                        this.setControlCode(this.controlCode);
                    }
                });

                this.connection.start().then(() => {
                    console.log(`RealtimeAlert> socket connected`);
                    this.parseUrl();
                }).catch(err => {
                    console.error(err);
                });

                this.intervalID = setInterval(async () => {
                    if (this.alert.outfitNC != null && this.alert.outfitTR != null) {
                        return;
                    }

                    if (this.alert.worldID == null || this.alert.zoneID == null) {
                        return;
                    }

                    const full: Loading<RealtimeAlert> = await RealtimeAlertApi.getFull(this.alert.worldID, this.alert.zoneID);
                    if (full.state != "loaded") {
                        console.warn(`RealtimeAlert> full alert was not 'loaded', in state: ${full.state}`);
                        return;
                    }

                    this.alert.full = full.data;

                    if (this.alert.outfitNC == null) {
                        this.getOutfitNC();
                    }

                    if (this.alert.outfitTR == null) {
                        this.getOutfitTR();
                    }
                }, 1000 * 10) as unknown as number;
            },

            /**
             * Parse the URL for if we're gonna be listed the realtime alerts or listening to them
             */
            parseUrl: function(): void {
                if (this.connection == null) {
                    return console.warn(`RealtimeAlert> cannot parseURL: connection is null`);
                }

                this.alert.worldID = null;
                this.alert.zoneID = null;

                const params: URLSearchParams = new URLSearchParams(location.search);
                console.log(params);

                if (params.has("worldID")) {
                    const worldID: number = Number.parseInt(params.get("worldID")!);
                    if (Number.isNaN(worldID) == true) {
                        console.warn(`RealtimeAlert> Failed to parse ${params.get("worldID")} to a valid int`);
                    } else {
                        this.alert.worldID = worldID;
                    }
                }

                if (params.has("zoneID")) {
                    const zoneID: number = Number.parseInt(params.get("zoneID")!);
                    if (Number.isNaN(zoneID) == true) {
                        console.warn(`RealtimeAlert> Failed to parse ${params.get("zoneID")} to a valid int`);
                    } else {
                        this.alert.zoneID = zoneID;
                    }
                }

                if (params.has("controlCode")) {
                    this.setControlCode(params.get("contorlCode")!);
                }

                this.alert.showControls = params.has("control");

                if (this.alert.worldID != null && this.alert.zoneID != null) {
                    this.view = "alert";
                    this.subscribe(this.alert.worldID, this.alert.zoneID);
                } else {
                    this.view = "setup";
                    RealtimeAlertApi.getList().then((data: Loading<RealtimeAlert[]>) => {
                        if (data.state == "loaded") {
                            console.log(`RealtimeAlert> Loaded ${data.data.length} realtime alerts`);
                            this.list.alerts = data.data;
                        }
                    });
                }
            },

            copyText: function(text: string): void {
                navigator.clipboard.writeText(text);
            },

            openInNewTab: function(url: string): void {
                window.open(url, "_blank");
            },

            /**
             * Tell the hub what our control code is
             * @param code
             */
            setControlCode: function(code: string): void {
                this.controlCode = code;

                if (this.connection != null) {
                    this.connection.send("SetControlCode", this.controlCode);
                    console.log(`RealtimeAlert> control code set`);
                }
            },

            remoteControlCall: function(action: string): void {
                if (this.connection == null) {
                    return console.warn(`RealtimeAlert> connection is null, cannot remote control call`);
                }

                console.log(`RealtimeAlert> calling remote method: '${action}'`);
                this.connection.send("RemoteControlCall", this.controlCode, action);
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

                if (this.connection != null) {
                    this.connection.send("Subscribe", this.alert.worldID, this.alert.zoneID);
                }
            },

            selectAlert: function(worldID: number, zoneID: number): void {
                this.alert.worldID = worldID;
                this.alert.zoneID = zoneID;
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
                    if (ev.attackerTeamID == teamID && this.alert.skipForOutfit.has(ev.attackerCharacterID) == false) {
                        charID = ev.attackerCharacterID;
                        break;
                    }
                    if (ev.killedTeamID == teamID && this.alert.skipForOutfit.has(ev.killedCharacterID) == false) {
                        charID = ev.killedCharacterID;
                        break;
                    }
                }

                if (charID == null) {
                    for (const ev of team.expEvents) {
                        if (ev.teamID == teamID && this.alert.skipForOutfit.has(ev.sourceID) == false) {
                            charID = ev.sourceID;
                            break;
                        }
                    }
                }

                if (charID == null) {
                    console.log(`RealtimeAlert> cannot get outfit for team ${teamID}, charID is null`);
                    return null;
                }

                console.log(`loading outfit of ${charID} for team ${teamID}`);
                const char: Loading<PsCharacter> = await CharacterApi.getByID(charID);
                if (char.state != "loaded" || char.data.outfitID == null || char.data.outfitID == "") {
                    this.alert.skipForOutfit.add(charID);
                    return null;
                }

                const outfit: Loading<PsOutfit> = await OutfitApi.getByID(char.data.outfitID);
                if (outfit.state == "loaded") {
                    return outfit.data;
                } else {
                    this.alert.skipForOutfit.add(charID);
                }

                return null;
            },

            /**
             * Callback when the hub sends the small update for an alert
             * @param data The mini alert (alert with no events)
             */
            onUpdateAlert: function(data: any): void {
                const alert: RealtimeAlert = RealtimeAlertApi.parse(data);
                this.alert.data = alert;
            },

            topCountWrapper: async function(callback: (team: RealtimeAlertTeam, teamID: number) => Map<string, number>, rightTitle: string): Promise<void> {
                if (this.alert.worldID == null || this.alert.zoneID == null) {
                    return console.warn(`RealtimeAlert> cannot show top revives: worldID or zoneID is null`);
                }

                const full: Loading<RealtimeAlert> = await RealtimeAlertApi.getFull(this.alert.worldID, this.alert.zoneID);
                if (full.state != "loaded") {
                    return console.warn(`RealtimeAlert> cannot show top revives: full returned '${full.state}', not 'loaded'`);
                }

                const nc: Map<string, number> = callback(full.data.nc, 2);
                const tr: Map<string, number> = callback(full.data.tr, 3);

                const ncBlock: Block | null = await this.makeBlock(nc);
                if (ncBlock != null) {
                    this.stats.block.nc = ncBlock;
                }
                const trBlock: Block | null = await this.makeBlock(tr);
                if (trBlock != null) {
                    this.stats.block.tr = trBlock;
                }
                
                this.stats.block.leftTitle = "Player";
                this.stats.block.rightTitle = rightTitle;
                this.stats.componentName = "list";
                this.alert.showPanels = true;
            },

            /**
             * Take a map of character IDs and number of times they did an action, and return a block that
             *  has their character name and outfit tag
             *  
             * @param map
             */
            makeBlock: async function(map: Map<string, number>): Promise<Block | null> {
                const topCharIDs: string[] = Array.from(map.entries())
                    .sort((a, b) => b[1] - a[1])
                    .slice(0, 5)
                    .map(iter => iter[0]);

                const chars: Loading<PsCharacter[]> = await CharacterApi.getByIDs(topCharIDs);
                if (chars.state != "loaded") {
                    console.warn(`RealtimeAlert> failed to make block: getting chars returned '${chars.state}', not 'loaded'`);
                    return null;
                }

                const block: Block = new Block();

                block.entries = topCharIDs.map(iter => {
                    const c: PsCharacter | null = chars.data.find(char => char.id == iter) || null;

                    const b: BlockEntry = new BlockEntry();
                    b.name = (c != null) ? CharacterUtils.getDisplay(c) : `unknown player?`;
                    b.count = map.get(iter)!;

                    return b;
                });

                block.total = Array.from(map.values()).reduce((acc, i) => acc += i, 0);

                return block;
            },

            /**
             * Callback when the hub tells us to perform an action from a remote call
             * @param action Action to be performed
             */
            onRemoteCall: function(action: string): void {
                console.log(`RealtimeAlert> performing action '${action}'`);

                const func: any = (this as any)[action];
                if (func == undefined) {
                    return console.warn(`RealtimeAlert> Cannot call ${action}, func is undefined`);
                }

                if (typeof func != "function") {
                    return console.warn(`RealtimeAlert> Cannot call ${action}, func is not a function`);
                }

                func();
            },

            remoteTest: function(): void {
                console.log(`remote call successful`);
            },

            remoteTogglePanels: function(): void {
                this.alert.showPanels = !this.alert.showPanels;
            },

            remoteToggleExample: function(): void {
                this.alert.showExample = !this.alert.showExample;
            },

            remoteTopKillers: function(): Promise<void> {
                return this.topCountWrapper((team: RealtimeAlertTeam, teamID: number): Map<string, number> => {
                    const m: Map<string, number> = new Map();

                    for (const ev of team.killDeathEvents) {
                        if (ev.attackerTeamID != teamID || ev.attackerTeamID == ev.killedTeamID) { continue; }

                        const charID: string = ev.attackerCharacterID;
                        m.set(charID, (m.get(charID) || 0) + 1);
                    }

                    return m;
                }, "Kills");
            },

            remoteTopRevives: function(): Promise<void> {
                return this.topCountWrapper((team: RealtimeAlertTeam, teamID: number): Map<string, number> => {
                    const m: Map<string, number> = new Map();

                    for (const ev of team.expEvents) {
                        if (ev.teamID != teamID || Experience.isRevive(ev.experienceID) == false) { continue; }

                        const charID: string = ev.sourceID;
                        m.set(charID, (m.get(charID) || 0) + 1);
                    }

                    return m;
                }, "Revives");
            },

            remoteTopVehicleKills: function(): Promise<void> {
                return this.topCountWrapper((team: RealtimeAlertTeam, teamID: number): Map<string, number> => {
                    const m: Map<string, number> = new Map();

                    for (const ev of team.vehicleDestroyEvents) {
                        if (ev.attackerTeamID != teamID || ev.attackerTeamID == ev.killedTeamID) { continue; }

                        const charID: string = ev.attackerCharacterID;
                        m.set(charID, (m.get(charID) || 0) + 1);
                    }

                    return m;
                }, "Vehicle kills");
            },

            remoteTopWeapons: async function(): Promise<void> {
                const cb: (team: RealtimeAlertTeam, teamID: number) => Map<string, number> = (team, teamID) => {
                    const m: Map<string, number> = new Map();

                    for (const ev of team.killDeathEvents) {
                        if (ev.attackerTeamID != teamID || ev.attackerTeamID == ev.killedTeamID) { continue; }

                        const id: string = `${ev.weaponID}`;
                        m.set(id, (m.get(id) || 0) + 1);
                    }

                    return m;
                }

                if (this.alert.worldID == null || this.alert.zoneID == null) {
                    return console.warn(`RealtimeAlert> cannot show top items: worldID or zoneID is null`);
                }

                const full: Loading<RealtimeAlert> = await RealtimeAlertApi.getFull(this.alert.worldID, this.alert.zoneID);
                if (full.state != "loaded") {
                    return console.warn(`RealtimeAlert> cannot show top items: full returned '${full.state}', not 'loaded'`);
                }

                const nc: Map<string, number> = cb(full.data.nc, 2);
                const tr: Map<string, number> = cb(full.data.tr, 3);

                const ncTop: string[] = Array.from(nc.entries())
                    .sort((a, b) => b[1] - a[1])
                    .slice(0, 5)
                    .map(iter => iter[0]);

                const trTop: string[] = Array.from(tr.entries())
                    .sort((a, b) => b[1] - a[1])
                    .slice(0, 5)
                    .map(iter => iter[0]);

                const weaponIDs: string[] = [...ncTop, ...trTop];

                const items: Loading<PsItem[]> = await ItemApi.getByIDs(weaponIDs);
                if (items.state != "loaded") {
                    return;
                }

                const trBlock: Block = new Block();
                trBlock.entries = trTop.map(iter => {
                    const item: PsItem | null = items.data.find(i => i.id == Number.parseInt(iter)) || null;

                    const b: BlockEntry = new BlockEntry();
                    b.name = item?.name ?? `unknown item?`;
                    b.count = tr.get(iter)!;

                    return b;
                });

                trBlock.total = Array.from(tr.values()).reduce((acc, i) => acc += i, 0);
                this.stats.block.tr = trBlock;

                const ncBlock: Block = new Block();
                ncBlock.entries = ncTop.map(iter => {
                    const item: PsItem | null = items.data.find(i => i.id == Number.parseInt(iter)) || null;

                    const b: BlockEntry = new BlockEntry();
                    b.name = item?.name ?? `unknown item?`;
                    b.count = nc.get(iter)!;

                    return b;
                });

                ncBlock.total = Array.from(nc.values()).reduce((acc, i) => acc += i, 0);
                this.stats.block.nc = ncBlock;

                this.stats.block.leftTitle = "Weapon";
                this.stats.block.rightTitle = "Kills";
                this.stats.componentName = "list";
                this.alert.showPanels = true;
            },

            remoteDomainKills: async function(): Promise<void> {
                if (this.alert.worldID == null || this.alert.zoneID == null) {
                    return console.warn(`RealtimeAlert> cannot show top items: worldID or zoneID is null`);
                }

                const full: Loading<RealtimeAlert> = await RealtimeAlertApi.getFull(this.alert.worldID, this.alert.zoneID);
                if (full.state != "loaded") {
                    return console.warn(`RealtimeAlert> cannot show top items: full returned '${full.state}', not 'loaded'`);
                }

                const weaponIDs: Set<number> = new Set();
                for (const ev of full.data.nc.killDeathEvents) {
                    if (ev.attackerTeamID != 2 || ev.attackerTeamID == ev.killedTeamID) { continue; }
                    weaponIDs.add(ev.weaponID);
                }

                for (const ev of full.data.tr.killDeathEvents) {
                    if (ev.attackerTeamID != 3 || ev.attackerTeamID == ev.killedTeamID) { continue; }
                    weaponIDs.add(ev.weaponID);
                }

                const ids: string[] = [];
                weaponIDs.forEach(iter => { ids.push(`${iter}`); });

                const items: Loading<PsItem[]> = await ItemApi.getByIDs(ids);
                if (items.state != "loaded") {
                    return console.warn(`failed to load items`);
                }

                const itemMap: Map<number, PsItem> = new Map();
                for (const i of items.data) {
                    itemMap.set(i.id, i);
                }

                const colors: string[] = ColorUtils.randomColors(0, 4);

                const createBlock = (team: RealtimeAlertTeam, teamID: number): Block => {
                    const b: Block = new Block();

                    const air: BlockEntry = new BlockEntry();
                    air.name = "Air";
                    air.color = colors[2];

                    const max: BlockEntry = new BlockEntry();
                    max.name = "MAX";
                    max.color = colors[0];

                    const inf: BlockEntry = new BlockEntry();
                    inf.name = "Infantry";
                    inf.color = colors[3];

                    const armor: BlockEntry = new BlockEntry();
                    armor.name = "Armor";
                    armor.color = colors[1];

                    b.entries = [inf, max, air, armor];

                    for (const ev of team.killDeathEvents) {
                        if (ev.attackerTeamID != teamID || ev.attackerTeamID == ev.killedTeamID) { continue; }
                        const item: PsItem | null = itemMap.get(ev.weaponID) || null;
                        if (item == null) {
                            continue;
                        }

                        if (ItemUtils.isInfantryWeaponCategoryID(item.categoryID)) {
                            ++inf.count;
                        } else if (ItemUtils.isMaxWeaponCategoryID(item.categoryID)) {
                            ++max.count;
                        } else if (ItemUtils.isAirWeaponCategoryID(item.categoryID)) {
                            ++air.count;
                        } else if (ItemUtils.isArmorWeaponCategoryID(item.categoryID)) {
                            ++armor.count;
                        } else {
                            console.warn(`unchecked item type ID ${item.categoryID}`);
                        }
                        ++b.total;
                    }

                    return b;
                };

                this.stats.block.nc = createBlock(full.data.nc, 2);
                this.stats.block.tr = createBlock(full.data.tr, 3);
                this.stats.block.leftTitle = "Domain";
                this.stats.block.rightTitle = "Kills";

                this.stats.componentName = "pie";
                this.alert.showPanels = true;
            }

        },

        computed: {
            streamURL: function(): string {
                let base: string = `${location.href}/?worldID=${this.alert.worldID}&zoneID=${this.alert.zoneID}`;
                if (this.setup.allowRemoteControl == true) {
                    base += `&controlCode=${this.controlCode}`;
                }

                return base;
            },

            controlURL: function(): string {
                return `${this.streamURL}&control=true`;
            },

            hasError: function(): boolean {
                return this.alert.worldID == null
                    || this.alert.zoneID == null;
            }

        },

        components: {
            RealtimeAlertTeamView,
            TeamIcon,
            ToggleButton, InfoHover,
            ChartBlockList, ChartBlockPieChart
        }
    });
    export default RealtimeAlertView;
</script>