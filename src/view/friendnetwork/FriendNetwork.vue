<template>
    <div class="d-flex flex-column" style="height: 100vh;">
        <honu-menu class="flex-grow-0">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/friendnetwork">Friend network</a>
            </li>
        </honu-menu>

        <div class="btn-group w-100 flex-grow-0">
            <button type="button" @click="startLayout" class="btn btn-success">
                Start
            </button>

            <button type="button" @click="endLayout" class="btn btn-warning">
                Stop layout
            </button>

            <toggle-button v-model="settings.sameWorld">
                Limit to same server
            </toggle-button>
        </div>

        <div v-if="loading == true">
            <div class="progress mt-3" style="height: 3rem;">
                <div class="progress-bar" :style="{ width: progressWidth }" style="height: 3rem;">
                    <span style="position: absolute; left: 50%; transform: translateX(-50%); font-size: 2.5rem;">
                        Loading {{total}} characters
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

                <a :href="'/c/' + selected.id">View character</a>

                <a @click="makeGraph(selected.id)" href="#">
                    View network
                </a>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuHomepage, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ToggleButton from "components/ToggleButton";

    import Graph from "graphology";
    import Sigma from "sigma";
    import FA2Layout from "node_modules/graphology-layout-forceatlas2/worker";

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

    export const FriendNetwork = Vue.extend({
        props: {

        },

        data: function() {
            return {
                context: null as HTMLElement | null,
                graph: null as Graph | null,
                hovered: null as string | null,
                neighbors: null as Set<string> | null,
                layout: null as FA2Layout | null,
                graphWidth: 5 as number,
                loaded: new Set() as Set<string>,
                queue: [] as FriendNode[],

                root: null as PsCharacter | null,
                selected: null as PsCharacter | null,

                loading: false as boolean,
                todo: 0 as number,
                total: 0 as number,

                friendMap: new Map() as Map<string, FlatExpandedCharacterFriend[]>,
                maxFriends: 0 as number,

                settings: {
                    sameWorld: true as boolean,
                },

                steps: {
                    friends: false as boolean,
                    layout: false as boolean,
                    render: false as boolean
                }
            }
        },

        mounted: function(): void {
            const parts: string[] = location.pathname.split("/").slice(1);
            if (parts.length != 2) {
                console.error(`Bad URL format: ${parts.join('/')}`);
            }

            this.$nextTick(() => {
                this.context = document.getElementById("graph");
                this.makeGraph(parts[1]);
            });
        },

        // todo: 
        //      search would be SLICK

        methods: {
            startLayout: function(): void {
                if (this.layout != null) {
                    console.log(`FriendNetwork> startting layout`);
                    this.layout.start();
                } else {
                    console.log(`FriendNetwork> no layout to start`);
                }
            },

            endLayout: function(): void {
                if (this.layout != null) {
                    console.log(`FriendNetwork> stopping layout`);
                    this.layout.stop();
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
                        }
                    }

                    if (l.state != "loaded") {
                        continue;
                    }

                    if (this.maxFriends <= l.data.length) {
                        this.maxFriends = l.data.length;
                    }

                    if (l.data.length < 10 && l.data.length > 5000) {
                        continue;
                    }

                    console.log(`FriendNetwork> Loaded ${i}/${length} Loaded ${l.data.length} friends for ${iter.characterID}`);

                    for (const f of l.data) {
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

            makeGraph: async function(rootCharacterID: string): Promise<void> {
                this.loading = true;
                this.total = 0;
                this.todo = 0;
                this.loaded = new Set<string>();
                this.queue = [];
                this.hovered = null;
                this.selected = null;
                this.maxFriends = 0;

                const character: Loading<PsCharacter> = await CharacterApi.getByID(rootCharacterID);
                if (character.state != "loaded") {
                    console.warn(`not here`);
                    return;
                }

                this.root = character.data;

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

                this.queue.push({ characterID: rootCharacterID, depth: 0 });
                this.loaded.add(rootCharacterID);
                this.graph.addNode(this.queue[0].characterID, {
                    x: 0, y: 0,
                    label: CharacterUtils.getDisplay(character.data),
                    size: 10, color: "gold"
                });

                await this.processQueue(1);
                this.total = this.queue.length;
                this.todo = this.queue.length + 1;

                await this.processQueue(2);

                this.graph.forEachNode((node, attr) => {
                    if (node == this.root?.id) {
                        attr.size = 10;
                    } else if (this.friendMap.has(node) == true) {
                        const friends: FlatExpandedCharacterFriend[] = this.friendMap.get(node)!;

                        attr.size = 1 + 10 * (friends.length / this.maxFriends);
                    }
                });

                this.steps.friends = true;

                const renderer = new Sigma(this.graph!, this.context, {
                    allowInvalidContainer: true
                });

                this.steps.render = true;

                if (this.layout != null) {
                    this.layout.kill();
                    this.layout = null;
                }

                this.layout = new FA2Layout(this.graph, {
                    settings: {
                        gravity: 1,
                        adjustSizes: true,
                        barnesHutOptimize: true
                    },
                });

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

                    return res;
                });

                renderer.setSetting("edgeReducer", (edge: string, data) => {
                    const res = { ...data };

                    if (this.hovered != null && this.graph!.hasExtremity(edge, this.hovered) == false) {
                        res.hidden = true;
                    }

                    return res;
                });

                renderer.setSetting("labelColor", { color: "#ffffff" });

                console.log(`made graph`);

                this.loading = false;
            }
        },

        computed: {
            progressWidth: function(): string {
                return `${((this.total - this.todo) / Math.max(1, this.total)) * 100}%`;
            }
        },

        components: {
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ToggleButton
        }
    });
    export default FriendNetwork;
</script>