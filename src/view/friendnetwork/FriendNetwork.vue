<template>
    <div class="d-flex flex-column" style="height: 100vh;">
        <honu-menu class="flex-grow-0">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/friendnetwork">Friend network</a>
            </li>
        </honu-menu>

        <div v-if="loading == true">
            <div class="progress mt-3" style="height: 3rem;">
                <div class="progress-bar" :style="{ width: progressWidth }" style="height: 3rem;">
                    <span style="position: absolute; left: 50%; transform: translateX(-50%); font-size: 2.5rem;">
                        <busy class="honu-busy honu-busy-sm"></busy>
                        Loading {{total - todo}}/{{total}} friend lists...
                    </span>
                </div>
            </div>
        </div>

        <div class="flex-grow-1">
            <div id="graph" class="w-100 h-100" style="display: block;"></div>

            <div v-if="selected != null" class="m-3 px-2 py-1" style="display: inline; position: absolute; right: 0; top: 50%; background-color: #222;">
                <h3>
                    <span v-if="selected.outfitID != null">
                        [{{selected.outfitTag}}]
                    </span>
                    {{selected.name}}
                </h3>

                <a :href="'/c/' + selected.id" class="btn btn-primary btn-link">View character</a>

                <a @click="makeGraph(selected.id)" href="#" class="btn btn-primary btn-link">
                    View network
                </a>

                <button class="btn btn-secondary" @click="deselectHovered">
                    Deselect
                </button>
            </div>

            <div class="m-3 px-2 pl-1" style="display: inline; position: absolute; left: 0; top: 50%; background-color: #222; transform: translateY(-50%)">
                <div class="btn-group btn-group-vertical w-100">
                    <button class="btn btn-success" @click="startLayout">
                        Start layout
                    </button>

                    <button class="btn btn-warning" @click="endLayout">
                        Stop layout
                    </button>
                </div>

                <div class="mb-2">
                    <label class="mb-0">
                        Search
                    </label>
                    <input class="form-control" placeholder="search..." v-model="search"></input>
                </div>

                <hr class="wt-header" />

                <toggle-button v-model="settings.orbit" class="w-100 mb-2">
                    Use "oubound attraction"
                </toggle-button>

                <toggle-button v-model="settings.strongGravity" class="w-100 mb-2">
                    Use "strong gravity"
                </toggle-button>

                <toggle-button v-model="settings.linLogMode" class="w-100 mb-2">
                    Use "lin log mode"
                </toggle-button>

                <div class="mb-2">
                    <label class="mb-0">
                        Minimum friends shared
                        <info-hover text="How many friends in common must be shared to be included"></info-hover>
                    </label>

                    <input class="form-control" v-model.number="settings.minConnections" />
                </div>

                <div class="mb-2">
                    <label class="mb-0">
                        Minimum friends
                        <info-hover text="How many friends a character must have to be included"></info-hover>
                    </label>

                    <input class="form-control" v-model.number="settings.minFriends" />
                </div>

                <div class="mb-2">
                    <label class="mb-0">
                        Max friends
                        <info-hover text="How many friends a character can have before being excluded"></info-hover>
                    </label>

                    <input class="form-control" v-model.number="settings.maxFriends" />
                </div>

                <toggle-button v-model="settings.rootOnly" class="w-100">
                    show close friends only
                </toggle-button>

            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuHomepage, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ToggleButton from "components/ToggleButton";
    import Busy from "components/Busy.vue";
    import InfoHover from "components/InfoHover.vue";

    import Graph from "graphology";
    import FA2Layout from "node_modules/graphology-layout-forceatlas2/worker";
    import Sigma from "sigma";
    import { NodeDisplayData, PartialButFor } from "sigma/types";
    import { Settings } from "sigma/settings";
    // not actually exported
    //import { drawLabel } from "sigma/rendering/canvas/label";

    import { Loading, Loadable } from "Loading";

    import { FlatExpandedCharacterFriend, CharacterFriendApi } from "api/CharacterFriendApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";

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
            x: width * (charID % 139457 / 139457) - (width / 2),
            y: width * (charID % 95173 / 95173) - (width / 2)
        };
    }

    type FriendNode = {
        characterID: string;
        depth: number;
    }

    // HACK: if the renderer is part of the Vue object, then all it's reactive stuff makes the graph horribly slow
    //      so, the renderer is pulled out
    let RENDERER: any = null;

    export const FriendNetwork = Vue.extend({
        props: {

        },

        data: function() {
            return {
                rootCharacterIDs: [] as string[],

                context: null as HTMLElement | null,
                graph: null as Graph | null,
                hovered: null as string | null,
                neighbors: null as Set<string> | null,
                layout: null as FA2Layout | null,
                graphWidth: 5 as number,
                loaded: new Set() as Set<string>,
                queue: [] as FriendNode[],

                layoutRunning: false as boolean,

                root: null as PsCharacter | null,
                selected: null as PsCharacter | null,

                loading: false as boolean,
                todo: 0 as number,
                total: 0 as number,

                friendMap: new Map() as Map<string, FlatExpandedCharacterFriend[]>,
                maxFriends: 0 as number,
                characterMap: new Map as Map<string, FlatExpandedCharacterFriend>, // <character ID, entry>

                friendConnections: new Map as Map<string, number>, // how many inter-connections a character has. <character ID, count>

                search: "" as string,

                settings: {
                    linLogMode: true as boolean,
                    sameWorld: true as boolean,
                    orbit: false as boolean,
                    strongGravity: true as boolean,
                    minConnections: 2 as number,
                    minFriends: 10 as number,
                    maxFriends: 5000 as number,
                    rootOnly: false as boolean
                },

                steps: {
                    friends: false as boolean,
                    layout: false as boolean,
                    render: false as boolean
                }
            }
        },

        mounted: function(): void {
            document.title = `Honu / Friend Network`;

            const parts: string[] = location.pathname.split("/").slice(1);
            if (parts.length != 2) {
                console.error(`Bad URL format: ${parts.join('/')}`);
            }

            this.$nextTick(() => {
                this.rootCharacterIDs = parts[1].split(",");
                console.log(`roots: ${this.rootCharacterIDs.join(", ")}`);
                this.context = document.getElementById("graph");
                this.makeGraph(this.rootCharacterIDs);
            });
        },

        // todo: 
        //      search would be SLICK

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

            processQueue: async function(depth: number): Promise<FlatExpandedCharacterFriend[]> {
                if (this.graph == null) {
                    throw `cannot process queue while graph is null`;
                }

                const friends: FlatExpandedCharacterFriend[] = [];
                console.log(`FriendNetwork> have ${this.queue.length} entries in queue`);

                let i: number = 0;
                let length: number = this.queue.length;

                while (this.queue.length > 0) {
                    ++i;
                    --this.todo;
                    const iter: FriendNode | undefined = this.queue.shift();
                    if (iter == undefined) {
                        continue;
                    }

                    let l: Loading<FlatExpandedCharacterFriend[]>;
                    if (this.friendMap.has(iter.characterID) == true) {
                        l = Loadable.loaded(this.friendMap.get(iter.characterID)!);
                    } else {
                        l = await CharacterFriendApi.getByCharacterID(iter.characterID, true);

                        if (l.state == "loaded") {
                            this.friendMap.set(iter.characterID, l.data);

                            for (const entry of l.data) {
                                this.characterMap.set(entry.characterID, entry);
                            }
                        }
                    }

                    if (l.state != "loaded") {
                        continue;
                    }

                    if (this.maxFriends <= l.data.length) {
                        this.maxFriends = l.data.length;
                    }

                    // exclude people with less than 10 friends and more than 5000
                    if (l.data.length < this.settings.minFriends || l.data.length > this.settings.maxFriends) {
                        console.info(`FriendNetwork> character ${iter.characterID} has ${l.data.length} friends, which is outside the bounds of (${this.settings.minFriends}, ${this.settings.maxFriends})`);
                        continue;
                    }

                    console.log(`FriendNetwork> Loaded ${i}/${length} Loaded ${l.data.length} friends for ${iter.characterID}`);

                    for (const f of l.data) {
                        // if the same world setting is turned on, don't include it in the graph
                        if (this.settings.sameWorld == true && this.root != null && f.friendWorldID != this.root.worldID) {
                            continue;
                        }

                        if (this.loaded.has(f.friendID) == false) {
                            this.graph.addNode(f.friendID, {
                                ...getPosition(f.friendID, this.graphWidth),
                                label: CharacterUtils.getDisplay({ name: f.friendName ?? `<missing ${f.friendID}>`, outfitTag: f.friendOutfitTag }),
                                size: 3,
                                color: ColorUtil.getFactionColor(f.friendFactionID ?? 0)
                            });
                        }

                        // increase the number of connections had by one
                        this.friendConnections.set(f.friendID, (this.friendConnections.get(f.friendID) ?? 0) + 1);

                        if (this.graph.hasEdge(iter.characterID, f.friendID) == true) {
                            //console.log(`edge between ${iter.characterID} and ${f.friendID} already exists, skipping`);
                            continue;
                        }

                        this.graph.addEdge(iter.characterID, f.friendID, {
                            color: "#444444"
                        });

                        friends.push(f);
                        this.loaded.add(f.friendID);

                        //console.log(`FriendNetwork> added ${f.friendName}/${f.characterID}`);
                    }
                }

                friends.forEach((iter) => {
                    this.queue.push({ characterID: iter.friendID, depth: depth });
                });

                return friends;
            },

            makeGraph: async function(rootCharacterIDs: string[]): Promise<void> {
                this.loading = true;
                this.total = 0;
                this.todo = 0;
                this.loaded = new Set<string>();
                this.queue = [];
                this.hovered = null;
                this.selected = null;
                this.maxFriends = 0;
                this.friendConnections = new Map();

                if (this.context == null) {
                    console.warn(`FriendNetwork> Cannot call makeGraph: context is null`);
                    return;
                }

                if (this.graph != null) {
                    this.graph.clear();
                    this.graph.clearEdges();
                }

                // @ts-ignore: It's not actually abstract and it works
                this.graph = new Graph();
                if (this.graph == null) { throw `wtf`; }

                for (const rootCharacterID of rootCharacterIDs) {
                    this.queue.push({ characterID: rootCharacterID, depth: 0 });
                    this.loaded.add(rootCharacterID);

                    const char: Loading<PsCharacter> = await CharacterApi.getByID(rootCharacterID);

                    this.graph.addNode(rootCharacterID, {
                        x: 0, y: 0,
                        label: (char.state == "loaded") ? CharacterUtils.getDisplay(char.data) : "",
                        size: 10,
                        color: "gold"
                    });
                }
                this.total = this.queue.length;
                this.todo = this.queue.length + 1;

                await this.processQueue(1);
                this.total = this.queue.length;
                this.todo = this.queue.length + 1;

                await this.processQueue(2);

                for (const entry of Array.from(this.friendConnections.entries())) {
                    const charID: string = entry[0];
                    const friendCount: number = entry[1];

                    //console.log(`FriendNetwork> ${charID} has ${friendCount}`);

                    if (this.settings.minConnections > friendCount) {
                        //console.log(`FriendNetwork> DROPPING ${charID}; ${friendCount} < ${this.settings.minConnections}`);
                        this.graph.dropNode(charID);
                    }
                }

                // only show friends connected to one of the root nodes
                if (this.settings.rootOnly) {
                    const ids: Set<string> = new Set();

                    for (const rootCharacterID of this.rootCharacterIDs) {
                        const friends: FlatExpandedCharacterFriend[] = this.friendMap.get(rootCharacterID)!;
                        for (const f of friends.map(iter => iter.friendID)) {
                            ids.add(f);
                        }
                    }

                    for (const nodeID of this.graph.nodes()) {
                        if (ids.has(nodeID) == false && this.rootCharacterIDs.indexOf(nodeID) == -1) {
                            this.graph.dropNode(nodeID);
                        }
                    }
                }

                this.graph.forEachNode((node, attr) => {
                    if (rootCharacterIDs.indexOf(node) > -1) {
                        attr.size = 10;
                    } else if (this.friendMap.has(node) == true) {
                        const friends: FlatExpandedCharacterFriend[] = this.friendMap.get(node)!;

                        attr.size = 5 + 10 * (friends.length / this.maxFriends);
                    }
                });

                this.steps.friends = true;

                const renderer = new Sigma(this.graph!, this.context, {
                    allowInvalidContainer: true
                });
                RENDERER = renderer;

                renderer.setSetting("labelRenderedSizeThreshold", 3);
                renderer.setSetting("labelColor", { color: "#fff" });
                //
                // TLDR: this makes you have black text when you hover a node, despite the "labelColor" (above) set to white
                //
                // problem. you (the program) can only set one color for node text. honu uses a black background,
                //      so a white text is appropriate. however, when a user hovers a node, the back drop is ALSO white.
                //      this is hard coded into sigma, and cannot be changed.
                // this means that basically only black text is appropriate for node labels, as you want the label nodes to appear
                //      when a user is hovering them. but honu uses a black background, so black text is not good!
                //
                // this method overrides the default method used to render a hovered node,
                //      and instead of using the settings "labelColor" text, black text is used instead
                //
                // the following code is copied directly from sigma.js, which can be found on GitHub:
                //      https://github.com/jacomyal/sigma.js
                //
                // here is the license for sigma.js:
                //      https://github.com/jacomyal/sigma.js/blob/main/LICENSE.txt
                //
                // specifically, the following two files were used:
                //      https://github.com/jacomyal/sigma.js/blob/main/src/rendering/canvas/hover.ts
                //      https://github.com/jacomyal/sigma.js/blob/main/src/rendering/canvas/label.ts
                //
                renderer.setSetting("hoverRenderer", (
                    context: CanvasRenderingContext2D,
                    data: PartialButFor<NodeDisplayData, "x" | "y" | "size" | "label" | "color">,
                    settings: Settings,
                ): void => {
                    const size = settings.labelSize;
                    const font = settings.labelFont;
                    const weight = settings.labelWeight;

                    context.font = `${weight} ${size}px ${font}`;

                    // Then we draw the label background
                    context.fillStyle = "#FFF";
                    context.shadowOffsetX = 0;
                    context.shadowOffsetY = 0;
                    context.shadowBlur = 8;
                    context.shadowColor = "#000";

                    const PADDING = 2;

                    if (typeof data.label === "string") {
                        const textWidth = context.measureText(data.label).width;
                        const boxWidth = Math.round(textWidth + 5);
                        const boxHeight = Math.round(size + 2 * PADDING);
                        const radius = Math.max(data.size, size / 2) + PADDING;

                        const angleRadian = Math.asin(boxHeight / 2 / radius);
                        const xDeltaCoord = Math.sqrt(Math.abs(Math.pow(radius, 2) - Math.pow(boxHeight / 2, 2)));

                        context.beginPath();
                        context.moveTo(data.x + xDeltaCoord, data.y + boxHeight / 2);
                        context.lineTo(data.x + radius + boxWidth, data.y + boxHeight / 2);
                        context.lineTo(data.x + radius + boxWidth, data.y - boxHeight / 2);
                        context.lineTo(data.x + xDeltaCoord, data.y - boxHeight / 2);
                        context.arc(data.x, data.y, radius, angleRadian, -angleRadian);
                        context.closePath();
                        context.fill();
                    } else {
                        context.beginPath();
                        context.arc(data.x, data.y, data.size + PADDING, 0, Math.PI * 2);
                        context.closePath();
                        context.fill();
                    }

                    context.shadowOffsetX = 0;
                    context.shadowOffsetY = 0;
                    context.shadowBlur = 0;

                    // this is where the code from label.ts is used
                    if (!data.label) { return; }
                    /* old code
                    const color = settings.labelColor.attribute
                        ? data[settings.labelColor.attribute] || settings.labelColor.color || "#000"
                        : settings.labelColor.color;
                    */
                    const color = "#000";

                    context.fillStyle = color;
                    context.font = `${weight} ${size}px ${font}`;

                    context.fillText(data.label, data.x + data.size + 3, data.y + size / 3);
                });
                // back to honu's code

                this.steps.render = true;

                this.setupLayout();
                this.startLayout();

                renderer.on("clickNode", (node) => {
                    if (this.hovered == node.node) {
                        this.neighbors = null;
                        this.hovered = null;
                        this.selected = null;
                    } else {
                        this.neighbors = new Set(this.graph!.neighbors(node.node));
                        this.hovered = node.node;

                        CharacterApi.getByID(node.node).then((data: Loading<PsCharacter>) => {
                            if (data.state == "loaded" && this.hovered == data.data.id) {
                                this.selected = data.data;
                            }
                        });
                    }

                    renderer.refresh();
                });

                renderer.setSetting("nodeReducer", (node: string, data) => {
                    const res = { ...data };

                    if (this.hovered == node) {
                        res.labelColor = "#000000";
                    }

                    if (this.hovered != node && this.neighbors != null && this.neighbors.has(node) == false) {
                        res.label = "";
                        res.color = "#222222";
                    }

                    if (this.search.length > 0) {
                        const ex: FlatExpandedCharacterFriend | undefined = this.characterMap.get(node);
                        if (ex == undefined) {
                            console.log(`missing ${node}`);
                            return res;
                        }

                        if (ex.friendName?.toLowerCase().startsWith(this.search.toLowerCase())) {
                            res.color = "#222222";
                            res.label = "";
                        } else {
                            res.labelColor = "#000000";
                        }
                    }

                    return res;
                });

                renderer.setSetting("edgeReducer", (edge: string, data) => {
                    const res = { ...data };

                    if (this.hovered != null && this.graph!.hasExtremity(edge, this.hovered) == false) {
                        res.hidden = true;
                    }

                    return res;
                });

                //
                // this is an attempt to automatically stop a graph from doing the layout when the shape of the graph has settled.
                // it doesn't quite work, but with some tweaking it could be okay.
                // the main problem is the graph can reach a state where the node will be constantly be shifting between two
                //      different positions, make it seem like the graph is moving more than it is.
                // another way i might play around later is calculating how far each node has moved over time, 
                //      rather than how much all nodes moved
                /*
                const nodeCount: number = this.graph.nodes().length;
                const avg: number[] = [];
                const previousPos: Map<string, [number, number]> = new Map();
                let previousRenderTime: number = new Date().getTime();
                renderer.on("afterRender", () => {
                    if (this.graph == null) {
                        return;
                    }

                    let sum: number = 0;

                    for (const nodeID of this.graph.nodes()) {
                        const attr = this.graph.getNodeAttributes(nodeID);

                        if (previousPos.has(nodeID) == true) {
                            const prev: [number, number] = previousPos.get(nodeID)!;
                            const diff = (attr.x - prev[0]) + (attr.y - prev[1]);
                            sum += diff;
                            //console.log(`node ${nodeID} moved ${diff} units`);
                        }

                        previousPos.set(nodeID, [attr.x, attr.y]);
                    }

                    avg.push(Math.abs(sum));
                    if (avg.length >= 50) {
                        avg.shift();
                    }

                    const aa: number = avg.reduce((acc, iter) => acc += iter, 0);
                    const renderTimeMs: number = new Date().getTime() - previousRenderTime;
                    previousRenderTime = new Date().getTime();

                    console.log(`graph shifted ${sum} units / ${aa} units avg :: [render=${renderTimeMs}ms] [sum/render=${sum / renderTimeMs}]`);

                    if (aa <= (nodeCount * 4)) {
                        console.warn(`automatically stopping!`);
                    }
                });
                */

                console.log(`made graph`);

                this.loading = false;
            },

            deselectHovered: function(): void {
                this.neighbors = null;
                this.hovered = null;
                this.selected = null;

                if (RENDERER != null) {
                    RENDERER.refresh();
                }
            },

            setupLayout: function(): void {
                if (this.graph == null) {
                    return;
                }

                // if FA2 is already running, make sure to start the layout again
                let startAgain: boolean = false;
                if (this.layout != null) {
                    startAgain = this.layout.isRunning();

                    this.endLayout();

                    this.layout.kill();
                    this.layout = null;
                }

                this.layout = new FA2Layout(this.graph, {
                    settings: {
                        linLogMode: this.settings.linLogMode,
                        gravity: 1,
                        //scalingRatio: 5,
                        //edgeWeightInfluence: 0,
                        adjustSizes: true,
                        //slowDown: 1,
                        // basically always want this on, as it allows for smaller than O(n^2) complexity
                        barnesHutOptimize: true,
                        strongGravityMode: this.settings.strongGravity,
                        outboundAttractionDistribution: this.settings.orbit,
                        
                    },
                });

                if (startAgain == true) {
                    this.startLayout();
                }
            }
        },

        computed: {
            progressWidth: function(): string {
                return `${((this.total - this.todo) / Math.max(1, this.total)) * 100}%`;
            }
        },

        watch: {
            "settings.orbit": function() { this.setupLayout(); },
            "settings.strongGravity": function() { this.setupLayout(); },
            "settings.minConnections": function() { this.makeGraph(this.rootCharacterIDs); },
            "settings.minFriends": function() { this.makeGraph(this.rootCharacterIDs); },
            "settings.maxFriends": function() { this.makeGraph(this.rootCharacterIDs); },
            "settings.rootOnly": function() { this.makeGraph(this.rootCharacterIDs); },

            "search": function() {
                if (RENDERER != null) {
                    RENDERER.refresh();
                }
            }
        },

        components: {
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ToggleButton, Busy, InfoHover
        }
    });
    export default FriendNetwork;
</script>