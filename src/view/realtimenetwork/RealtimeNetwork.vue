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
                </honu-menu>
            </div>

            <div>
                <toggle-button v-model="auto" class="d-block">
                    Toggle auto-update
                </toggle-button>

                <input v-model="filter" class="form-input" placeholder="Filter" />
            </div>
        </div>

        <div id="graph" class="w-100 h-100" style="display: block; z-index: -100;"></div>
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
                context: null as HTMLElement | null,
                graph: null as Graph | null,
                layout: null as FA2Layout | null,
                renderer: null as Sigma | null,

                hovered: null as string | null,
                neighbors: null as Set<string> | null,
                graphWidth: 5 as number,
                layoutRunning: false as boolean,

                allc: new Set() as Set<string>,

                filter: "" as string,

                auto: true as boolean,

                intervalID: 0 as number,

                network: new RealtimeNetwork() as RealtimeNetwork,

                worldID: null as number | null,

                settings: {

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
            }, 1000 * 3) as unknown as number;
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

                this.renderer.refresh();
            },

            updateNodes: function(): void {
                if (this.graph == null) {
                    return console.warn(`Cannot update nodes: graph is null`);
                }

                // Remove nodes that are no longer in the network
                this.graph.forEachNode((charID: string, _) => {
                    if (this.allc.has(charID) == false) {
                        //console.log(`dropping ${charID} as its not present in players`);
                        this.graph!.dropNode(charID);
                    }
                });

                // Find any node that is in the network, but not in the graph
                for (const player of this.network.players) {
                    this.graph.updateNode(player.characterID, (attr: any) => {
                        return {
                            x: attr.x || getPosition(player.characterID, this.graphWidth).x,
                            y: attr.y || getPosition(player.characterID, this.graphWidth).y,
                            label: player.display,
                            color: attr.color || ColorUtil.getFactionColor(player.factionID ?? 0),
                            size: 5 + player.interactions.length
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

                const map: Map<string, RealtimeNetworkPlayer> = new Map();
                for (const p of this.network.players) {
                    map.set(p.characterID, p);
                }

                // Find edges between nodes that are no longer present in the network, and remove them from the graph                
                this.graph.forEachEdge((key: string, _) => {
                    const [charID, otherID] = key.split(".");

                    const player: RealtimeNetworkPlayer | undefined = map.get(charID);
                    if (player != null) {
                        if (player.interactions.find(iter => iter.otherID == otherID) == undefined) {
                            this.graph!.dropEdge(key);
                        }
                    }
                });

                // Run thru each interaction in the network. If it exists, update the strength of the connection, else if it doesn't exist, create it
                for (const player of this.network.players) {
                    for (const interaction of player.interactions) {
                        this.graph.updateEdge(player.characterID, interaction.otherID, (attr) => {
                            return {
                                ...attr,
                                weight: (0.3 + (3 * interaction.strength)) / 3,
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

                    if (this.hovered != node && this.neighbors != null && this.neighbors.has(node) == false
                        || (this.filter.length > 0 && data.label.indexOf(this.filter) == -1)) {

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
                        outboundAttractionDistribution: true,
                        linLogMode: true,
                        slowDown: 1
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