<template>
    <div>
        <div v-if="showUI">
            <honu-menu>
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    Realtime map
                </li>
            </honu-menu>
        </div>

        <div v-if="showUI" class="d-flex mb-2" style="gap: 0.5rem;">
            <div style="width: initial;">
                <toggle-button v-model="layers.terrain">
                    Terrain map
                </toggle-button>

                <toggle-button v-model="layers.owner">
                    Ownership
                </toggle-button>

                <toggle-button v-model="layers.outline">
                    Outline
                </toggle-button>

                <toggle-button v-model="layers.name">
                    Names
                </toggle-button>

                <toggle-button v-model="layers.lattice">
                    Lattice
                </toggle-button>
            </div>

            <div class="input-group" style="width: initial;">
                <span class="input-group-text input-group-prepend">
                    Line thickness
                </span>

                <input v-model.number="settings.thickness" class="form-control" type="number" style="width: 8ch; max-width: 8ch;" />
            </div>

            <div class="input-group" style="width: initial;">
                <span class="input-group-text input-group-prepend">
                    Server
                </span>

                <select v-model.number="settings.worldID" class="form-control" style="width: 20ch; max-width: 20ch;">
                    <option :value="1">Connery</option>
                    <option :value="10">Miller</option>
                    <option :value="13">Cobalt</option>
                    <option :value="17">Emerald</option>
                    <option :value="19">Jaeger</option>
                    <option :value="40">SolTech</option>
                </select>
            </div>

            <div class="input-group" style="width: initial;">
                <span class="input-group-text input-group-prepend">
                    Continent
                </span>

                <select v-model.number="settings.zoneID" class="form-control" style="width: 20ch; max-width: 20ch;">
                    <option :value="2">Indar</option>
                    <option :value="4">Hossin</option>
                    <option :value="6">Amerish</option>
                    <option :value="8">Esamir</option>
                    <option :value="344">Oshur</option>
                </select>
            </div>

            <div style="width: initial;">
                <toggle-button v-model="flip.showUI">
                    Show flip UI
                </toggle-button>
            </div>

            <div style="width: initial;">
                <button @click="copyUrl(true)" class="btn btn-primary" type="button">
                    Copy URL
                    <info-hover text="Hides the UI at the top, useful for stream overlays"></info-hover>
                </button>
            </div>
        </div>

        <div style="height: 100vh;">
            <div v-if="flip.showUI == true" style="position: absolute; right: 0; top: 0; z-index: 1000;" class="list-group">
                <div v-for="(cmd, index) in flip.commands" class="list-group-item">
                    <div class="input-group">
                        <input type="text" :value="cmd" class="form-control" :id="'flip-cmd-' + index" />
                        <div class="input-group-append">
                            <button class="btn btn-primary input-group-addon" @click="copyCommand(index)">Copy</button>
                        </div>
                    </div>
                </div>

                <div class="list-group-item">
                    <div class="btn-group w-100">
                        <button class="btn btn-outline-light" @click="setSelectedFaction(1)" :style="{ 'color': flip.selectedFaction == 1 ? 'var(--color-vs)' : '' }">
                            VS
                        </button>

                        <button class="btn btn-outline-light" @click="setSelectedFaction(2)" :style="{ 'color': flip.selectedFaction == 2 ? 'var(--color-nc)' : '' }">
                            NC
                        </button>

                        <button class="btn btn-outline-light" @click="setSelectedFaction(3)" :style="{ 'color': flip.selectedFaction == 3 ? 'var(--color-tr)' : '' }">
                            TR
                        </button>

                        <button class="btn btn-outline-light" @click="setSelectedFaction(4)" :style="{ 'color': flip.selectedFaction == 4 ? 'var(--color-ns)' : '' }">
                            NS
                        </button>
                    </div>

                    <div v-if="flip.selectedFaction != null" class="mt-2">
                        {{flip.selectedFaction | faction}} is selected,
                        <br />right click will set the base.
                        <br />Click again to toggle
                    </div>
                </div>

                <div class="list-group-item">
                    <button class="btn btn-danger w-100" @click="clearAllFactions">
                        Clear all
                    </button>
                </div>
            </div>

            <div id="realtime-map" class="realtime-map" style="height: 100vh; z-index: 50;"></div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";
    import FactionColors from "FactionColors";

    import ZoneUtils from "util/Zone";

    import * as L from "leaflet";
    import * as sR from "signalR";
    import * as pf from "pathfinding";

    import "node_modules/leaflet-contextmenu/dist/leaflet.contextmenu.js";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuDropdown } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";

    import { ZoneRegion, LatticeLink } from "map/LedgerModels";
    import { PsMapHex, PsFacility, PsFacilityLink, ZoneMap, MapApi } from "api/MapApi";

    import "filters/FactionNameFilter";

    type PsZone = {
        zoneID: number;
        facilities: Map<number, PsFacilityOwner>;
    };

    type PsFacilityOwner = {
        facilityID: number;
        owner: number;
        flipOwner?: number;
    }

    class Path {
        public path: Node[] = [];
        public label: string = "";
        public distance: number = 0;
    }

    class Node {
        public id: number = 0;
        public label: string = "";
        public links: number[] = [];
    }

    export const RealtimeMap = Vue.extend({
        props: {

        },

        data: function() {
            return {
                showUI: true as boolean,
                map: null as L.Map | null,

                zoneData: null as ZoneMap | null,
                ownershipData: null as PsZone | null,
                regionData: [] as ZoneRegion[],
                latticeData: [] as LatticeLink[],

                socketState: "unconnected" as string,
                connection: null as sR.HubConnection | null,
                lastUpdate: null as Date | null,

                settings: {
                    worldID: 1 as number | null,
                    zoneID: 2 as number | null,
                    thickness: 10 as number
                },

                panes: {
                    terrain: null as L.TileLayer | null,
                    color: null as HTMLElement | null,
                    lattice: null as HTMLElement | null,
                },

                layers: {
                    terrain: true as boolean,
                    name: true as boolean,
                    outline: true as boolean,
                    lattice: true as boolean,
                    owner: true as boolean
                },

                flip: {
                    showUI: false as boolean,
                    facilityID: null as number | null,
                    commands: [] as string[],
                    selectedFaction: null as number | null
                }
            }
        },

        created: function(): void {
            document.title = `Honu / Realtime map`;
        },

        mounted: function(): void {
            this.parseQueryParams();
            this.makeSignalR();
            this.$nextTick(() => {
                this.createMap();
            });

            this.dij();
        },

        methods: {
            makeSignalR: function(): void {
                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/realtime-map")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .build();

                this.connection.on("UpdateMap", this.updateMap);

                this.connection.start().then(() => {
                    this.socketState = "opened";
                    console.log(`connected`);

                    this.updateConnection();
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

            setSelectedFaction: function(factionID: number): void {
                if (this.flip.selectedFaction == factionID) {
                    this.flip.selectedFaction = null;
                } else {
                    this.flip.selectedFaction = factionID;
                }
            },

            dij: function(): void {
                let graph = {
                    start: { A: 5, B: 2 },
                    A: { start: 1, C: 4, D: 2 },
                    B: { A: 8, D: 7 },
                    C: { D: 6, finish: 3 },
                    D: { finish: 1 },
                    finish: {}
                };

                let shortestDistanceNode = (distances: any, visited: any) => {
                    // create a default value for shortest
                    let shortest = null;

                    // for each node in the distances object
                    for (let node in distances) {
                        // if no node has been assigned to shortest yet
                        // or if the current node's distance is smaller than the current shortest
                        let currentIsShortest =
                            shortest === null || distances[node] < distances[shortest];

                        // and if the current node is in the unvisited set
                        if (currentIsShortest && !visited.includes(node)) {
                            // update shortest to be the current node
                            shortest = node;
                        }
                    }
                    return shortest;
                };

                let findShortestPath = (graph: any, startNode: any, endNode: any) => {
                    // track distances from the start node using a hash object
                    let distances = {};
                    distances[endNode] = "Infinity";
                    distances = Object.assign(distances, graph[startNode]);// track paths using a hash object
                    let parents = { endNode: null };
                    for (let child in graph[startNode]) {
                        parents[child] = startNode;
                    }

                    // collect visited nodes
                    let visited = [];// find the nearest node
                    let node = shortestDistanceNode(distances, visited);

                    // for that node:
                    while (node) {
                        // find its distance from the start node & its child nodes
                        let distance = distances[node];
                        let children = graph[node];

                        // for each of those child nodes:
                        for (let child in children) {

                            // make sure each child node is not the start node
                            if (String(child) === String(startNode)) {
                                continue;
                            } else {
                                // save the distance from the start node to the child node
                                let newdistance = distance + children[child];// if there's no recorded distance from the start node to the child node in the distances object
                                // or if the recorded distance is shorter than the previously stored distance from the start node to the child node
                                if (!distances[child] || distances[child] > newdistance) {
                                    // save the distance to the object
                                    distances[child] = newdistance;
                                    // record the path
                                    parents[child] = node;
                                }
                            }
                        }
                        // move the current node to the visited set
                        visited.push(node);// move to the nearest neighbor node
                        node = shortestDistanceNode(distances, visited);
                    }

                    // using the stored paths from start node to end node
                    // record the shortest path
                    let shortestPath = [endNode];
                    let parent = parents[endNode];
                    while (parent) {
                        shortestPath.push(parent);
                        parent = parents[parent];
                    }
                    shortestPath.reverse();

                    //this is the shortest path
                    let results = {
                        distance: distances[endNode],
                        path: shortestPath,
                    };
                    // return the shortest path & the end node's distance from the start node
                    return results;
                };

                console.log(findShortestPath(graph, "start", "finish"));

            },

            setFaction: function(ev: any, factionID: number | null, connectWarpgate: boolean): void {
                console.log("event", ev);
                console.log(`setting facility ${this.flip.facilityID} to ${factionID}`);

                if (this.flip.facilityID == null) { return console.warn(`cannot setFaction: flip.facilityID is null`); }
                if (this.zoneData == null) { return console.warn(`cannot setFaction: zoneData is null`); }
                if (this.ownershipData == null) { return console.warn(`cannot setFaction: ownershipData is null`); }

                const facility: PsFacility | undefined = this.zoneData.facilities.find(iter => iter.facilityID == this.flip.facilityID);
                if (facility == undefined) {
                    console.warn(`cannot setFaction: facility ${this.flip.facilityID} was not found`);
                    return;
                }

                const owner: PsFacilityOwner | undefined = this.ownershipData.facilities.get(this.flip.facilityID);
                if (owner == null) {
                    return console.warn(`cannot setFaction: ownership for ${this.flip.facilityID} was not found`);
                }

                this.flip.showUI = true;

                this.pathfind(205001, this.flip.facilityID);

                // construct adjacency matrix

                // create what facility ID is in what index
                const indexes: Map<number, number> = new Map(); // <facility id, index>
                let index: number = 0;
                for (const kvp of this.ownershipData.facilities) {
                    indexes.set(kvp[0], index++);
                }
                console.log(indexes);

                // Create the adjacency matrix
                const graph: number[][] = [];
                graph.length = indexes.size;
                for (let i = 0; i < indexes.size; ++i) {
                    graph[i] = [];
                    graph[i].length = indexes.size;
                    for (let j = 0; j < indexes.size; ++j) {
                        graph[i][j] = 1;
                    }
                }
                console.log(graph);

                // set what facilities are adjacent to each other
                for (const link of this.zoneData.links) {
                    const indexA: number | undefined = indexes.get(link.facilityA);
                    const indexB: number | undefined = indexes.get(link.facilityB);

                    if (indexA == undefined && indexB == undefined) {
                        throw `Failed to find index for facilityA (${link.facilityA}) and facilityB (${link.facilityB})`;
                    }
                    if (indexA == undefined) {
                        throw `Failed to find index for facilityA (${link.facilityA})`;
                    }
                    if (indexB == undefined) {
                        throw `Failed to find index for facilityB (${link.facilityB})`;
                    }

                    graph[indexA][indexB] = 0;
                    graph[indexB][indexA] = 0;
                }
                console.log(graph);

                const grid = new pf.Grid(graph);

                /*
                // this isn't done lul
                if (connectWarpgate == true) {
                    if (this.zoneData == null) {
                        return console.error(`cannot setFaction: zoneData is null`);
                    }

                    // Steps:
                    // 1. find warpgate
                    // 2. find path from wanted base to warpgate
                    // 3. ensure none of the bases that would be flipped are already pending a flip

                    // 1. find warpgates
                    const warpgates: PsFacility[] = this.zoneData.facilities.filter(iter => iter.typeID == 7); // 7 = warpgate
                    const warpgateIDs: number[] = warpgates.map(iter => iter.facilityID);
                    console.log(`found ${warpgates.length} warpgates when connected ${this.flip.facilityID}`);

                    const warpgateOwners: PsFacilityOwner[] = Array.from(this.ownershipData.facilities.values())
                        .filter(iter => warpgateIDs.indexOf(iter.facilityID) > -1);

                    if (warpgateOwners.length != warpgates.length) {
                        return console.error(`cannot setFaction: warpgateOwners is not the same length as warpgates (${warpgateOwners.length} != ${warpgates.length})`);
                    }

                    const factionWarpgates: PsFacilityOwner[] = warpgateOwners.filter(iter => iter.owner == factionID);
                    if (factionWarpgates.length != 1) {
                        return console.error(`cannot setFaction: there are ${factionWarpgates.length} warpgates owned by ${factionID}, expected 1`);
                    }

                    const warpgateID: number = factionWarpgates[0].facilityID;
                    const facilityID: number = this.flip.facilityID;

                    if (warpgateID == facilityID) {
                        return console.error(`cannot setFaction: warpgateID (${warpgateID}) is the same as the target facilityID (${facilityID})`);
                    }

                    const queue: number[] = [];
                    queue.push(facilityID);
                    const visited: number[] = [];

                    let iterFallback: number = 100; // how many times max to find the path

                    let iter: number | undefined = queue.shift();

                    while (iter != undefined) {
                        if (iterFallback-- < 0) {
                            console.error(`hit maximum iteration count!`);
                            break;
                        }

                        const links: PsFacilityLink[] = this.zoneData.links.filter(i => i.facilityA == iter || i.facilityB == iter);
                    }

                    // 2. find path from wanted base to warpgate
                }
                */

                owner.flipOwner = factionID ?? undefined;

                this.updateFlipCommands();
                this.redrawMap({ ownership: true });
            },

            pathfind: function(facilityA: number, facilityB: number): number[] {
                if (this.zoneData == null) {
                    throw `cannot pathfind from ${facilityA} to ${facilityB}: zoneData is null`;
                }
                if (this.ownershipData == null) {
                    throw `cannot pathfind from ${facilityA} to ${facilityB}: ownershipData is null`;
                }

                // create what facility ID is in what index
                const indexes: Map<number, number> = new Map(); // <facility id, index>
                let index: number = 0;
                for (const kvp of this.ownershipData.facilities) {
                    indexes.set(kvp[0], index++);
                }
                console.log(indexes);

                // Create the adjacency matrix
                const graph: boolean[][] = [];
                graph.length = indexes.size;
                for (let i = 0; i < indexes.size; ++i) {
                    graph[i] = [];
                    graph[i].length = indexes.size;
                    for (let j = 0; j < indexes.size; ++j) {
                        graph[i][j] = false;
                    }
                }
                console.log(graph);

                // set what facilities are adjacent to each other
                for (const link of this.zoneData.links) {
                    const indexA: number | undefined = indexes.get(link.facilityA);
                    const indexB: number | undefined = indexes.get(link.facilityB);

                    if (indexA == undefined && indexB == undefined) {
                        throw `Failed to find index for facilityA (${link.facilityA}) and facilityB (${link.facilityB})`;
                    }
                    if (indexA == undefined) {
                        throw `Failed to find index for facilityA (${link.facilityA})`;
                    }
                    if (indexB == undefined) {
                        throw `Failed to find index for facilityB (${link.facilityB})`;
                    }

                    graph[indexA][indexB] = true;
                    graph[indexB][indexA] = true;
                }
                console.log(graph);

                let start: boolean = true;
                const facilities: PsFacility[] = this.zoneData.facilities;
                const links: PsFacilityLink[] = this.zoneData.links;
                let completed: Path[] = [];

                const travel = (node: Node, path: Node[], total: number): void => {
                    if (start == true) {
                        path.push(node);
                        start = false;
                    }

                    if (path.length == facilities.length) {
                        const p: Path = new Path();
                        for (let i = 0; i < path.length; ++i) {
                            p.path.push(path[i]);
                        }
                        p.distance = total;
                        completed.push(p);

                        console.log(`path length before: ${path.length}`);
                        path = path.slice(0, -1);
                        console.log(`path length after: ${path.length}`);
                        return;
                    }

                    const iterID: number = node.id;
                    const index: number | undefined = indexes.get(iterID);
                    if (index == undefined) {
                        throw `failed to find index for ${iterID}`;
                    }

                    // get the links the facility being iterated over has
                    const iterLinks: Set<number> = new Set();
                    for (const iter of links) {
                        if (iter.facilityA == iterID && iter.facilityB != iterID) {
                            iterLinks.add(iter.facilityB);
                        }
                        if (iter.facilityB == iterID && iter.facilityA != iterID) {
                            iterLinks.add(iter.facilityA);
                        }
                    }

                    for (const linkID of iterLinks) {
                        let isin: boolean = false;
                        for (let i = 0; i < path.length; ++i) {
                            if (path[i].id == linkID) {
                                isin = true;
                                break;
                            }
                        }

                        if (isin == true) {
                            console.log(`link to ${linkID} is already in the path, skipping`);
                            continue;
                        }

                        const n: Node = new Node();
                        n.id = linkID;
                        path.push(n);
                        travel(n, path, total + 1);
                    }
                };

                const s: Node = new Node();
                s.id = facilityA;
                travel(s, [], 0);

                debugger;

                return [];
            },

            setFactionVS: function(ev: any): void { this.setFaction(ev, 1, false); },
            setFactionVSWG: function(ev: any): void { this.setFaction(ev, 1, true); },
            setFactionNC: function(ev: any): void { this.setFaction(ev, 2, false); },
            setFactionNCWG: function(ev: any): void { this.setFaction(ev, 2, true); },
            setFactionTR: function(ev: any): void { this.setFaction(ev, 3, false); },
            setFactionTRWG: function(ev: any): void { this.setFaction(ev, 3, true); },
            setFactionNS: function(ev: any): void { this.setFaction(ev, 4, false); },
            clearFaction: function(ev: any): void { this.setFaction(ev, null, false); },

            clearAllFactions: function(): void {
                this.flip.commands = [];
                this.flip.selectedFaction = null;

                if (this.ownershipData == null) {
                    return;
                }

                this.ownershipData.facilities.forEach((owner: PsFacilityOwner) => {
                    owner.flipOwner = undefined;
                });

                this.redrawMap({ ownership: true });
            },

            updateFlipCommands: function(): void {
                this.flip.commands = [];

                if (this.ownershipData == null) {
                    return console.warn(`cannot update flip commands: ownershipData is null`);
                }

                let currentCommand: string = "/alias a:facility setfaction;alias v:a 1; alias n:a 2; alias t:a 3; alias s:a 4;";

                this.ownershipData.facilities.forEach((owner: PsFacilityOwner) => {
                    if (owner.flipOwner != undefined && owner.owner != owner.flipOwner) {
                        let iterCmd: string = "";

                        if (owner.flipOwner == 1) {
                            iterCmd += "v ";
                        } else if (owner.flipOwner == 2) {
                            iterCmd += "n ";
                        } else if (owner.flipOwner == 3) {
                            iterCmd += "t ";
                        } else if (owner.flipOwner == 4) {
                            iterCmd += "s ";
                        }

                        iterCmd += `${owner.facilityID}`;

                        if (currentCommand.length + iterCmd.length + 2 > 120) {
                            this.flip.commands.push(currentCommand);

                            currentCommand = `/${iterCmd}`;
                        } else {
                            currentCommand += `${iterCmd};`;
                        }
                    }
                });

                if (currentCommand.length > 0) {
                    this.flip.commands.push(currentCommand);
                }
            },

            copyCommand: function(index: number): void {
                if (index < 0 || index > this.flip.commands.length - 1) {
                    return console.warn(`cannot copy command index ${index}: this is out of bound (0 - ${this.flip.commands.length - 1}`);
                }

                const cmd: HTMLElement | null = document.getElementById(`flip-cmd-${index}`);
                if (cmd == null) {
                    return console.warn(`cannot copy command index ${index}: failed to find #flip-cmd-${index}`);
                }
                if (cmd.tagName != "INPUT") {
                    return console.warn(`cannot copy command index ${index}: #flip-cmd-${index} is not an <input> element (is a <${cmd.tagName}>)`);
                }

                const value: string = (cmd as HTMLInputElement).value;

                navigator.clipboard.writeText(value);
            },

            /**
             * Create a fresh copy of the map, clearing old previous data if needed
             */
            createMap: async function(): Promise<void> {
                if (this.settings.zoneID == null) {
                    console.warn(`realtime-map> Cannot create map, no zone ID selected`);
                    return;
                }

                if (this.map != null) {
                    this.map.remove();
                    this.map = null;
                }

                if (document.getElementById("realtime-map") == null) {
                    throw `Cannot load map, the element #realtime-map is null`;
                }

                const options: L.MapOptions = {
                    crs: L.CRS.Simple,
                    zoomSnap: 0.1,
                    zoomDelta: 0.1
                };

                // The contextmanu plugin is only Javascript, and doesn't update the type for L.MapOptions
                // so, the ones that are typed are pulled out above
                this.map = new L.Map("realtime-map", {
                    ...options,
                    contextmenu: true,
                    contextmenuWidth: 140,
                    contextmenuItems: [
                        {
                            "text": "Set VS",
                            "iconCls": "setfaction-vs",
                            "callback": this.setFactionVS
                        },
                        {
                            "text": "Set NC",
                            "iconCls": "setfaction-nc",
                            "callback": this.setFactionNC
                        },
                        {
                            "text": "Set TR",
                            "iconCls": "setfaction-tr",
                            "callback": this.setFactionTR
                        },
                        {
                            "text": "Set NS",
                            "iconCls": "setfaction-ns",
                            "callback": this.setFactionNS
                        },
                        {
                            "text": "Clear",
                            "iconCls": "setfaction-ns",
                            "callback": this.clearFaction
                        },
                        {
                            "text": "Clear all",
                            "iconCls": "setfaction-ns",
                            "callback": this.clearAllFactions
                        },
                    ]
                } as any).setZoom(2.63).setView(L.latLng(0, 0));

                const zoneName: string = ZoneUtils.getZoneName(this.settings.zoneID).toLowerCase();

                this.panes.terrain = L.tileLayer(`/img/zones/${zoneName}/zoom{z}/${zoneName}_{z}_{x}_{y}.jpg`, {
                    minZoom: 1,
                    maxZoom: 5,
                    noWrap: true,
                    bounds: L.latLngBounds(L.latLng(-128, -128), L.latLng(128, 128))
                }).addTo(this.map);

                this.panes.color = this.map.createPane("regions");
                this.panes.lattice = this.map.createPane("lattices");

                const zoneData: Loading<ZoneMap> = await MapApi.getZone(this.settings.zoneID);
                if (zoneData.state != "loaded") {
                    throw `Failed to get zone data for ${this.settings.zoneID}`;
                }
                this.zoneData = zoneData.data;

                const regions: ZoneRegion[] = ZoneRegion.setupMapRegions(zoneData.data);

                this.regionData = [];
                for (const region of regions) {
                    const fac: PsFacility | null = zoneData.data.facilities.find(iter => iter.regionID == region.regionID) || null;
                    region.facility = fac;

                    if (fac != null && this.ownershipData != null) {
                        const ownership: PsFacilityOwner | undefined = this.ownershipData.facilities.get(fac.facilityID);
                        if (ownership != undefined) {
                            region.ownerID = ownership.owner;
                        }
                    }

                    let name: string = fac?.name ?? "";

                    region.bindTooltip(name, {
                        permanent: true,
                        direction: "center",
                        className: "realtime-map-base-name",
                        interactive: true
                    });

                    region.on("contextmenu", (ev: L.LeafletMouseEvent) => {
                        console.log(`contextmenu`, ev);
                        this.flip.facilityID = null;

                        if (!ev.target) {
                            console.warn(`missing target from event, cannot set flip.facilityID`);
                            return;
                        }

                        if (!ev.target.facility) {
                            console.warn(`missing target.facility from event, cannot set flip.facilityID`);
                            return;
                        }

                        this.flip.facilityID = ev.target.facility.facilityID;
                        console.log(`flip.facilityID: ${this.flip.facilityID}`);

                        // Right click also opens the contextmenu plugin, prevent that if there is a selected faction
                        if (this.flip.selectedFaction != null) {
                            this.setFaction({}, this.flip.selectedFaction, false);
                            L.DomEvent.stopPropagation(ev);
                        }
                    });

                    region.addTo(this.map);
                    this.regionData.push(region);

                    if (this.layers.name == false) {
                        region.closeTooltip();
                    }
                }

                this.latticeData = [];
                for (const region of regions) {
                    if (region.facility == null) {
                        continue;
                    }

                    for (const link of zoneData.data.links) {
                        if (link.facilityA == region.facility.facilityID) {
                            const regionB: ZoneRegion | null = regions.find(iter => iter.facility != null && iter.facility.facilityID == link.facilityB) || null;
                            if (regionB != null) {
                                const lattice: LatticeLink = new LatticeLink(region, regionB, "#aaaaaa");
                                lattice.setThickness(this.settings.thickness);
                                lattice.addTo(this.map);

                                this.latticeData.push(lattice);
                            }
                        }
                    }
                }

                this.redrawMap({ terrain: true, ownership: true });
            },

            /**
             * Callback for when new data is received from the websocket. Updates the ownership and zone data, and if needed, redraws the map
             * @param data
             */
            updateMap: function(data: any): void {
                let panesToRedraw = {
                    terrain: false,
                    ownership: false
                };

                let needsRefresh: boolean = false;
                if (this.ownershipData == null) {
                    console.log(`needs refresh: ownershipData is null (initial render)`);
                    needsRefresh = true;
                    panesToRedraw.terrain = true;
                    panesToRedraw.ownership = true;
                }

                // Transform the object into a map with numberic keys
                const zone: PsZone = data;
                if (this.ownershipData != null && this.ownershipData.zoneID != zone.zoneID) {
                    console.log(`needs refresh: zone has changed`);
                    needsRefresh = true;
                    panesToRedraw.terrain = true;
                    panesToRedraw.ownership = true;
                }

                zone.facilities = new Map(Object.entries(zone.facilities).map(iter => {
                    return [Number.parseInt(iter[0]), iter[1]];
                }));

                if (this.ownershipData != null) {
                    this.ownershipData.facilities.forEach((owner: PsFacilityOwner, facilityID: number) => {
                        const newOwner: PsFacilityOwner | null = zone.facilities.get(facilityID) ?? null;
                        if (newOwner == null) {
                            console.log(`needs refresh: facility ID ${facilityID} is missing in new data`);
                            needsRefresh = true;
                            panesToRedraw.ownership = true;
                            return;
                        }

                        if (newOwner.owner != owner.owner) {
                            console.log(`needs refresh: facility ID ${facilityID} was owned by ${owner.owner}, now owned by ${newOwner.owner}`);
                            needsRefresh = true;
                            panesToRedraw.ownership = true;
                            return;
                        }
                    });
                }

                this.ownershipData = zone;

                console.log(this.ownershipData);

                if (needsRefresh == true) {
                    this.redrawMap(panesToRedraw);
                }
            },

            /**
             * Redraw/Render the map, optionally specifying what parts of the map to re-render
             * @param panes
             */
            redrawMap: function(panes?: { terrain?: boolean, ownership?: boolean }): void {
                if (this.ownershipData == null) {
                    console.warn(`realtime-map> this.ownershipData is null, cannot redraw cuz i got no data`);
                    return;
                }

                console.log(`RealtimeMap> redraw map with options:`, panes);

                if (this.panes.terrain != null && panes?.terrain == true) {
                    this.panes.terrain.setOpacity(this.layers.terrain == true ? 1 : 0);
                    this.panes.terrain.redraw();
                }

                // Fill in each region based on who owns it
                this.ownershipData.facilities.forEach((owner: PsFacilityOwner, facilityID: number) => {

                    const region: ZoneRegion | undefined = this.regionData.find(iter => iter.facility?.facilityID == facilityID);
                    if (region != undefined) {

                        // If the flip owner is different than the current owner, draw a gold outline around it
                        // undefined color means use the fill color
                        const isPendingFlip: boolean = !(owner.flipOwner == undefined || owner.flipOwner == owner.owner);
                        region.setStyle({
                            opacity: this.layers.outline == true ? 1 : 0,
                            fill: true,
                            color: !isPendingFlip ? "#FFFFFF" : FactionColors.getFactionColor(owner.flipOwner ?? owner.owner),
                            fillColor: FactionColors.getFactionColor(owner.owner),
                            fillOpacity: (this.layers.owner == true) ? this.regionFillOpacity : 0,
                            weight: isPendingFlip ? 15 : 3
                        });
                        region.ownerID = owner.owner;

                        if (panes?.ownership == true) {
                            region.redraw();
                        }

                        //console.log(`${region.facility?.name} => ${owner.owner}`);

                        // Update the lattice link colors for the lattice links on this facility
                        for (const link of this.latticeData) {
                            if (link.facilityA.regionID == region.regionID && link.facilityB.regionID == region.regionID) { // Skip connections to self
                                continue;
                            }
                            if (link.facilityA.regionID != region.regionID && link.facilityB.regionID != region.regionID) { // Skip connections not adjacent
                                continue;
                            }

                            let other: ZoneRegion = (link.facilityA == region) ? link.facilityB : link.facilityA;
                            if (other == null) {
                                continue;
                            }

                            if (other.ownerID == 0 || region.ownerID == 0) {
                                link.setOpacity(0);
                            } else {
                                link.setOpacity(1);
                            }

                            if (other.ownerID == region.ownerID) {
                                link.setLinkColor(FactionColors.getFactionColor(region.ownerID));
                            } else {
                                link.setLinkColor("#ffff00");
                            }
                        }
                    }
                });

                if (this.panes.lattice != null) {
                    this.panes.lattice.style.opacity = this.layers.lattice == true ? "1" : "0";
                }
            },

            /**
             * Tell the websocket hub what data our connection wants to recieve
             */
            updateConnection: async function(): Promise<void> {
                if (this.connection == null) {
                    console.warn(`realtime-map> this.connection is null, cannot update connection`);
                    return;
                }

                await this.connection.send("Initalize", this.settings.worldID, this.settings.zoneID);
            },

            /**
             * Parses the query parameters into the options used to control how the map is rendered
             */
            parseQueryParams: function(): void {
                const params: URLSearchParams = new URLSearchParams(location.search);

                this.showUI = params.get("showUI") != "false";

                const worldID: number = Number.parseInt(params.get("worldID") || "");
                if (Number.isNaN(worldID) == false) {
                    this.settings.worldID = worldID;
                }

                const zoneID: number = Number.parseInt(params.get("zoneID") || "");
                if (Number.isNaN(zoneID) == false) {
                    this.settings.zoneID = zoneID;
                }

                const hidden: string | null = params.get("hide");
                if (hidden != null) {
                    const parts: string[] = hidden.split(",");

                    for (const part of parts) {
                        console.log(`realtime-map> turning off ${part}`);
                        if (part == "terrain") { this.layers.terrain = false; }
                        if (part == "name") { this.layers.name = false; }
                        if (part == "outline") { this.layers.outline = false; }
                        if (part == "lattice") { this.layers.lattice = false; }
                        if (part == "owner") { this.layers.owner = false; }
                    }
                }
            },

            getUrl: function(showUI: boolean = true): string {
                const params: URLSearchParams = new URLSearchParams();

                if (this.settings.worldID != null) {
                    params.set("worldID", this.settings.worldID.toString());
                }
                if (this.settings.zoneID != null) {
                    params.set("zoneID", this.settings.zoneID.toString());
                }

                const hide: string[] = [];

                if (this.layers.terrain == false) { hide.push("terrain"); }
                if (this.layers.lattice == false) { hide.push("lattice"); }
                if (this.layers.name == false) { hide.push("name"); }
                if (this.layers.outline == false) { hide.push("outline"); }
                if (this.layers.owner == false) { hide.push("owner"); }

                if (hide.length > 0) {
                    params.set("hide", hide.join(","));
                }

                if (showUI == true) {
                    params.set("showUI", "true");
                } else {
                    params.set("showUI", "false");
                }

                const newUrl: string = `${window.location.protocol}//${window.location.host}${window.location.pathname}?${params.toString()}`;

                return newUrl;
            },

            /**
             * Update the query parameters in the URL with the settings the user has selected
             * @param forceShowUI
             */
            updateQueryParams: function(): void {
                const url: string = this.getUrl(true);
                window.history.pushState({ path: url }, '', url);
            },

            copyUrl: function(forceShowUI: boolean = false): void {
                const url: string = this.getUrl(forceShowUI);
                navigator.clipboard.writeText(url);
            }
        },

        watch: {
            "layers.terrain": function(): void {
                if (this.panes.terrain == null) {
                    return;
                }

                if (this.layers.terrain == true) {
                    this.panes.terrain.setOpacity(1);
                } else {
                    this.panes.terrain.setOpacity(0);
                }
                this.panes.terrain.redraw();

                for (const region of this.regionData) {
                    region.setStyle({
                        fillOpacity: this.layers.owner == true ? this.regionFillOpacity : 0
                    });

                    region.redraw();
                }

                this.updateQueryParams();
            },

            "layers.owner": function(): void { this.redrawMap({ ownership: true }); this.updateQueryParams(); },
            "layers.outline": function(): void { this.redrawMap({ ownership: true }); this.updateQueryParams(); },
            "layers.lattice": function(): void { this.redrawMap({ ownership: true }); this.updateQueryParams(); },

            "layers.name": function(): void {
                for (const region of this.regionData) {
                    if (this.layers.name == true) {
                        region.openTooltip();
                    } else {
                        region.closeTooltip();
                    }
                }
            },

            "settings.zoneID": async function(): Promise<void> {
                if (this.socketState == "opened") {
                    await this.updateConnection();
                    this.createMap();
                    this.updateQueryParams();
                }
            },

            "settings.worldID": function(): void {
                if (this.socketState == "opened") {
                    this.updateConnection();
                    this.updateQueryParams();
                }
            },

            "settings.thickness": function(): void {
                this.createMap();
            }
        },

        computed: {
            regionFillOpacity: function(): number {
                if (this.layers.terrain == false) {
                    return 1;
                }
                return 0.6;
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover,
            HonuMenu, MenuSep, MenuDropdown,
            ToggleButton
        }
    });
    export default RealtimeMap;
</script>