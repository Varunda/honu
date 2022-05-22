<template>
    <div class="d-flex flex-column" style="height: 100vh;">
        <div class="flex-grow-0">
            <honu-menu>
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    Realtime Network
                </li>
            </honu-menu>
        </div>

        <div class="flex-grow-1">
            <div>
                <button class="btn btn-primary" @click="parseData">
                    get
                </button>

                <button class="btn btn-success" @click="startLayout">
                    start
                </button>

                <button class="btn btn-warning" @click="endLayout">
                    stop
                </button>

                <toggle-button v-model="auto">
                    auto
                </toggle-button>
            </div>

            <div id="graph" class="w-100 h-100" style="display: block;"></div>
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

    import { RealtimeNetwork, RealtimeNetworkApi, RealtimeNetworkPlayer } from "api/RealtimeNetworkApi";

    import * as sR from "signalR";

    import ColorUtil from "util/Color";
    import CharacterUtils from "util/Character";

    export const RealtimeNetworkVue = Vue.extend({
        props: {

        },

        data: function() {
            return {
                context: null as HTMLElement | null,
                graph: null as Graph | null,
                layout: null as FA2Layout | null,
                renderer: null as Sigma | null,

                hovered: null as string | null,
                neighbors: null as Set<string> | null,
                graphWidth: 5 as number,
                layoutRunning: false as boolean,

                auto: true as boolean,

                intervalID: 0 as number,

                network: new RealtimeNetwork() as RealtimeNetwork,

                worldID: null as number | null,

                settings: {
                    sameWorld: true as boolean,
                    orbit: false as boolean,
                    strongGravity: true as boolean
                },
            }
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
                } else {
                    console.error(`Failed to parse ${parts[1]} to a valid int, got NaN`);
                }
            }

            this.$nextTick(() => {
                this.context = document.getElementById("graph");
                this.createGraph();
                this.parseData();
            });

            this.intervalID = setInterval(async () => {
                if (this.auto == true) {
                    await this.parseData();
                }
            }, 1000 * 10) as unknown as number;
        },

        methods: {
            startLayout: function(): void {
                if (this.layout != null) {
                    console.log(`FriendNetwork> starting layout`);
                    this.layout.start();
                    this.layoutRunning = true;
                } else {
                    console.log(`FriendNetwork> no layout to start`);
                }
            },

            endLayout: function(): void {
                if (this.layout != null) {
                    console.log(`FriendNetwork> stopping layout`);
                    this.layout.stop();
                    this.layoutRunning = false;
                } else {
                    console.log(`FriendNetwork> no layout to stop`);
                }
            },

            parseData: async function(): Promise<void> {
                console.log(`Updating data`);

                if (this.worldID == null) { return console.warn(`cannot parse data, worldID is null`); }
                if (this.graph == null) { return console.warn(`cannot parse data: graph is null`); }
                if (this.renderer == null) { return console.warn(`cannot parse data: renderer is null`); }

                const r = await RealtimeNetworkApi.getByWorldID(this.worldID);
                if (r.state != "loaded") {
                    return console.warn(`cannot parse data: response was ${r.state}, not 'loaded'`);
                }

                this.network = r.data;

                console.time("update nodes");
                this.updateNodes();
                console.timeEnd("update nodes");

                console.time("update edges");
                this.updateEdges();
                console.timeEnd("update edges");

                this.renderer.refresh();
            },

            updateNodes: function(): void {
                if (this.graph == null) {
                    return console.warn(`Cannot update nodes: graph is null`);
                }

                // Remove nodes that are no longer in the network
                this.graph.forEachNode((charID: string, _) => {
                    if (this.network.players.findIndex(iter => iter.characterID == charID) == -1) {
                        //console.log(`dropping ${charID} as its not present in players`);
                        this.graph!.dropNode(charID);
                    }
                });

                // Find any node that is in the network, but not in the graph
                for (const player of this.network.players) {
                    if (this.graph.hasNode(player.characterID) == false) {
                        this.graph.addNode(player.characterID, {
                            x: Math.random(),
                            y: Math.random(),
                            label: player.display,
                            color: ColorUtil.getFactionColor(player.factionID ?? 0),
                            size: player.interactions.length
                        });
                    }
                }
            },

            updateEdges: function(): void {
                if (this.graph == null) {
                    return console.warn(`Cannot update edges: graph is null`);
                }

                // Find edges between nodes that are no longer present in the network, and remove them from the graph                
                this.graph.forEachEdge((key: string, _) => {
                    const [charID, otherID] = key.split(".");

                    const player: RealtimeNetworkPlayer | undefined = this.network.players.find(iter => iter.characterID == charID);
                    if (player != null) {
                        if (player.interactions.find(iter => iter.otherID == otherID) == undefined) {
                            this.graph!.dropEdge(key);
                        }
                    }
                });

                // Run thru each interaction in the network. If it exists, update the strength of the connection, else if it doesn't exist, create it
                for (const player of this.network.players) {
                    for (const interaction of player.interactions) {
                        if (this.graph.hasNode(interaction.otherID) == false) {
                            this.graph.addNode(interaction.otherID, {
                                x: Math.random(),
                                y: Math.random(),
                                label: interaction.otherID,
                            });
                        }

                        this.graph.updateEdge(player.characterID, interaction.otherID, (attr) => {
                            return {
                                ...attr,
                                weight: 5 * interaction.strength,
                                color: "#444444"
                            }
                        });
                    }
                }
            },

            createGraph: function(): void {
                if (this.context == null) {
                    console.error(`Cannot create graph, context is null`);
                    return;
                }

                // @ts-ignore: It's not actually abstract and it works
                this.graph = new Graph();
                if (this.graph == null) { throw `wtf`; }

                this.renderer = new Sigma(this.graph!, this.context, {
                    allowInvalidContainer: true
                });

                this.renderer.on("clickNode", (node) => {
                    console.log(node);
                    if (this.hovered == node.node) {
                        this.neighbors = null;
                        this.hovered = null;
                        //this.selected = null;
                    } else {
                        this.neighbors = new Set(this.graph!.neighbors(node.node));
                        this.hovered = node.node;
                    }

                    if (this.renderer) {
                        this.renderer.refresh();
                    }
                });

                this.renderer.setSetting("nodeReducer", (node: string, data) => {
                    const res = { ...data };

                    if (this.hovered == node) {
                        res.labelColor = "#000000";
                    }

                    if (this.hovered != node && this.neighbors != null && this.neighbors.has(node) == false) {
                        res.label = "";
                        res.color = "#222222";
                    }

                    return res;
                });

                this.renderer.setSetting("edgeReducer", (edge: string, data) => {
                    const res = { ...data };

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

                this.layout = new FA2Layout(this.graph, {
                    settings: {
                        gravity: 1,
                        adjustSizes: true,
                        barnesHutOptimize: true,
                        //outboundAttractionDistribution: true,
                    },
                });

                if (startAgain == true) {
                    this.startLayout();
                }
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