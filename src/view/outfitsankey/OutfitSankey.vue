<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <a href="/outfitsankey">Sankey</a>
                </li>
            </honu-menu>
        </div>

        <div>
            <button :disabled="sessions.state != 'loaded' || outfits.state != 'loaded'" class="btn btn-success" @click="makeGraph">
                Make graph
            </button>
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

        <div id="d3_canvas"></div>

        <div id="chart" style="width: 100%; height: 100vh;">

        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { Loadable, Loading } from "Loading";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsOutfit, OutfitApi } from "api/OutfitApi";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import DateTimeInput from "components/DateTimeInput.vue";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ProgressBar from "components/ProgressBar.vue";

    import TimeUtils from "util/Time";

    import * as d3s from "d3-sankey";
    import * as d3 from "d3";

    /// @ts-ignore
    import * as Plotly from "plotly.js/dist/plotly";
    import ColorUtils from "util/Color";

    type DataEntry = {
        from: string;
        to: string;
        flow: number;
    }

    type PDE = {
        from: string;
        to: string;
        timestamp: number;
        members: string[];
    };

    type PD = {
        outfitID: string;
        timestamp: number;
        members: string[];
        flow: PDE[];
    };

    export const OutfitSankey = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfit: Loadable.idle() as Loading<PsOutfit>,
                sessions: Loadable.idle() as Loading<Session[]>,
                outfits: Loadable.idle() as Loading<PsOutfit[]>,

                interval: 1000 * 60 * 60 * 24 * 7 as number, // 1000 ms, 60 seconds, 60 minutes, 24 hours, 7 days

                progress: {
                    initial: false as boolean,
                    current: 0 as number,
                    total: 0 as number
                },

                plotly: {} as any,

                p: [] as PD[],

                data: [] as DataEntry[]
            }
        },

        created: function(): void {
            document.title = "Honu / Sankey";
        },

        mounted: function(): void {
            this.$nextTick(async () => {
                this.d3s();
                /*
                await this.getSessions();
                await this.getOutfits();
                this.makeData();
                this.makeGraph();
                */
            });
        },

        methods: {
            d3s: function(): void {
                const svg = d3.select("#d3_canvas").append("svg")
                    .attr("width", 1920)
                    .attr("height", 1080)
                    .append("g")
                    .attr("transform", `translate(0, 0)`);

                const s = d3s.sankey()
                    .nodeWidth(36)
                    .nodePadding(290)
                    .size([1920, 1080]);

                console.log(`d3 stuff`);

                d3.json("https://raw.githubusercontent.com/holtzy/D3-graph-gallery/master/DATA/data_sankey.json", function(err, graph) {
                    console.log(`over here!`);
                    debugger;
                    if (err) {
                        console.error(err);
                    }

                    console.log(`over here!`);

                    s.nodes(graph.nodes).links(graph.links);
                });
            },

            /**
             * Get all the sessions relevant to load the outfit's sankey
             */
            getSessions: async function(): Promise<void> {
                this.sessions = Loadable.loading();

                const all: Session[] = [];

                const s: Loading<Session[]> = await SessionApi.getByOutfit("37567362753122235");
                if (s.state != "loaded") {
                    return;
                }
                this.progress.initial = true;

                console.log(`${s.data.length} from outfit`);

                all.push(...s.data);

                const charIDs: string[] = s.data.map(iter => iter.characterID).filter((v, i, a) => a.indexOf(v) == i);
                this.progress.total = charIDs.length;

                for (const id of charIDs) {
                    const charSessions: Loading<Session[]> = await SessionApi.getByCharacterID(id);
                    if (charSessions.state == "loaded") {
                        all.push(...charSessions.data);
                    } else {
                        console.warn(`got state ${charSessions.state} to get`);
                    }
                    ++this.progress.current;
                }

                console.log(`${all.length} sessions loaded`);

                const map: Map<number, Session> = new Map();
                for (const session of all) {
                    map.set(session.id, session);
                }

                this.sessions = Loadable.loaded(Array.from(map.values()));
            },

            /**
             * Get all the outfits from the sessions that are loaded
             */
            getOutfits: async function(): Promise<void> {
                if (this.sessions.state != "loaded") {
                    return console.warn(`cannot get outfits, sessions is ${this.sessions.state} is not 'loaded'`);
                }

                const outfitIDs: string[] = this.sessions.data.filter(iter => iter.outfitID != null)
                    .map(iter => iter.outfitID!)
                    .filter((v, i, a) => a.indexOf(v) == i);

                this.outfits = Loadable.loading();
                this.outfits = await OutfitApi.getByIDs(outfitIDs);
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

                const firstSession: Session = this.sessions.data.sort((a, b) => a.start.getTime() - b.start.getTime())[0];
                console.log(`first session at ${firstSession.start}`);

                const day: Date = firstSession.start;
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

                for (const session of this.sessions.data) {
                    if (c.has(session.characterID) == true) {
                        continue;
                    }
                    c.set(session.characterID, session.outfitID ?? "0");
                }

                for (let i = day.getTime(); i <= now.getTime(); i += this.interval) {
                    const slice: Session[] = this.sessions.data.filter((iter: Session) => {
                        const time: number = iter.start.getTime();
                        return time >= i && time <= (i + this.interval);
                    });

                    console.log(`NEW LINE =====================================`);

                    const entries: PD[] = [];
                    const curr: Map<string, string> = new Map();

                    for (const session of slice) {
                        const outfitID: string = session.outfitID ?? "0";
                        curr.set(session.characterID, outfitID);
                    }

                    for (const kvp of c.entries()) {
                        const charID: string = kvp[0];

                        const previousOutfitID: string = kvp[1];
                        const currentOutfitID: string = curr.get(charID) ?? previousOutfitID;

                        let entry: PD | undefined = entries.find(iter => iter.outfitID == previousOutfitID && iter.timestamp == i);
                        if (entry == undefined) {
                            //console.log(`new entry ${from} => ${to}`);
                            entry = {
                                outfitID: previousOutfitID,
                                timestamp: i,
                                members: [],
                                flow: []
                            };
                            entries.push(entry);
                        }

                        entry.members.push(charID);

                        let flow: PDE | undefined = entry.flow.find(iter => iter.to == currentOutfitID);
                        if (flow == undefined) {
                            flow = {
                                from: previousOutfitID,
                                to: currentOutfitID,
                                timestamp: i,
                                members: []
                            }
                            entry.flow.push(flow);
                            console.log(`new flow for ${flow.from} => ${flow.to} @ ${new Date(i)}`);
                        }
                        flow.members.push(charID);

                        //console.log(`${from} => ${to} :: ${entry.flow}`);
                    }

                    for (const session of slice) {
                        c.set(session.characterID, session.outfitID ?? "0");
                    }

                    /*
                    for (const e of entries) {
                        console.log(e);

                        if (i == day.getTime()) {
                            this.p.push(e);
                            continue;
                        }

                        if (e.flow.length > 1) {
                            this.p.push(e);
                            continue;
                        }

                        if (e.outfitID != e.flow[0].to) {
                            this.p.push(e);
                            continue;
                        }

                        console.log(`SKIP`);
                    }
                    */

                    this.p.push(...entries);
                }
            },

            /**
             * Render the Sankey graph from Plotly
             */
            makeGraph: function(): void {
                if (this.outfits.state != "loaded") {
                    return console.warn(`cannot make graph: outfits is not loaded`);
                }
                if (this.sessions.state != "loaded") {
                    return console.warn(`cannot make graph: sessions is not loaded`);
                }

                const getOutfitIndex = (outfitID: string): number => {
                    if (this.outfits.state != "loaded") {
                        return 0;
                    }
                    return this.outfits.data.findIndex(iter => iter.id == outfitID);
                };

                const getOutfitLabel = (outfitID: string): string => {
                    if (this.outfits.state != "loaded") {
                        return "not loaded";
                    }
                    if (outfitID == "0") {
                        return "no outfit";
                    }

                    const outfit: PsOutfit | undefined = this.outfits.data.find(iter => iter.id == outfitID);
                    if (outfit == undefined) {
                        return `missing ${outfitID}`;
                    }

                    return `${(outfit.tag != null) ? `[${outfit.tag}] ` : ""}${outfit.name}`;
                }

                const flows: PDE[] = [];
                for (const pd of this.p) {
                    flows.push(...pd.flow);
                }

                const getIndex = (outfitID: string, timestamp: number): number => {
                    return this.p.findIndex(i => i.outfitID == outfitID && i.timestamp == timestamp) ?? 0;
                };

                const getY = (outfitID: string, timestamp: number): number => {
                    const thisDay: PD[] = this.p.filter(iter => iter.timestamp == timestamp)
                        .sort((a, b) => b.members.length - a.members.length || b.outfitID.localeCompare(a.outfitID));

                    //console.log(thisDay);

                    const totalMembers: number = thisDay.reduce((acc, i) => acc += i.members.length, 0);

                    //console.log(`@${timestamp} has ${thisDay.length} outfits, with ${totalMembers} total members`);

                    let acc: number = 0;
                    for (const datum of thisDay) {
                        if (datum.outfitID == outfitID) {
                            console.log(`${outfitID}@${timestamp} => ${acc / totalMembers} - ${(acc + datum.members.length) / totalMembers}`);
                            return acc / totalMembers;
                        }
                        acc += datum.members.length;
                    }

                    return 0;
                }

                const firstSession: Session = this.sessions.data.sort((a, b) => a.start.getTime() - b.start.getTime())[0];
                const day: Date = firstSession.start;
                day.setUTCHours(0);
                day.setUTCMinutes(0);
                day.setUTCSeconds(0);
                day.setUTCMilliseconds(0);

                const now: Date = new Date();
                now.setUTCHours(0);
                now.setUTCMinutes(0);
                now.setUTCSeconds(0);
                now.setUTCMilliseconds(0);

                const outfitCount: number = this.outfits.data.length;
                const colors: string[] = ColorUtils.randomColors(Math.random(), outfitCount + 1, 0.8); // +1 for no outfit

                const nodes = this.p.map((value) => {
                    const n = {
                        color: colors[getOutfitIndex(value.outfitID)],
                        x: (value.timestamp - day.getTime()) / (now.getTime() - day.getTime()),
                        y: getY(value.outfitID, value.timestamp),
                        label: getOutfitLabel(value.outfitID),
                    };

                    console.log(`${value.outfitID}@${value.timestamp} => (${n.x}, ${n.y}) :: ${n.label}`);

                    return n;
                });

                console.log(nodes);

                const data = [{
                    type: "sankey",
                    arragement: "fixed",
                    orientation: "h",
                    //hoverinfo: "none",
                    node: {
                        label: nodes.map(i => i.label),
                        x: nodes.map(i => i.x),
                        y: nodes.map(i => i.y),
                        color: nodes.map(i => i.color + "33"),
                        thickness: 20,
                        hovertemplate: "x: %{x}, y: %{y}",
                        line: {
                            width: 0
                        },
                        pad: 0
                        //pad: 10
                    },
                    link: {
                        source: flows.map(iter => getIndex(iter.from, iter.timestamp)),
                        target: flows.map(iter => getIndex(iter.to, iter.timestamp + this.interval)),
                        value: flows.map(iter => iter.members.length),
                        //color: flows.map(iter => colors[getOutfitIndex(iter.from)] + "33")
                    }
                }];

                const layout = {
                    title: "Outfit Sankey",
                    showlegend: false,
                    height: 1920
                };

                const options = {
                    scrollZoom: true
                };

                console.log(data);

                Plotly.newPlot(document.getElementById("chart")!, data, layout, options).then((huh: any) => {
                    this.plotly = huh;
                });

                d3.select("svg")
                    .attr("width", 1920).attr("height", 1080)
                    .append("g")
                    .attr("transform", `translate(10,10)`);
            }
        },

        components: {
            DateTimeInput, InfoHover, Busy,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ProgressBar
        }
    });
    export default OutfitSankey;
</script>