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

        <div class="flex-grow-1">
            <div id="graph" class="w-100 h-100"></div>

            <div v-if="selected != null" style="position: relative; right: 0;">
                Selected
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
                    throw `asdflkajsd;f`;
                }
                const friends: FlatExpandedCharacterFriend[] = [];
                console.log(`FriendNetwork> have ${this.queue.length} entries in queue`);

                let i: number = 0;
                let length: number = this.queue.length;

                while (this.queue.length > 0) {
                    ++i;
                    const iter: FriendNode | undefined = this.queue.shift();
                    if (iter == undefined) {
                        continue;
                    }

                    const l: Loading<FlatExpandedCharacterFriend[]> = await CharacterFriendApi.getByCharacterID(iter.characterID, true);
                    if (l.state != "loaded") {
                        continue;
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
                                ...randomPosition(this.graphWidth),
                                label: CharacterUtils.getDisplay({ name: f.friendName ?? `<missing ${f.friendID}>`, outfitTag: f.friendOutfitTag }),
                                size: 3,
                                color: ColorUtil.getFactionColor(f.friendFactionID ?? 0)
                            });
                        }

                        this.graph.addEdge(iter.characterID, f.friendID);

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
                await this.processQueue(2);

                this.steps.friends = true;

                console.log(`FriendNetwork> Starting forceAtlas2 iterations`);

                this.steps.layout = true;

                console.log(`FriendNetwork> forceAtlas2, rendering`);

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
                    },
                    getEdgeWeight: "weight"
                });

                renderer.on("clickNode", (node) => {
                    if (this.hovered == node.node) {
                        this.neighbors = null;
                        this.hovered = null;
                    } else {
                        this.neighbors = new Set(this.graph!.neighbors(node.node));
                        this.hovered = node.node;
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
            }
        },

        components: {
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ToggleButton
        }
    });
    export default FriendNetwork;
</script>