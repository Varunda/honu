<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <a href="/outfitsankey">Sankey</a>
                </li>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <span v-if="outfit.state == 'loading'">
                        &lt;Loading...&gt;
                    </span>

                    <a v-else-if="outfit.state == 'loaded'" :href="'/o/' + outfitID">
                        {{outfit.data.name}}
                    </a>

                    <span v-else>
                        &lt;ERROR&gt;
                    </span>
                </li>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    Sankey
                </li>
            </honu-menu>
        </div>
        
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

        <div id="popper-root" style="z-index: 10000;"></div>

        <div id="d3_canvas"></div>

        <div id="popper-div" style="display: none; background-color: var(--secondary); color: white; border: 2px var(--light) solid; position: fixed;">
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
                    <span v-if="popper.outfitAID == '0'" >
                        no outfit
                    </span>
                    <span v-else-if="popper.outfitA != undefined">
                        <a :href="'/o/' + popper.outfitA.id">
                            <span v-if="popper.outfitA.tag != null">
                                [{{popper.outfitA.tag}}]
                            </span>
                            {{popper.outfitA.name}}
                        </a>
                    </span>
                </span>

                <span v-else-if="popper.outfitAID != popper.outfitBID">
                    left 
                    <span v-if="popper.outfitAID == '0'">
                        no outfit
                    </span>
                    <span v-else-if="popper.outfitA != undefined">
                        <a :href="'/o/' + popper.outfitA.id">
                            <span v-if="popper.outfitA.tag != null">
                                [{{popper.outfitA.tag}}]
                            </span>
                            {{popper.outfitA.name}}
                        </a>
                    </span>

                    for

                    <span v-if="popper.outfitBID == '0'">
                        no outfit
                    </span>
                    <span v-else-if="popper.outfitB != undefined">
                        <a :href="'/o/' + popper.outfitBID">
                            <span v-if="popper.outfitB.tag != null">
                                [{{popper.outfitB.tag}}]
                            </span>
                            {{popper.outfitB.name}}
                        </a>
                    </span>
                </span>
            </div>

            <div style="max-height: 30vh; overflow: auto">
                <ul style="padding-left: 20px">
                    <li v-for="c in popper.characters" class="pr-1">
                        <a :href="'/c/' + c.id">
                            {{c.name}}
                        </a>
                    </li>
                </ul>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { Loadable, Loading } from "Loading";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsOutfit, OutfitApi } from "api/OutfitApi";
    import { CharacterApi, PsCharacter } from "api/CharacterApi";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import DateTimeInput from "components/DateTimeInput.vue";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ProgressBar from "components/ProgressBar.vue";

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

    class MembershipDifferenceWeek {
        private map: Map<string, string[]> = new Map();

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

    }

    class MembershipDifferences {
        private map: Map<number, MembershipDifferenceWeek> = new Map();

        public getWeek(n: number): MembershipDifferenceWeek {
            if (this.map.has(n) == false) {
                this.map.set(n, new MembershipDifferenceWeek());
            }

            return this.map.get(n)!;
        }

    }

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
                    nodes: [] as HNode[],
                    links: [] as HLink[],

                    map: new Map() as Map<string, HNode>,
                    diff: new MembershipDifferences() as MembershipDifferences
                },

                outfitColors: new Map() as Map<string, string>,

                popper: {
                    title: "" as string,
                    header: "" as string,
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
                this.progress.step = "parsing outfit ID";
                this.parseOutfit();

                this.progress.step = "loading outfit sessions";
                await this.getSessions();

                this.progress.step = "loading outfit data";
                await this.getOutfits();

                this.progress.step = "loading characters";
                await this.getCharacters();

                this.progress.step = "loading data";
                this.makeData(10000, 1080);

                this.progress.step = "creating graphic";
                this.d3s(10000, 1080);

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

            makeNode: function(id: string, outfitID: string, name: string, value: number, x?: number, y?: number): HNode {
                return {
                    id: id,
                    outfitID: outfitID,
                    timestamp: new Date(),
                    name: name,
                    color: ColorUtils.randomColorSingle(),
                    x: x ?? Math.random() * 1920,
                    y: y ?? Math.random() * 1080,
                    dx: 10,
                    dy: value,
                    sourceLinks: [],
                    targetLinks: [],
                    value: value
                };
            },

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

            d3s: function(width: number, height: number): void {
                console.time("do d3");

                const svg = d3.select("#d3_canvas").append("svg")
                    .attr("width", width)
                    .attr("height", height)
                    .attr("viewBox", [0, 0, width, height]);

                this.debug(`d3 stuff`);

                const zoom = d3.zoom();
                zoom.scaleExtent([1, 40])
                    .translateExtent([[-100, -100], [width + 90, height + 100]])
                    .filter((ev: any) => {
                        ev.preventDefault();
                        return (!ev.ctrlKey || ev.type == "wheel") && !ev.button;
                    })
                    .on("zoom", zoomed);

                //svg.call(zoom)//.node();

                function zoomed({ transform }: any) {
                    svg.attr("transform", transform);
                }

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
                const defs = svg.append("defs")
                    .selectAll(".link")
                    .data(this.graph.links)
                    .enter()
                    .append("linearGradient")
                    .attr("id", (d: HLink) => { return `${d.source}-${d.target}`; })
                    .attr("gradientUnits", "userSpaceOnUse")
                    .attr("x1", (d: HLink) => getNodeOrThrow(d.source).x)
                    .attr("x2", (d: HLink) => getNodeOrThrow(d.target).x);

                // add the starting color to each path
                this.debug("adding stop 0");
                defs.append("stop")
                    .attr("offset", "0%")
                    .attr("stop-color", (d: HLink) => {
                        const source: HNode = nodeMap.get(d.source)!;
                        return source.color;
                    });

                // add the ending color to each path
                this.debug("adding stop 1");
                defs.append("stop")
                    .attr("offset", "100%")
                    .attr("stop-color", (d: HLink) => {
                        const target: HNode = nodeMap.get(d.target)!;
                        return target.color;
                    });

                // build the path elements
                this.debug("building links");
                const paths = svg.append("g")
                    .selectAll(".link")
                    .data(this.graph.links)
                    .enter()
                    .append("path")
                    .attr("class", "link")
                    .attr("d", (d: HLink) => { return this.makeLinkPath(d); })
                    .attr("stroke", (d: HLink) => { return `url(#${d.source}-${d.target})`; })
                    .attr("fill", "none")
                    .style("stroke-width", (d: HLink) => { return d.dy; })
                    .style("stroke-opacity", "0.5")
                    .on("mouseup", this.pathClickListener);

                // sort them based on value, so paths that cross over each other can still be hovered over
                paths.sort((a: HLink, b: HLink) => {
                    return b.value - a.value;
                });

                // add text to each path that gives what the link is
                this.debug("building link text");
                paths.append("title")
                    .text((d: HLink) => {
                        const source: HNode = nodeMap.get(d.source)!;
                        const target: HNode = nodeMap.get(d.target)!;

                        const weekEnd: Date = new Date(source.timestamp.getTime() + this.interval);

                        return `${TimeUtils.formatNoTimezone(source.timestamp, "YYYY-MM-DD")} - ${TimeUtils.formatNoTimezone(weekEnd, "YYYY-MM-DD")}`
                            + `\n${source.name} to ${target.name}: ${d.value}`;
                    });

                // add the nodes for each week
                this.debug("building nodes");
                const node = svg.append("g")
                    .selectAll(".node")
                    .data(this.graph.nodes)
                    .enter()
                    .append("rect")
                    .attr("class", "node")
                    .attr("width", (d: HNode) => d.dx)
                    .attr("height", (d: HNode) => d.dy)
                    .attr("x", (d: HNode) => d.x)
                    .attr("y", (d: HNode) => d.y)
                    .style("fill", (d: HNode) => d.color)
                    .style("stroke", "#000")
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
                for (const l of this.graph.links) {
                    if (l.target != target.id) {
                        continue;
                    }

                    if (l.source == source.id) {
                        break;
                    }
                    this.debug(`\t\t${l.source} => ${l.target} (${l.value}/${l.dy})`);
                    targetOffset += l.dy;
                }

                let sourceOffset: number = 0;
                this.debug(`\tfinding sourceOffset for ${d.source} => ${d.target}/${d.dy}`);
                for (const l of this.graph.links) {
                    if (l.source != source.id) {
                        continue;
                    }

                    if (l.target == target.id) {
                        break;
                    }
                    this.debug(`\t\t${l.source} => ${l.target} (${l.value}/${l.dy})`);
                    sourceOffset += l.dy;
                }

                const x0 = source.x + source.dx;
                const x1 = target.x;
                const x2 = x0 * (1 - 0.5) + x1 * 0.5;
                const x3 = x0 * (1 - 0.5) + x1 * 0.5;
                const y0 = source.y + sourceOffset + (d.dy / 2);
                const y1 = target.y + targetOffset + (d.dy / 2);

                const p: string = `M ${x0}, ${y0} C ${x2}, ${y0} ${x3}, ${y1} ${x1}, ${y1}`;
                this.debug(`\tlink ${d.source} => ${d.target}: dy: ${d.dy}\n\t\tx0 ${x0} x1 ${x1} x2 ${x2} x3 ${x3} y0 ${y0} y1 ${y1} targetOffset ${targetOffset} sourceOffset ${sourceOffset} \n\t\t${p}`);

                return p;
            },

            /**
             * listener for when a path is clicked on
             * @param ev
             * @param link
             */
            pathClickListener: function(ev: any, link: HLink): void {
                if (ev.button != 0) {
                    return;
                }

                const source: HNode = this.graph.map.get(link.source)!;
                const target: HNode = this.graph.map.get(link.target)!;

                const popperDiv: HTMLElement | null = document.getElementById("popper-div");
                if (popperDiv == null) {
                    console.error(`Missing tooltip element '#popper-div'`);
                    return;
                }

                console.log(ev);
                popperDiv.style.display = "block";
                popperDiv.style.left = `${ev.clientX}px`;
                popperDiv.style.top = `${ev.clientY}px`;

                /*
                if (ev.clientY > (window.innerHeight / 2)) {
                } else {
                    popperDiv.style.bottom = `${ev.clientY}px`;
                }
                */

                const charIDs: string[] = this.graph.diff.getWeek(target.timestamp.getTime()).getCharacters(source.outfitID, target.outfitID);

                this.popper.characters = charIDs.map((charID: string) => {
                    return this.charMap.get(charID);
                }).filter((iter: PsCharacter | undefined) => iter != undefined).map(iter => iter!);

                this.popper.characters.sort((a, b) => a.name.localeCompare(b.name));

                this.popper.title = `Between ${TimeUtils.formatNoTimezone(source.timestamp, "YYYY-MM-DD")} and ${TimeUtils.formatNoTimezone(target.timestamp, "YYYY-MM-DD")}`;

                this.popper.outfitAID = source.outfitID;
                this.popper.outfitA = this.outfitMap.get(source.outfitID);
                this.popper.outfitBID = target.outfitID;
                this.popper.outfitB = this.outfitMap.get(target.outfitID);
            },

            /**
             * Use the session data to make the Sankey data
             */
            makeData: function(width: number, height: number): void {
                if (this.sessions.state != "loaded") {
                    return console.warn(`sessions not loaded`);
                }

                if (this.sessions.data.length == 0) {
                    return console.warn(`no session data`);
                }

                if (this.outfits.state != "loaded") {
                    return console.warn(`outfits not loaded`);
                }

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

                const outfitIDs: string[] = this.sessions.data.map(iter => iter.outfitID ?? "0").filter((v, i, a) => a.indexOf(v) == i);
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
                        outfit.y = (count / characterCount) * height;
                        outfit.x = ((i - day.getTime()) / (now.getTime() - day.getTime())) * width;
                        outfit.dx = 10;
                        outfit.dy = outfit.value / characterCount * height;
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

                            diffMap.forEach((difference: number, prevOutfitID: string) => {
                                countDown -= difference;

                                const sourceNode: HNode | undefined = nodeMap.get(`${previousTime}-${prevOutfitID}`);
                                if (sourceNode == undefined) {
                                    console.warn(`missing ${previousTime}-${prevOutfitID}`);
                                    return;
                                }

                                const link: HLink = this.makeLink(sourceNode, outfit, difference, (difference / characterCount) * height);
                                this.graph.links.push(link);
                                this.debug(`\t\tCHANGE ${sourceNode.id} => ${outfit.id}: ${difference}`);
                            });

                            this.debug(`\t\tSTAYED ${previousTime}-${outfitID} => ${i}-${outfitID}: ${countDown}`);

                            if (countDown > 0) {
                                const thisNode: HNode | undefined = nodeMap.get(`${previousTime}-${outfitID}`);
                                if (thisNode != undefined) {
                                    const link: HLink = this.makeLink(thisNode, outfit, countDown, (countDown / characterCount) * height);
                                    this.graph.links.push(link);
                                }
                            }
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
                this.sessions = Loadable.loading();

                const all: Session[] = [];

                const s: Loading<Session[]> = await SessionApi.getByOutfit(this.outfitID);
                if (s.state != "loaded") {
                    return;
                }
                this.progress.initial = true;

                console.time("get sessions");

                console.log(`${s.data.length} from outfit`);

                all.push(...s.data);

                const charIDs: string[] = s.data.map(iter => iter.characterID).filter((v, i, a) => a.indexOf(v) == i);
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

                const map: Map<number, Session> = new Map();
                for (const session of all) {
                    map.set(session.id, session);
                }

                this.sessions = Loadable.loaded(Array.from(map.values()));

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

                const outfitIDs: string[] = this.sessions.data.filter(iter => iter.outfitID != null)
                    .map(iter => iter.outfitID!)
                    .filter((v, i, a) => a.indexOf(v) == i);

                this.outfits = Loadable.loading();
                this.outfits = await OutfitApi.getByIDs(outfitIDs);

                if (this.outfits.state == "loaded") {
                    this.outfitColors.clear();
                    const colors: string[] = ColorUtils.randomColors(Math.random(), this.outfits.data.length);

                    for (let i = 0; i < this.outfits.data.length; ++i) {
                        const outfitID: string = this.outfits.data[i].id;
                        this.outfitColors.set(outfitID, colors[i]);
                    }

                    for (const outfit of this.outfits.data) {
                        this.outfitMap.set(outfit.id, outfit);
                    }
                }

                console.timeEnd("get outfits");
            },

            getCharacters: async function(): Promise<void> {
                if (this.sessions.state != "loaded") {
                    return console.warn(`cannot get characters, sessions is ${this.sessions.state} is not 'loaded'`);
                }

                console.time("get characters");

                const charIDs: string[] = this.sessions.data.map(iter => iter.characterID)
                    .filter((v, i, a) => a.indexOf(v) == i);

                this.characters = Loadable.loading();
                this.characters = await CharacterApi.getByIDs(charIDs);

                if (this.characters.state == "loaded") {
                    this.charMap.clear();
                    for (const c of this.characters.data) {
                        this.charMap.set(c.id, c);
                    }
                }

                console.timeEnd("get characters");
            },

            closePopper: function (): void {
                const tooltip: HTMLElement | null = document.getElementById("popper-div");
                if (tooltip != null) {
                    tooltip.style.display = "none";
                }
            },

        },

        components: {
            DateTimeInput, InfoHover, Busy,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ProgressBar
        }
    });
    export default OutfitSankey;
</script>