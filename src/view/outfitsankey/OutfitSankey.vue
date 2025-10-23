<template>
    <div class="d-flex" style="flex-flow: column; height: 100vh; overflow: hidden;">
        <div class="d-flex align-items-center mb-2 flex-grow-0 flex-basis-0">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <a href="/outfitfinder">Outfit</a>
                </li>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <span v-if="outfit.state == 'idle' || outfit.state == 'loading'">
                        &lt;Loading...&gt;
                    </span>

                    <a v-else-if="outfit.state == 'loaded'" :href="'/o/' + outfitID">
                        {{outfit.data.name}}
                    </a>

                    <span v-else>
                        &lt;error&gt;
                    </span>
                </li>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    Sankey
                </li>
            </honu-menu>

            <div class="flex-grow-0 d-block mr-2">
                <div v-if="settings.show == false">
                    <button class="btn btn-success h-100" @click="settings.show = true">
                        Show settings
                    </button>
                </div>
            </div>

            <div class="flex-grow-0 d-block">
                <button class="btn btn-primary h-100" @click="resetZoom">
                    Reset zoom
                </button>
            </div>
        </div>

        <div class="flex-grow-0">
            <div v-if="sessions.state == 'loading'">
                <div v-if="progress.initial == false">
                    Loading outfit data
                    <busy v-if="progress.initial == false" class="honu-busy-lg"></busy>
                </div>
                <progress-bar :progress="progress.current" :total="progress.total">
                    Character sessions loaded:
                </progress-bar>
            </div>

            <div v-if="progress.step != ''">
                <span class="h3">
                    {{progress.step}}
                </span>
                <busy class="honu-busy-lg"></busy>
            </div>
        </div>

        <div id="page-root" class="flex-grow-1">
            <div id="d3_canvas" style="overflow: hidden;">
                <svg id="root" :width="graph.width" :height="graph.height" :viewBox="viewboxStr"></svg>
            </div>
        </div>

        <div id="options-menu" style="position: fixed; top: 200px; width: 300px;" class="bg-dark m-2" :class="[ settings.show ? 'slide-300-in' : 'slide-300-out' ]">
            <div>
                <button class="btn btn-secondary" @click="settings.show = false">
                    &times;
                </button>
            </div>

            <collapsible header-text="What is this?" :show="false">
                <div id="help-text">
                    <p>
                        This is a graphic that shows the changes in outfit membership for all characters in a specific outfit
                    </p>
                    <p>
                        Each outfit is a different colors, broken up by weeks.
                        Between each week, if a character in one outfit went to another, a path is created between them.
                    </p>
                    <p>
                        Hover over elements to highlight them, and click on paths between 
                    </p>
                </div>
            </collapsible>

            <hr class="border px-3 mx-1 my-2" />

            <div class="px-2">
                <label>
                    Highlight character
                    <info-hover text="Select a character to highlight the path of outfits"></info-hover>
                </label>

                <div class="input-group">
                    <select v-if="characters.state == 'loaded'" v-model="follow.characterID" class="form-control">
                        <option v-for="c in follow.sortedCharacters" :value="c.id">
                            {{c.name}}
                        </option>
                    </select>
                    <select v-else disabled class="form-control">
                        <option :value="null">loading...</option>
                    </select>
                    <button class="btn btn-secondary btn-group-addon" @click="closeFollowCharacter">
                        Clear
                    </button>
                </div>
            </div>

            <div class="px-2">
                <label>
                    Highlight outfit
                    <info-hover text="Select an outfit to highlight all paths of the outfit"></info-hover>
                </label>

                <div class="input-group">
                    <select v-if="outfits.state == 'loaded'" v-model="followOutfit.outfitID" class="form-control">
                        <option v-for="o in followOutfit.sortedOutfits" :value="o.id">
                            <span style="width: 1ch;" :style="{ 'color': outfitColors.get(o.id) }">
                                &nbsp;
                            </span>
                            {{o.name}}
                        </option>
                    </select>
                    <select v-else disabled class="form-control">
                        <option :value="null">loading...</option>
                    </select>
                    <button class="btn btn-secondary btn-group-addon" @click="closeFollowOutfit">
                        Clear
                    </button>
                </div>
            </div>

            <hr class="border px-3 mx-1 my-2" />

            <div class="px-2">
                <button class="btn btn-success w-100" @click="downloadPng">
                    Download PNG
                    <info-hover text="Open a PNG in a new tab of the Sankey"></info-hover>
                </button>
            </div>

            <hr class="border px-3 mx-1 my-2" />

            <collapsible header-text="Settings" :show="false">
                <div class="px-2">
                    <div class="input-group">
                        <span class="input-group-addon input-group-prepend">
                            node width (dx)
                        </span>
                        <input v-model.number="settings.nodeWidth" type="number" class="form-control" />
                    </div>

                    <div class="d-flex" style="flex-wrap: wrap;">
                        <toggle-button v-model="settings.showPaths" class="flex-grow-0">
                            show paths
                        </toggle-button>
                        <toggle-button v-model="settings.showPathId" class="flex-grow-0">
                            show path id
                        </toggle-button>
                        <button class="btn btn-success flex-grow-0" @click="assignOutfitColors(); redraw();">
                            recolor
                        </button>
                    </div>

                    <button class="btn btn-primary w-100 my-2" @click="redraw">
                        Redraw
                    </button>
                </div>
            </collapsible>
        </div>

        <div id="popper-div" :style="{ display: popper.show == true ? 'block' : 'none' }" style="background-color: var(--secondary); color: white; border: 2px var(--light) solid; position: fixed;">
            <div class="d-flex bg-dark" style="align-items: center;">
                <strong class="flex-grow-1 px-2">
                    {{popper.title}}
                </strong>

                <button type="button" class="btn flex-grow-0" @click="closePopper">
                    &times;
                </button>
            </div>

            <div class="bg-dark px-2 pb-2">
                <span>
                    {{popper.characters.length}} players
                </span>

                <span v-if="popper.outfitAID == popper.outfitBID">
                    stayed in 
                    <outfit-name :outfit-id="popper.outfitAID" :outfit="popper.outfitA"></outfit-name>
                </span>

                <span v-else-if="popper.outfitAID != popper.outfitBID">
                    left 
                    <outfit-name :outfit-id="popper.outfitAID" :outfit="popper.outfitA"></outfit-name>

                    for
                    <outfit-name :outfit-id="popper.outfitBID" :outfit="popper.outfitB"></outfit-name>
                </span>
            </div>

            <div style="max-height: 30vh; overflow: auto">
                <ul style="padding-left: 20px">
                    <li v-for="c in popper.characters" class="pr-1">
                        <a :href="'/c/' + c.id">
                            {{c.name}}

                            <span v-if="c.dateCreated.getTime() > popper.timestamp.getTime()" class="text-danger" title="This character was not created yet!" style="text-decoration: underline; padding-left: 0.25rem;">
                                *
                            </span>
                        </a>
                    </li>
                </ul>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import Toaster from "Toaster";
    import { Loadable, Loading } from "Loading";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsOutfit, OutfitApi } from "api/OutfitApi";
    import { CharacterApi, PsCharacter } from "api/CharacterApi";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import DateTimeInput from "components/DateTimeInput.vue";
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ProgressBar from "components/ProgressBar.vue";
    import ToggleButton from "components/ToggleButton";

    import TimeUtils from "util/Time";
    import * as moment from "moment";

    const d3: any = (window as any).d3;

    /// @ts-ignore
    //import * as d3z from "node_modules/d3-zoom/dist/d3-zoom.js";

    import ColorUtils from "util/Color";

    type HNode = {
        id: string;

        outfitID: string;
        timestamp: Date;

        name: string;
        color: string;
        x: number;
        y: number;
        dx: number;
        dy: number;
        value: number;

        sourceLinks: HNode[];
        targetLinks: HNode[];
    };

    type HLink = {
        source: string;
        target: string;
        value: number;
        dy: number;
    };

    /**
     * Class that representes the changes from one week to another
     */
    class MembershipWeek {
        public week: number = 0;
        private map: Map<string, string[]> = new Map();

        constructor(week: number) {
            this.week = week;
        }

        public getCharacters(outfitA: string, outfitB: string): string[] {
            const key: string = `${outfitA}-${outfitB}`;
            return this.map.get(key) ?? [];
        }

        public addCharacter(outfitA: string, outfitB: string, charID: string): void {
            const key: string = `${outfitA}-${outfitB}`;
            if (this.map.has(key) == false) {
                this.map.set(key, []);
            }

            this.map.get(key)!.push(charID);
        }

        public getCharacterPath(charID: string): string {
            let path: string | undefined = undefined;

            this.map.forEach((charIDs: string[], pathId: string) => {
                if (charIDs.indexOf(charID) > -1) {
                    path = pathId;
                    return;
                }
            });

            if (path == undefined) {
                console.error(`${charID} was not in the membership for week ${this.week}`);
            }

            return path ?? "0-0";
        }

    }

    /**
     * utility class that holds the weekly membership of each outfit
     */
    class Membership {
        private map: Map<number, MembershipWeek> = new Map();

        public getWeek(n: number): MembershipWeek {
            if (this.map.has(n) == false) {
                this.map.set(n, new MembershipWeek(n));
            }

            return this.map.get(n)!;
        }

        public getCharacterPathIds(charID: string, interval: number): string[] {
            const ids: string[] = [];

            this.map.forEach((value: MembershipWeek, weekN: number) => {
                const outfitIDs: string[] = value.getCharacterPath(charID).split("-");

                const outfitA: string = outfitIDs[0];
                const outfitB: string = outfitIDs[1];

                ids.push(`link-${weekN - interval}-${outfitA}-${weekN}-${outfitB}`);
            });

            return ids;
        }

    }

    const OutfitName = Vue.extend({
        props: {
            OutfitId: { type: String, required: true },
            outfit: { type: Object as PropType<PsOutfit | undefined>, required: false }
        },

        template: `
            <span v-if="OutfitId == '0'" >
                no outfit
            </span>
            <a v-else :href="'/o/' + OutfitId">
                <span v-if="outfit != undefined">
                    <span v-if="outfit.tag != null">
                        [{{outfit.tag}}]
                    </span>
                    {{outfit.name}}
                </span>
                <span v-else>
                    &lt;missing outfit {{OutfitId}}&gt;
                </span>
            </a>
        `
    });

    export const OutfitSankey = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfitID: "" as string,

                outfit: Loadable.idle() as Loading<PsOutfit>,
                sessions: Loadable.idle() as Loading<Session[]>,
                outfits: Loadable.idle() as Loading<PsOutfit[]>,
                outfitMap: new Map() as Map<string, PsOutfit>,

                characters: Loadable.idle() as Loading<PsCharacter[]>,
                charMap: new Map() as Map<string, PsCharacter>,

                interval: 1000 * 60 * 60 * 24 * 7 as number, // 1000 ms, 60 seconds, 60 minutes, 24 hours, 7 days

                debugLevel: 0 as number,

                graph: {
                    root: null as SVGElement | null,
                    zoom: null as any | null,

                    width: 100 as number,
                    height: 100 as number,

                    nodes: [] as HNode[],
                    links: [] as HLink[],

                    map: new Map() as Map<string, HNode>,
                    linkSourceMap: new Map() as Map<string, HLink[]>,
                    linkTargetMap: new Map() as Map<string, HLink[]>,
                    diff: new Membership() as Membership
                },

                settings: {
                    show: true as boolean,
                    pxPerWeek: 50 as number,
                    pxPerChar: 5 as number,
                    showPaths: true as boolean,
                    showPathFill: true as boolean,
                    showNodeStroke: true as boolean,
                    nodeWidth: 10 as number,
                    showPathId: false as boolean
                },

                follow: {
                    active: false as boolean,
                    characterID: "" as string,
                    sortedCharacters: [] as PsCharacter[],
                    paths: [] as string[],
                },

                followOutfit: {
                    active: false as boolean,
                    outfitID: "" as string,
                    sortedOutfits: [] as PsOutfit[],
                    paths: [] as string[]
                },

                selectedCharacter: "" as string,

                outfitColors: new Map() as Map<string, string>,

                popper: {
                    show: false as boolean,
                    title: "" as string,
                    header: "" as string,
                    timestamp: new Date() as Date,
                    characters: [] as PsCharacter[],

                    outfitAID: "" as string,
                    outfitBID: "" as string,

                    outfitA: undefined as PsOutfit | undefined,
                    outfitB: undefined as PsOutfit | undefined
                },

                progress: {
                    step: "" as string,
                    initial: false as boolean,
                    current: 0 as number,
                    total: 0 as number
                },
            }
        },

        created: function(): void {
            document.title = "Honu / Sankey";
            this.parseOutfitIDFromUrl();
        },

        mounted: function(): void {
            this.$nextTick(async () => {

                const pageRoot: HTMLElement | null = document.getElementById("page-root");
                if (pageRoot == null) {
                    Toaster.add("HTML error", "failed to find #page-root", "danger");
                    throw `failed to find #page-root`;
                }

                this.graph.width = pageRoot.clientWidth;
                this.graph.height = pageRoot.clientHeight;
                console.log(`OutfitSankey> width: ${this.graph.width}, height: ${this.graph.height}`);

                this.progress.step = "parsing outfit ID";
                this.parseOutfit();

                this.progress.step = "loading outfit sessions";
                await this.getSessions();

                this.progress.step = "loading outfit data";
                await this.getOutfits();

                this.progress.step = "loading characters";
                await this.getCharacters();

                this.progress.step = "loading data";
                this.makeData();

                this.progress.step = "creating graphic";
                this.d3s();

                this.progress.step = "";
            });
        },

        methods: {
            parseOutfitIDFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                const outfitID: number = Number.parseInt(parts[2]);
                if (Number.isNaN(outfitID) == false) {
                    this.outfitID = parts[2];
                    console.log(`outfit id is ${this.outfitID}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${outfitID}`;
                }
            },

            parseOutfit: async function(): Promise<void> {
                this.outfit = await OutfitApi.getByID(this.outfitID);
                if (this.outfit.state == "nocontent") {
                    document.title = `Honu / Outfit / <not found> / Sankey`;
                } else if (this.outfit.state == "loaded") {
                    document.title = `Honu / Outfit / ${this.outfit.data.name} / Sankey`;

                    const url = new URL(location.href);
                    url.searchParams.set("tag", this.outfit.data.tag ?? this.outfit.data.name);
                    history.replaceState({ path: url.href }, "", url.href);
                }
            },

            /**
             * Create a node
             * 
             * @param id
             * @param outfitID
             * @param name
             * @param value
             * @param x
             * @param y
             */
            makeNode: function(id: string, outfitID: string, name: string, value: number, x?: number, y?: number): HNode {
                return {
                    id: id,
                    outfitID: outfitID,
                    timestamp: new Date(),
                    name: name,
                    color: ColorUtils.randomColorSingle(),
                    x: x ?? Math.random() * 1920,
                    y: y ?? Math.random() * 1080,
                    dx: this.settings.nodeWidth,
                    dy: value,
                    sourceLinks: [],
                    targetLinks: [],
                    value: value
                };
            },

            /**
             * Create a link between two nodes
             * 
             * @param source
             * @param target
             * @param value
             * @param dy
             */
            makeLink: function(source: HNode, target: HNode, value: number, dy: number): HLink {
                const link: HLink = {
                    source: source.id,
                    target: target.id,
                    value: value,
                    dy: dy
                };

                source.targetLinks.push(target);
                target.sourceLinks.push(source);

                return link;
            },

            debug: function(...data: any[]): void {
                if (this.debugLevel >= 1) {
                    console.log(...data);
                }
            },

            trace: function(...data: any[]): void {
                if (this.debugLevel >= 2) {
                    console.log(...data);
                }
            },

            highlightCharacter: function(charID: string): void {
                console.log(`highlighting all paths that ${charID} took`);

                // remove previous path
                for (const path of this.follow.paths) {
                    document.getElementById(path)?.classList.remove("link-hover");
                }

                this.follow.active = true;
                this.follow.paths = this.graph.diff.getCharacterPathIds(charID, this.interval);

                for (const path of this.follow.paths) {
                    const elem: HTMLElement | null = document.getElementById(path);
                    if (elem == null) {
                        console.warn(`missing ${path}`);
                    } else {
                        elem.classList.add("link-hover");
                    }
                }
            },

            closeFollowCharacter: function(): void {
                for (const path of this.follow.paths) {
                    const elem: HTMLElement | null = document.getElementById(path);
                    if (elem == null) {
                        console.warn(`missing ${path}`);
                    } else {
                        elem.classList.remove("link-hover");
                    }
                }

                this.follow.characterID = "";
                this.follow.active = false;
            },

            openFollowOutfit: function(): void {
                for (const elemId of this.followOutfit.paths) {
                    document.getElementById(elemId)?.classList.remove("link-hover", "node-hover");
                }

                console.log(`showing outfit paths for ${this.followOutfit.outfitID}`);

                const elements: Set<string> = new Set();
                for (const node of this.graph.nodes) {
                    if (node.outfitID != this.followOutfit.outfitID) {
                        continue;
                    }

                    for (const link of node.sourceLinks) {
                        elements.add(`link-${link.id}-${node.id}`);
                    }
                    for (const link of node.targetLinks) {
                        elements.add(`link-${node.id}-${link.id}`);
                    }
                    elements.add(`node-${node.id}`);
                }

                this.followOutfit.paths = Array.from(elements.keys());

                for (const elemId of elements) {
                    const elem: HTMLElement | null = document.getElementById(elemId);
                    if (elem != null) {
                        if (elemId.startsWith("node-")) {
                            elem.classList.add("node-hover");
                        } else {
                            elem.classList.add("link-hover");
                        }
                    } else {
                        console.warn(`failed to find #${elemId}`);
                    }
                }

                this.followOutfit.active = true;
            },

            closeFollowOutfit: function(): void {
                for (const elemId of this.followOutfit.paths) {
                    document.getElementById(elemId)?.classList.remove("link-hover", "node-hover");
                }

                this.followOutfit.outfitID = "";
                this.followOutfit.active = false;
            },

            redraw: function(): void {
                this.makeData();
                this.d3s();
            },

            resetZoom: function(): void {
                if (this.graph.zoom != null) {
                    d3.select("svg")
                        .transition()
                        .call(this.graph.zoom.transform, d3.zoomIdentity);
                }
            },

            /**
             * Create the d3 graph
             */
            d3s: function(): void {
                console.time("do d3");

                if (this.graph.root != null) {
                    for (let i = 0; i < this.graph.root.childNodes.length; ++i) {
                        this.graph.root.childNodes.item(i).remove();
                    }
                }

                const svg = d3.select("#root");

                const svgRoot: HTMLElement | null = document.getElementById("root");
                if (svgRoot == null) {
                    throw `failed to find #root`;
                }

                console.log(svgRoot);

                // this is fine
                this.graph.root = svgRoot as unknown as SVGElement;

                this.debug(`d3 stuff`);

                const docRoot = svg.append("g")
                    .attr("id", "doc-root");

                this.graph.zoom = d3.zoom();
                this.graph.zoom
                    .scaleExtent([0.1, 40])
                    //.translateExtent([[0, 0], [width, height]])
                    .filter((ev: any) => {
                        return ev.type != "mousedownn";
                    })
                    .on("zoom", (ev: any) => {
                        // a zoom event can also be a drag/pan event
                        // so if we click on a path (which opens the popper), then start dragging, we want to close the popper
                        this.closePopper();
                        docRoot.attr("transform", ev.transform);
                    });

                svg.call(this.graph.zoom);

                for (const node of this.graph.nodes) {
                    node.sourceLinks.sort((a, b) => a.y - b.y);
                    node.targetLinks.sort((a, b) => a.y - b.y);
                }

                const nodeMap: Map<string, HNode> = new Map();
                for (const node of this.graph.nodes) {
                    nodeMap.set(node.id, node);
                }

                this.debug(this.graph.nodes);
                this.debug(this.graph.links);

                this.debug(`over here!`);

                const getNodeOrThrow = (id: string): HNode => {
                    const n: HNode | undefined = nodeMap.get(id);
                    if (n == undefined) {
                        throw `failed to find node ${id}`;
                    }

                    return n;
                };

                // make a gradient for each path
                this.debug(`making linearGradient`);
                const defs = svg.append("defs");

                const gradients = defs.selectAll(".link")
                    .data(this.graph.links)
                    .enter()
                    .append("linearGradient")
                    .attr("id", (d: HLink) => { return `${d.source}-${d.target}`; })
                    .attr("gradientUnits", "userSpaceOnUse")
                    .attr("x1", (d: HLink) => getNodeOrThrow(d.source).x)
                    .attr("x2", (d: HLink) => getNodeOrThrow(d.target).x);

                // add the starting color to each path
                this.debug("adding stop 0");
                gradients.append("stop")
                    .attr("offset", "0%")
                    .attr("stop-color", (d: HLink) => {
                        const source: HNode = nodeMap.get(d.source)!;
                        return source.color;
                    });

                // add the ending color to each path
                this.debug("adding stop 1");
                gradients.append("stop")
                    .attr("offset", "100%")
                    .attr("stop-color", (d: HLink) => {
                        const target: HNode = nodeMap.get(d.target)!;
                        return target.color;
                    });

                // generate link maps, used to calculate path offsets
                this.graph.linkSourceMap.clear();
                this.graph.linkTargetMap.clear();
                for (const link of this.graph.links) {
                    if (this.graph.linkSourceMap.has(link.source) == false) {
                        this.graph.linkSourceMap.set(link.source, []);
                    }

                    this.graph.linkSourceMap.get(link.source)!.push(link);

                    if (this.graph.linkTargetMap.has(link.target) == false) {
                        this.graph.linkTargetMap.set(link.target, []);
                    }
                    this.graph.linkTargetMap.get(link.target)!.push(link);
                }

                // build the path elements
                this.debug("building links");

                if (this.settings.showPaths == true) {
                    const paths = docRoot.append("g")
                        .selectAll(".link")
                        .data(this.graph.links)
                        .enter()
                        .append("path")
                        .attr("class", "link")
                        .attr("id", (d: HLink) => { return `link-${d.source}-${d.target}`; })
                        .attr("d", (d: HLink) => { return this.makeLinkPath(d); })
                        .attr("stroke", (d: HLink) => { return `white`; })
                        .attr("stroke-width", "2")
                        //.attr("stroke-opacity", "0") // this is handled by the css in OutfitSankey.cshtml
                        .attr("fill", (d: HLink) => { return `url(#${d.source}-${d.target})`; })
                        .style("mix-blend-mode", "multiple")
                        .style("fill-opacity", (d: HLink) => {
                            const source: HNode = getNodeOrThrow(d.source);
                            const target: HNode = getNodeOrThrow(d.target);

                            if (source.outfitID != target.outfitID) {
                                return this.lerp(0.5, 0.8, 1 - (d.value / this.charMap.size));
                            }

                            return 0.5;
                        })
                        .on("mousedown", this.pathClickListener);

                    // sort them based on value, so paths that cross over each other can still be hovered over
                    paths.sort((a: HLink, b: HLink) => {
                        const sourceA: HNode = getNodeOrThrow(a.source);
                        const targetA: HNode = getNodeOrThrow(a.target);
                        const sameA: boolean = sourceA.outfitID == targetA.outfitID;

                        const sourceB: HNode = getNodeOrThrow(b.source);
                        const targetB: HNode = getNodeOrThrow(b.target);
                        const sameB: boolean = sourceB.outfitID == targetB.outfitID;

                        // if a link is to the same outfit, but the other one isn't, make that one show up on top
                        if (sameA == true && sameB == true) {
                            return b.value - a.value;
                        } else if (sameA == true && sameB == false) {
                            return -1;
                        } else if (sameA == false && sameB == true) {
                            return 1;
                        } else if (sameA == false && sameB == false) {
                            return b.value - a.value;
                        } else {
                            throw `unchecked sameA//B values!`;
                        }
                    });

                    // add text to each path that gives what the link is
                    this.debug("building link text");
                    paths.append("title")
                        .text((d: HLink) => {
                            const source: HNode = nodeMap.get(d.source)!;
                            const target: HNode = nodeMap.get(d.target)!;

                            const weekEnd: Date = new Date(source.timestamp.getTime() + this.interval);

                            let s: string = `${TimeUtils.formatNoTimezone(source.timestamp, "YYYY-MM-DD")} - ${TimeUtils.formatNoTimezone(weekEnd, "YYYY-MM-DD")}`
                                + `\n${source.name} to ${target.name}: ${d.value}`;

                            if (this.settings.showPathId == true) {
                                s = `link-${d.source}-${d.target}\n` + s;
                            }

                            return s;
                        });
                }

                // add the nodes for each week
                this.debug("building nodes");
                const node = docRoot.append("g")
                    .selectAll(".node")
                    .data(this.graph.nodes)
                    .enter()
                    .append("rect")
                    .attr("id", (d: HNode) => `node-${d.id}`)
                    .attr("class", "node")
                    .attr("width", (d: HNode) => d.dx)
                    .attr("height", (d: HNode) => d.dy)
                    .attr("x", (d: HNode) => d.x)
                    .attr("y", (d: HNode) => d.y)
                    .style("fill", (d: HNode) => d.color)
                    // when you hover over a node, highlight all paths to and from it
                    .on("mouseover", (ev: any, node: HNode) => {
                        if (this.follow.active == true || this.followOutfit.active == true) {
                            return;
                        }

                        for (const pathID of this.getLinkIds(node)) {
                            document.getElementById(pathID)?.classList.add("link-hover");
                        }
                    })
                    // and when no longer hovering, remove the highlight
                    .on("mouseleave", (ev: any, node: HNode) => {
                        if (this.follow.active == true || this.followOutfit.active == true) {
                            return;
                        }

                        for (const pathID of this.getLinkIds(node)) {
                            document.getElementById(pathID)?.classList.remove("link-hover");
                        }
                    })
                    .append("title")
                    .text((d: HNode) => {
                        return `${d.name}: ${d.value} characters`;
                    });


                console.timeEnd("do d3");
            },

            makeNodeMap: function(): void {
                for (const node of this.graph.nodes) {
                    this.graph.map.set(node.id, node);
                }
            },

            /**
             * get the element IDs of the paths that go to a node and from a node
             * @param node
             */
            getLinkIds: function(node: HNode): string[] {
                const pathIds: string[] = [];

                for (const source of node.sourceLinks) {
                    pathIds.push(`link-${source.id}-${node.id}`);
                }

                for (const target of node.targetLinks) {
                    pathIds.push(`link-${node.id}-${target.id}`);
                }

                return pathIds;
            },

            /**
             * callback pulled out, this is what makes the curve
             * @param d
             */
            makeLinkPath: function(d: HLink): string {
                this.debug(`creating path for ${d.source} => ${d.target}: ${d.value} / ${d.dy}`);

                const source: HNode = this.graph.map.get(d.source)!;
                const target: HNode = this.graph.map.get(d.target)!;

                this.debug("\tsource", source);
                this.debug("\ttarget", target);

                // can't have the path end in the center path each time
                // so an offset is added, based on the height of the nodes before it
                let targetOffset: number = 0;
                this.debug(`\tfinding targetOffset for ${d.source} => ${d.target}/${d.dy}`);
                const targetLinks: HLink[] = this.graph.linkTargetMap.get(target.id) ?? [];
                for (const l of targetLinks) {
                    if (l.source == source.id) {
                        break;
                    }
                    targetOffset += l.dy;
                }

                let sourceOffset: number = 0;
                this.debug(`\tfinding sourceOffset for ${d.source} => ${d.target}/${d.dy}`);
                const sourceLinks: HLink[] = this.graph.linkSourceMap.get(source.id) ?? [];
                for (const l of sourceLinks) {
                    if (l.target == target.id) {
                        break;
                    }
                    sourceOffset += l.dy;
                }

                // start in top right corner of source node (x0, y0)
                const x0 = source.x + source.dx;
                const y0 = source.y + sourceOffset;

                // curve to top left corner of target node (x1, y1)
                const x1 = target.x;
                const y1 = target.y + targetOffset;

                // straight line down to bottom left corner of target node (x2, y2)
                const x2 = target.x;
                const y2 = target.y + targetOffset + d.dy;

                // curve to bottom right corner of source node (x3, y3)
                const x3 = source.x + source.dx;
                const y3 = source.y + sourceOffset + d.dy;

                const start: string = `M ${x0}, ${y0} `;
                const p1: string = this.bcurve(x0, y0, x1, y1);
                const p2: string = `L ${x2}, ${y2} `;
                const p3: string = this.bcurve(x2, y2, x3, y3);

                return `${start} ${p1} ${p2} ${p3} Z`;
            },

            bcurve: function(x0: number, y0: number, x1: number, y1: number): string {
                return ` C ${this.lerp(x0, x1, 0.5)}, ${y0} `
                    + ` ${this.lerp(x0, x1, 0.5)}, ${y1} `
                    + ` ${x1}, ${y1} ` ;
            },

            lerp: function(a: number, b: number, p: number): number {
                return a * (1 - p) + b * p;
            },

            downloadPng: function(): void {
                if (this.graph.root == null) {
                    console.error("cannot download png: graph root is null");
                    return;
                }

                const header: string = "data:image/svg+xml;charset=utf-8";
                const xmlData: string = new XMLSerializer().serializeToString(this.graph.root);

                const xml: string = `${header},${encodeURIComponent(xmlData)}`;

                const img: HTMLImageElement = document.createElement("img");
                img.src = xml;

                img.onload = () => {
                    const canvas: HTMLCanvasElement = document.createElement("canvas");
                    canvas.width = this.graph.width;
                    canvas.height = this.graph.height;

                    canvas.getContext("2d")!.drawImage(img, 0, 0, canvas.width, canvas.height);

                    const dataUrl: string = canvas.toDataURL("image/png", 1.0);

                    console.log(`dataUrl`);

                    const tab = window.open();
                    if (tab == null) {
                        console.error(`failed to open new window`);
                    } else {
                        tab.document.body.innerHTML = `<img src="${dataUrl}" width=${this.graph.width}px height=${this.graph.height}px>`;
                    }
                };

            },

            nodeClickListener: function(ev: any, node: HNode): void {

            },

            /**
             * listener for when a path is clicked on
             * opens a pop up that shows which characters moved from what outfit
             * @param ev
             * @param link
             */
            pathClickListener: function(ev: any, link: HLink): void {
                if (ev.button != 0) {
                    return;
                }

                console.log(`showing link ${link.source}-${link.target}`);

                const source: HNode = this.graph.map.get(link.source)!;
                const target: HNode = this.graph.map.get(link.target)!;

                const popperDiv: HTMLElement | null = document.getElementById("popper-div");
                if (popperDiv == null) {
                    console.error(`Missing tooltip element '#popper-div'`);
                    return;
                }

                console.log(ev);
                popperDiv.style.display = "block";
                popperDiv.style.left = `${ev.clientX + 4}px`;

                if (ev.clientY < (window.innerHeight / 2)) {
                    popperDiv.style.bottom = "";
                    popperDiv.style.top = `${ev.clientY}px`;
                } else {
                    popperDiv.style.top = "";
                    popperDiv.style.bottom = `${window.innerHeight - ev.clientY}px`;
                }

                console.log(`put popper at ${popperDiv.style.top}/${popperDiv.style.bottom}, ${popperDiv.style.left}`);

                const charIDs: string[] = this.graph.diff.getWeek(target.timestamp.getTime()).getCharacters(source.outfitID, target.outfitID);

                this.popper.characters = charIDs.map((charID: string) => {
                    return this.charMap.get(charID);
                }).filter((iter: PsCharacter | undefined) => iter != undefined).map(iter => iter!);
                console.log(`characters ${this.popper.characters.length}`);

                this.popper.characters.sort((a, b) => a.name.localeCompare(b.name));

                this.popper.timestamp = target.timestamp;
                this.popper.title = `Between ${TimeUtils.formatNoTimezone(source.timestamp, "YYYY-MM-DD")} and ${TimeUtils.formatNoTimezone(target.timestamp, "YYYY-MM-DD")}`;

                this.popper.outfitAID = source.outfitID;
                this.popper.outfitA = this.outfitMap.get(source.outfitID);
                this.popper.outfitBID = target.outfitID;
                this.popper.outfitB = this.outfitMap.get(target.outfitID);
                this.popper.show = true;
            },

            /**
             * Use the session data to make the Sankey data
             */
            makeData: function(): void {
                if (this.sessions.state != "loaded") {
                    return console.warn(`sessions not loaded`);
                }

                if (this.sessions.data.length == 0) {
                    return console.warn(`no session data`);
                }

                if (this.outfits.state != "loaded") {
                    return console.warn(`outfits not loaded`);
                }

                this.graph.nodes = [];
                this.graph.links = [];
                this.graph.diff = new Membership();
                this.graph.map.clear();
                this.graph.linkTargetMap.clear();
                this.graph.linkSourceMap.clear();

                const firstSession: Session = this.sessions.data.sort((a, b) => a.start.getTime() - b.start.getTime())[0];
                console.log(`first session at ${firstSession.start}`);

                const day: Date = moment(firstSession.start).weekday(0).toDate();
                day.setUTCHours(0);
                day.setUTCMinutes(0);
                day.setUTCSeconds(0);
                day.setUTCMilliseconds(0);

                const now: Date = new Date();
                now.setUTCHours(0);
                now.setUTCMinutes(0);
                now.setUTCSeconds(0);
                now.setUTCMilliseconds(0);

                const c: Map<string, string> = new Map();

                const charMap: Map<string, PsCharacter> = new Map();
                if (this.characters.state == "loaded") {
                    for (const char of this.characters.data) {
                        charMap.set(char.id, char);
                    }
                }

                // set initial outfits
                for (const session of this.sessions.data) {
                    if (c.has(session.characterID) == true) {
                        continue;
                    }

                    // if a character was created after the first session, set them as no outfit
                    const char: PsCharacter | undefined = charMap.get(session.characterID);
                    if (char != undefined && char.dateCreated >= day) {
                        c.set(session.characterID, "0");
                    } else {
                        c.set(session.characterID, session.outfitID ?? "0");
                    }
                }

                const characterCount: number = c.size;

                console.log(`Loading data for ${characterCount} characters`);

                const outfitSet: Set<string> = new Set();
                for (const s of this.sessions.data) {
                    outfitSet.add(s.outfitID ?? "0");
                }
                const outfitIDs: string[] = Array.from(outfitSet.keys()).sort((a: string, b: string) => {
                    // if a is the outfit we want, or b is no outfit, put a first
                    if (a == this.outfitID || b == "0") {
                        return -1;
                    }
                    // if b is the outfit we want, or a is no outfit, put b first
                    if (b == this.outfitID || a == "0") {
                        return 1;
                    }
                    return a.localeCompare(b);
                });
                console.log(`considering ${outfitIDs.length} outfits: [${outfitIDs.join(", ")}]`);

                const nodeMap: Map<string, HNode> = new Map();

                let index: number = 0;

                // put each session into a corresponding bucket for quicker retrieval 
                const buckets: Map<number, Session[]> = new Map();
                for (const session of this.sessions.data) {
                    const slice: number = Math.floor(session.start.getTime() / this.interval);

                    if (buckets.has(slice) == false) {
                        buckets.set(slice, []);
                    }

                    buckets.get(slice)!.push(session);
                }

                console.log(`${buckets.size} buckets`);
                //this.graph.width = buckets.size * (this.settings.pxPerWeek + this.settings.nodeWidth);
                console.log(`${c.size} characters`);
                //this.graph.height = c.size * this.settings.pxPerChar;

                console.time("make data");

                // go thru each week, instead of each bucket, as a bucket may be empty
                for (let i = day.getTime(); i <= now.getTime(); i += this.interval) {
                    if (index++ >= 2) {
                        //break;
                    }

                    const slice: Session[] = buckets.get(Math.floor(i / this.interval)) ?? [];

                    // copy the previous outfits to a new one
                    const previousOutfitMembership: Map<string, string> = new Map();
                    c.forEach((v, k) => {
                        previousOutfitMembership.set(k, v);
                    });

                    this.debug(`NEW LINE =====================================`);
                    this.debug(`\tLoaded ${slice.length} sessions between ${i} and ${i + this.interval}`);

                    for (const session of slice) {
                        const outfitID: string = session.outfitID ?? "0";
                        c.set(session.characterID, outfitID);
                    }

                    // count of how many characters we've put in places
                    let count: number = 0;

                    for (const outfitID of outfitIDs) {
                        let outfit: HNode = this.makeNode(`${i}-${outfitID}`, outfitID, `${outfitID}`, 0, 0, 0);

                        this.debug(`\tChecking outfit ${outfitID}/${outfit.id}`);

                        const diffMap: Map<string, number> = new Map(); // <outfit ID, difference>

                        // how many characters are in this outfit. this includes characters with no session this week (didn't play)
                        let outfitCount: number = 0;
                        c.forEach((value: string, charID: string) => {
                            if (value == outfitID) {
                                ++outfitCount;
                            }

                            if (value == outfitID) {
                                const previousOutfitID: string = previousOutfitMembership.get(charID)!;
                                if (value != previousOutfitID) {
                                    diffMap.set(previousOutfitID, (diffMap.get(previousOutfitID) ?? 0) + 1);
                                    this.trace(`\t\t${charID} went from ${previousOutfitID} to ${value}`);
                                }
                                this.graph.diff.getWeek(i).addCharacter(previousOutfitID, outfitID, charID);
                            }
                        });

                        outfit.timestamp = new Date(i);
                        outfit.value = outfitCount;
                        outfit.y = (count / characterCount) * this.graph.height;
                        outfit.x = ((i - day.getTime()) / (now.getTime() - day.getTime())) * this.graph.width;
                        outfit.dx = 10;
                        outfit.dy = outfit.value / characterCount * this.graph.height;
                        outfit.color = outfitID == "0" ? "black" : this.outfitColors.get(outfitID)!;

                        if (outfitID == "0") {
                            outfit.name = "no outfit";
                        } else {
                            outfit.name = this.outfits.data.find(iter => iter.id == outfitID)?.name ?? `<missing ${outfitID}>`;
                        }
                        //outfit.name += ` ${outfitID}`;

                        count += outfit.value;

                        if (outfit.value > 0) {
                            this.graph.nodes.push(outfit);
                            this.trace(`\t\t${outfitID} had ${outfit.value} characters with sessions`);
                            nodeMap.set(outfit.id, outfit);

                            const previousTime = i - this.interval;
                            let countDown: number = outfit.value;

                            // make same outfit paths show up first
                            diffMap.forEach((difference: number, prevOutfitID: string) => {
                                countDown -= difference;
                            });

                            if (countDown > 0) {
                                const thisNode: HNode | undefined = nodeMap.get(`${previousTime}-${outfitID}`);
                                if (thisNode != undefined) {
                                    const link: HLink = this.makeLink(thisNode, outfit, countDown, (countDown / characterCount) * this.graph.height);
                                    this.graph.links.push(link);
                                }
                            }

                            diffMap.forEach((difference: number, prevOutfitID: string) => {
                                const sourceNode: HNode | undefined = nodeMap.get(`${previousTime}-${prevOutfitID}`);
                                if (sourceNode == undefined) {
                                    console.warn(`missing ${previousTime}-${prevOutfitID}`);
                                    return;
                                }

                                const link: HLink = this.makeLink(sourceNode, outfit, difference, (difference / characterCount) * this.graph.height);
                                this.graph.links.push(link);
                                this.debug(`\t\tCHANGE ${sourceNode.id} => ${outfit.id}: ${difference}`);
                            });

                            this.debug(`\t\tSTAYED ${previousTime}-${outfitID} => ${i}-${outfitID}: ${countDown}`);
                        }
                    }

                    if (count != characterCount) {
                        console.warn(`mismatched number of characters! expected ${characterCount}, got ${count} instead`);
                    }
                }

                this.makeNodeMap();

                console.timeEnd("make data");
            },

            /**
             * Get all the sessions relevant to load the outfit's sankey
             */
            getSessions: async function(): Promise<void> {
                console.time("get sessions");
                this.sessions = Loadable.loading();

                const all: Session[] = [];

                console.time("get sessions: outfit")
                const s: Loading<Session[]> = await SessionApi.getByOutfit(this.outfitID);
                if (s.state != "loaded") {
                    return;
                }
                this.progress.initial = true;

                console.log(`${s.data.length} from outfit`);

                all.push(...s.data);
                console.timeEnd("get sessions: outfit");

                const charSet: Set<string> = new Set();
                for (const session of s.data) {
                    charSet.add(session.characterID);
                }

                const charIDs: string[] = Array.from(charSet.keys());
                this.progress.total = charIDs.length;

                for (const id of charIDs) {
                    const charSessions: Loading<Session[]> = await SessionApi.getByCharacterID(id);
                    if (charSessions.state == "loaded") {
                        all.push(...charSessions.data);
                    } else {
                        console.warn(`got state ${charSessions.state} to get sessions for ${id}`);
                    }
                    ++this.progress.current;
                }

                console.log(`${all.length} sessions loaded`);

                // remove duplicates
                console.time("sessions: get unique");
                const map: Map<number, Session> = new Map();
                for (const session of all) {
                    map.set(session.id, session);
                }

                this.sessions = Loadable.loaded(Array.from(map.values()));
                console.timeEnd("sessions: get unique");

                console.timeEnd("get sessions");
            },

            /**
             * Get all the outfits from the sessions that are loaded
             */
            getOutfits: async function(): Promise<void> {
                if (this.sessions.state != "loaded") {
                    return console.warn(`cannot get outfits, sessions is ${this.sessions.state} is not 'loaded'`);
                }

                console.time("get outfits");

                console.time("outfits: get unique")
                const outfitIDs: Set<string> = new Set();
                for (const s of this.sessions.data) {
                    if (s.outfitID != null) {
                        outfitIDs.add(s.outfitID);
                    }
                }

                console.timeEnd("outfits: get unique");

                console.time("outfits: load api");
                this.outfits = Loadable.loading();
                this.outfits = await OutfitApi.getByIDs(Array.from(outfitIDs.keys()));
                console.timeEnd("outfits: load api");

                if (this.outfits.state == "loaded") {
                    this.assignOutfitColors();

                    this.followOutfit.sortedOutfits = [...this.outfits.data].sort((a, b) => a.name.localeCompare(b.name));
                }

                console.timeEnd("get outfits");
            },

            assignOutfitColors: function(): void {
                if (this.outfits.state != "loaded") {
                    console.warn(`cannot assign outfit colors: outfits is ${this.outfit.state}, not 'loaded'`);
                    return;
                }

                console.time("outfits: colors");
                this.outfitColors.clear();

                const colors: string[] = ColorUtils.randomColors(Math.random(), this.outfits.data.length);
                colors.sort((a, b) => Math.random() - 0.5);

                for (let i = 0; i < this.outfits.data.length; ++i) {
                    const outfitID: string = this.outfits.data[i].id;
                    this.outfitColors.set(outfitID, colors[i]);
                }

                for (const outfit of this.outfits.data) {
                    this.outfitMap.set(outfit.id, outfit);
                }
                console.timeEnd("outfits: colors");
            },

            getCharacters: async function(): Promise<void> {
                if (this.sessions.state != "loaded") {
                    return console.warn(`cannot get characters, sessions is ${this.sessions.state} is not 'loaded'`);
                }

                console.time("get characters");

                console.time("chars: get unique");
                const charIDs: Set<string> = new Set();
                for (const s of this.sessions.data) {
                    charIDs.add(s.characterID);
                }
                console.timeEnd("chars: get unique");

                console.time("chars: load api");
                this.characters = Loadable.loading();
                this.characters = await CharacterApi.getByIDs(Array.from(charIDs.keys()));
                console.timeEnd("chars: load api");

                if (this.characters.state == "loaded") {
                    console.time("chars: map");
                    this.charMap.clear();
                    for (const c of this.characters.data) {
                        this.charMap.set(c.id, c);
                    }
                    console.timeEnd("chars: map");

                    this.follow.sortedCharacters = [...this.characters.data].sort((a, b) => a.name.localeCompare(b.name));
                }

                console.timeEnd("get characters");
            },

            closePopper: function (): void {
                const tooltip: HTMLElement | null = document.getElementById("popper-div");
                if (tooltip != null) {
                    tooltip.style.display = "none";
                }
                this.popper.show = false;
            },

        },

        components: {
            DateTimeInput, InfoHover, Busy,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ProgressBar, Collapsible, ToggleButton,
            OutfitName
        },

        computed: {
            viewboxStr: function(): string {
                return `${0},${0},${this.graph.width},${this.graph.height}`;
            }
        },

        watch: {
            "follow.characterID": function(): void {
                if (this.follow.characterID != "" && this.follow.characterID != undefined) {
                    this.highlightCharacter(this.follow.characterID);
                }
            },

            "followOutfit.outfitID": function(): void {
                if (this.followOutfit.outfitID != "" && this.followOutfit.outfitID != undefined) {
                    this.openFollowOutfit();
                }
            }
        }
    });
    export default OutfitSankey;
</script>