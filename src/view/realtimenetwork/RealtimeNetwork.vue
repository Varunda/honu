<template>
    <div style="height: 100vh;">
        <div class="position-fixed" style="z-index: 100;">
            <div>
                <honu-menu>
                    <menu-dropdown></menu-dropdown>

                    <menu-sep></menu-sep>

                    <li class="nav-item h1 p-0">
                        Realtime Network
                    </li>

                    <template v-if="worldID != null">
                        <menu-sep></menu-sep>

                        <li class="nav-item h1 p-0">
                            {{worldID | world}}
                        </li>
                    </template>
                </honu-menu>
            </div>

            <div style="max-width: 15vh;">
                <button type="button" class="btn btn-primary w-100 mb-3" data-toggle="modal" data-target="#help-modal">
                    What is this?
                </button>

                <toggle-button v-model="auto" class="d-block w-100 mb-2">
                    Toggle auto-update
                </toggle-button>

                <div class="mb-3">
                    <label class="mb-0">Node filter</label>
                    <input v-model="filter" class="form-control" placeholder="Filter" />
                </div>

                <div class="mb-3">
                    <label class="mb-0">Layout method</label>
                    <select v-model="settings.preferedLayout" class="form-control">
                        <option value="atlas">ForceAtlas2</option>
                        <option value="force">Force</option>
                    </select>
                </div>

                <button type="button" class="btn btn-warning w-100 mb-3" @click="resetGraph">
                    Reset graph
                </button>

                <div>
                    <div>
                        Socket state: {{socketState}}
                    </div>

                    <div>
                        Last update: {{lastUpdate | moment("hh:mm:ss")}}
                    </div>

                    <div>
                        Nodes: {{allc.size}}
                    </div>

                    <div>
                        Updates/sec: {{rendersPerSecond | locale(2)}}
                    </div>
                </div>
            </div>
        </div>

        <div class="position-fixed" style="z-index: 100; right: 2rem; top: 15px; background-color: #222;">
            <div v-if="selected != null">
                <h2>
                    <a :href="'/c/' + selected.characterID">
                        {{selected.display}}
                    </a>
                </h2>

                <div class="input-grid-col2" style="grid-template-columns: 1fr min-content; row-gap: 0.5rem; justify-content: left;">
                    <template v-for="inter in selected.interactions">
                        <div class="input-cell">
                            <a :href="'/c/' + inter.otherID">
                                {{inter.otherName}}
                            </a>
                        </div>

                        <div class="input-cell mr-2">
                            {{inter.strength * 10000 | locale(2)}}
                        </div>
                    </template>
                </div>
            </div>
        </div>

        <div id="graph" class="w-100 h-100" style="display: block; z-index: -100;"></div>

        <div class="modal" id="help-modal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">What is this?</h5>

                        <button type="button" class="close" data-dismiss="modal">
                            &times;
                        </button>
                    </div>

                    <div class="modal-body">
                        <p>
                            The realtime network is a graph that shows the interactions between characters as they happen in real time.
                            This shows an approximation of the proximity of characters, but <b>NOT</b> where they are located on a continent
                        </p>

                        <p>
                            Each cluster of nodes represents a cluster of charactes who have recently interacted with each other.
                            Nodes will zoom around as they interact with different characters
                        </p>

                        <span>
                            Only the most recent 3 minutes of events are used to build this graph. The following events are used:
                        </span>

                        <ul>
                            <li><b>All: </b>Kills, assists</li>
                            <li><b>Medic: </b>Heals, revives, shield repairs</li>
                            <li><b>Engineer: </b>Resupply, MAX repairs</li>
                        </ul>

                        <div>
                            <h4 class="wt-header">Circles/Nodes</h4>

                            <p>
                                Each node represents a single character. You can click on a node to see more information
                            </p>

                            <p>
                                Every node has connections to other nodes, drawn as lines between nodes
                            </p>
                        </div>

                        <div>
                            <h4 class="wt-header">Connections/Edges</h4>

                            <p>
                                Each edge is weighted based on how many interactions took place between characters, and how long ago those events took place
                            </p>

                            <p>
                                The color of an edge is will scale on the weight of a connection.
                                A white edge is a heavy interaction, with darker edges reprsenting lighter connections
                            </p>
                        </div>

                        <div>
                            <h4 class="wt-header">Controls</h4>

                            <p>
                                <b>Auto update: </b>Will the graph auto-update periodically?
                            </p>

                            <p>
                                <b>Node filter: </b>Filter the displayed nodes that match the input (case-insensitive)
                            </p>

                            <p>
                                <b>Layout method: </b>What method will be used to move the nodes around and cluster them
                            </p>

                            <p>
                                <b>Reset graph: </b>Reset the graph to its initial state, useful if data is too clumped
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuDropdown } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";
    import Busy from "components/Busy.vue";

    import Graph from "graphology";
    import Sigma from "sigma";
    import FA2Layout from "node_modules/graphology-layout-forceatlas2/worker";
    import ForceSupvisor from "node_modules/graphology-layout-force/worker";

    import { RealtimeNetwork, RealtimeNetworkApi, RealtimeNetworkPlayer } from "api/RealtimeNetworkApi";

    import * as sR from "signalR";

    import "filters/LocaleFilter";
    import "MomentFilter";

    import ColorUtil from "util/Color";
    import WorldUtil from "util/World";

    function randomPosition(width: number = 5) {
        return {
            x: 16 * (width * Math.random() - (width / 2)) / 9,
            y: width * Math.random() - (width / 2)
        };
    }

    function getPosition(characterID: string, width: number = 5) {
        const charID: number = Number.parseInt(characterID);
        if (Number.isNaN(charID)) {
            return randomPosition(width);
        }

        return {
            x: Math.max(width / 6, width * (charID % 139457 / 139457)) - (width / 2),
            y: Math.max(width / 6, width * (charID % 95173 / 95173)) - (width / 2)
        };
    }

    export const RealtimeNetworkVue = Vue.extend({
        props: {

        },

        data: function() {
            return {
                connection: null as sR.HubConnection | null,
                socketState: "" as string,
                lastUpdate: null as Date | null,

                context: null as HTMLElement | null,
                graph: null as Graph | null,
                layout: null as FA2Layout | ForceSupvisor | null,
                renderer: null as Sigma | null,

                filter: "" as string,
                hovered: null as string | null,
                neighbors: null as Set<string> | null,
                graphWidth: 5 as number,
                layoutRunning: false as boolean,

                renderCount: 0 as number,
                rendersPerSecond: 0 as number,
                lastRenderCountUpdate: new Date(),

                auto: true as boolean,

                network: new RealtimeNetwork() as RealtimeNetwork,
                selected: null as RealtimeNetworkPlayer | null,
                allc: new Set() as Set<string>,
                worldID: null as number | null,

                alertData: [] as RealtimeNetwork[],

                settings: {
                    preferedLayout: "atlas" as "force" | "atlas"
                },
            }
        },

        created: function(): void {
            this.connection = new sR.HubConnectionBuilder()
                .withUrl("/ws/realtime-network")
                .withAutomaticReconnect([5000, 10000, 20000, 20000])
                .build();

            this.connection.on("UpdateNetwork", (data: any) => {
                if (this.auto == true) {
                    console.log(data);
                    this.lastUpdate = new Date();
                    this.updateGraph(data);
                }
            });

            this.connection.start().then(() => {
                this.socketState = "opened";
                console.log(`connected`);
                this.subscribe();
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

        mounted: function(): void {
            document.title = `Honu / Realtime Network`;

            const parts: string[] = location.pathname.split("/").slice(1);
            if (parts.length != 2) {
                console.error(`Bad URL format: ${parts.join('/')}`);
            } else {
                const worldID: number = Number.parseInt(parts[1]);
                if (worldID != NaN) {
                    this.worldID = worldID;
                    document.title = `Honu / Realtime Network / ${WorldUtil.getWorldID(this.worldID)}`;
                    this.subscribe();
                } else {
                    console.error(`Failed to parse ${parts[1]} to a valid int, got NaN`);
                }
            }

            this.$nextTick(() => {
                this.context = document.getElementById("graph");
                this.createGraph();
            });

            // updates per second updater
            setInterval(() => {
                const diffMs = (new Date()).getTime() - this.lastRenderCountUpdate.getTime();
                this.rendersPerSecond = this.renderCount / diffMs * 1000;
                this.renderCount = 0;
                this.lastRenderCountUpdate = new Date();
            }, 1000 * 1);
        },

        methods: {
            subscribe: function(): void {
                if (this.worldID == null) { return console.warn(`cannot connect to socket: worldID is null`); }
                if (this.connection == null) { return console.warn(`cannot connect to socket: connection is null`); }
                if (this.connection.state != sR.HubConnectionState.Connected) { return console.warn(`cannot connect to socket: state is no 'Connected': ${this.connection.state}`); }

                this.connection.invoke("Initalize", this.worldID).then(() => {
                    console.log(`RealtimeNetwork> subscribed`);
                }).catch((err: any) => {
                    console.error(`RealtimeNetwork> Error subscribing to realtime network for ${this.worldID}: ${err}`);
                });
            },

            startLayout: function(): void {
                if (this.layout != null) {
                    console.log(`RealtimeNetwork> starting layout`);
                    this.layout.start();
                    this.layoutRunning = true;
                } else {
                    console.log(`RealtimeNetwork> no layout to start`);
                }
            },

            endLayout: function(): void {
                if (this.layout != null) {
                    console.log(`RealtimeNetwork> stopping layout`);
                    this.layout.stop();
                    this.layoutRunning = false;
                } else {
                    console.log(`RealtimeNetwork> no layout to stop`);
                }
            },

            playbackHour: async function(): Promise<void> {
                if (this.worldID == null) {
                    return;
                }

                this.network = new RealtimeNetwork();
                this.resetGraph();
                this.auto = false;
                this.setupLayout();

                const now: number = (new Date()).getTime();

                const duration: number = 1000 * 60 * 60 * 1;
                const start: Date = new Date(now - duration);
                const interval: number = 1000 * 60 * 3;

                for (let i = 0; i < duration; i += interval) {
                    const s: number = start.getTime() + i;
                    const periodStart: Date = new Date(start.getTime() + i);

                    const e: number = periodStart.getTime() + interval;
                    const periodEnd: Date = new Date(periodStart.getTime() + interval);

                    console.log(i, s, e);

                    const r = await RealtimeNetworkApi.getByWorldID(this.worldID, periodStart, periodEnd, null);
                    if (r.state == "loaded") {
                        this.alertData.push(r.data);
                    }
                }

                this.setupLayout();

                const intervalID = setInterval(() => {
                    const network: RealtimeNetwork | undefined = this.alertData.shift();
                    if (network == undefined) {
                        clearTimeout(intervalID);
                    } else {
                        console.log(`RealtimeNetwork> ${this.alertData.length} left`);
                        this.updateGraph(network);
                    }
                }, 10 * 1000);
            },

            updateGraph: function(data: RealtimeNetwork): void {
                this.network = data;

                // Find all character IDs that are in the network, either as a player, or only as an interaction
                this.allc.clear();
                for (const player of this.network.players) {
                    this.allc.add(player.characterID);

                    for (const interaction of player.interactions) {
                        this.allc.add(interaction.otherID);
                    }
                }

                console.time("update nodes");
                this.updateNodes();
                console.timeEnd("update nodes");

                console.time("update edges");
                this.updateEdges();
                console.timeEnd("update edges");

                if (this.renderer != null) {
                    this.renderer.refresh();
                }
            },

            updateNetworkData: async function(): Promise<void> {
                console.log(`Updating data`);

                if (this.worldID == null) { return console.warn(`cannot parse data, worldID is null`); }

                const r = await RealtimeNetworkApi.getByWorldID(this.worldID);
                if (r.state != "loaded") {
                    return console.warn(`cannot parse data: response was ${r.state}, not 'loaded'`);
                }

                this.updateGraph(r.data);
            },

            updateNodes: function(): void {
                if (this.graph == null) {
                    return console.warn(`Cannot update nodes: graph is null`);
                }

                // Remove characters that were present in the last network, but are not longer in the network
                this.graph.forEachNode((charID: string, _) => {
                    if (this.allc.has(charID) == false) {
                        this.graph!.dropNode(charID);
                    }
                });

                // Find any node that is in the network, but not in the graph
                for (const player of this.network.players) {
                    this.graph.updateNode(player.characterID, (attr: any) => {
                        const totalStrength: number = player.interactions.reduce((acc, iter) => acc += iter.strength, 0);

                        return {
                            x: attr.x || getPosition(player.characterID, this.graphWidth).x,
                            y: attr.y || getPosition(player.characterID, this.graphWidth).y,
                            label: player.display,
                            color: attr.color || ColorUtil.getFactionColor(player.factionID ?? 0),
                            size: Math.min(20, 5 + 5 * totalStrength)
                        }
                    });

                    for (const inter of player.interactions) {
                        if (this.graph.hasNode(inter.otherID) == false) {
                            this.graph.addNode(inter.otherID, {
                                ...getPosition(inter.otherID, this.graphWidth),
                                label: inter.otherName,
                                color: ColorUtil.getFactionColor(inter.factionID ?? 0),
                                size: 5
                            });
                        }
                    }
                }
            },

            updateEdges: function(): void {
                if (this.graph == null) {
                    return console.warn(`Cannot update edges: graph is null`);
                }

                // Create a lookup table for removing edges. Without this table, it's a O(n^2) operation,
                //      as every edge will iterate thru the entire player list
                const map: Map<string, RealtimeNetworkPlayer> = new Map();
                for (const p of this.network.players) {
                    map.set(p.characterID, p);
                }

                // Find edges between nodes that are no longer present in the network, and remove them from the graph                
                this.graph.forEachEdge((key: string, attr: any) => {
                    const [charID, otherID] = key.split(".");

                    const player: RealtimeNetworkPlayer | undefined = map.get(charID);
                    if (player != null) {
                        if (player.interactions.find(iter => iter.otherID == otherID) == undefined) {
                            this.graph!.dropEdge(key);
                        }
                    }
                });

                // Yes, the min is above the max, this will ensure the value will be changed at least once
                let min: number = 1;
                let max: number = 0;

                // Run thru each interaction in the network. If it exists, update the strength of the connection, else if it doesn't exist, create it
                for (const player of this.network.players) {
                    for (const interaction of player.interactions) {
                        // Key is supplied instead of the auto-default so we can remove the edge later if needed
                        this.graph.updateEdgeWithKey(`${player.characterID}.${interaction.otherID}`, player.characterID, interaction.otherID, (attr) => {
                            if (interaction.strength > max) { max = interaction.strength; }
                            if (interaction.strength < min) { min = interaction.strength; }

                            // Black edges = weaker connection, white edges = strong connection
                            const value: number = Math.min(max, Math.max(min, interaction.strength));
                            // Do a linear interpolation based on the weakest and strongest connections seen so far
                            const c = ColorUtil.colorGradient(value, { red: 0, green: 0, blue: 0 }, { red: 255, green: 255, blue: 255 });

                            let newAttr = {
                                ...attr,
                                weight: (0.3 + (3 * interaction.strength)) / 3,
                                color: ColorUtil.rgbToString(c)
                            };

                            return newAttr;
                        });
                    }
                }
            },

            createGraph: function(): void {
                if (this.context == null) {
                    console.error(`Cannot create graph, context is null`);
                    return;
                }

                if (this.layout != null) {
                    this.endLayout();
                    this.layout.kill();
                    this.layout = null;
                }

                // @ts-ignore: It's not actually abstract and it works
                this.graph = new Graph();
                if (this.graph == null) { throw `wtf`; }

                this.renderer = new Sigma(this.graph!, this.context, {
                    allowInvalidContainer: true
                });

                this.renderer.on("afterRender", () => {
                    ++this.renderCount;
                });

                this.renderer.on("clickNode", (node) => {
                    // If the selected node is the same as the node that was selected, deselect everything
                    if (this.hovered == node.node) {
                        this.neighbors = null;
                        this.hovered = null;
                        this.selected = null;
                    } else {
                        this.neighbors = new Set(this.graph!.neighbors(node.node));
                        this.hovered = node.node;
                        this.selected = this.network.players.find(iter => iter.characterID == node.node) || null;
                    }

                    if (this.renderer) {
                        this.renderer.refresh();
                    }
                });

                // Applied before every render on every node
                this.renderer.setSetting("nodeReducer", (node: string, data) => {
                    const res = { ...data };

                    if (this.hovered == node) {
                        res.labelColor = "#fff";
                    }

                    // If the hovered node isn't this node, and there are neighbors we want to highlight, hide this node
                    //  OR, if there is a filter applied, and this node doesn't match that filter, hide this node
                    if (this.hovered != node && this.neighbors != null && this.neighbors.has(node) == false
                        || (this.filter.length > 0 && data.label.toLowerCase().indexOf(this.filter.toLowerCase()) == -1)) {

                        res.label = "";
                        res.color = "#222222";
                    }

                    return res;
                });

                // Applied before every render on every edge
                this.renderer.setSetting("edgeReducer", (edge: string, data) => {
                    const res = { ...data };

                    // If a node is selected, and this edge isn't attached to that node, hide this edge
                    if (this.hovered != null && this.graph!.hasExtremity(edge, this.hovered) == false) {
                        res.hidden = true;
                    }

                    return res;
                });

                this.renderer.setSetting("labelColor", { color: "#ffffff" });

                this.setupLayout();
                this.startLayout();
            },

            setupLayout: function(): void {
                if (this.graph == null) {
                    return;
                }

                let startAgain: boolean = false;

                if (this.layout != null) {
                    startAgain = this.layout.isRunning();

                    this.endLayout();

                    this.layout.kill();
                    this.layout = null;
                }

                if (this.settings.preferedLayout == "atlas") {
                    this.layout = new FA2Layout(this.graph, {
                        settings: {
                            gravity: 1,
                            adjustSizes: true,
                            barnesHutOptimize: true,
                            outboundAttractionDistribution: true,
                            linLogMode: true,
                            slowDown: 1
                        },
                    });
                } else if (this.settings.preferedLayout == "force") {
                    this.layout = new ForceSupvisor(this.graph, {
                        settings: {
                            attraction: 0.001,
                            maxMove: 1,
                            inertia: 0.01,
                        }
                    });
                } else {
                    console.error(`Unknown prefered layout: ${this.settings.preferedLayout}`);
                }

                if (startAgain == true) {
                    this.startLayout();
                }
            },

            resetGraph: function(): void {
                if (this.graph == null) {
                    return console.warn(`Cannot reset graph: graph is null`);
                }

                this.graph.clear();
                this.graph.clearEdges();
                this.updateNetworkData();
            }
        },

        watch: {
            "settings.preferedLayout": function(): void {
                this.setupLayout();
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover,
            HonuMenu, MenuSep, MenuDropdown,
            ToggleButton,
            Busy
        }

    });
    export default RealtimeNetworkVue;
</script>