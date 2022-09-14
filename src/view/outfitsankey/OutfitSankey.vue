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

        <div id="canvas-parent">
            <canvas id="outfit-sankey"></canvas>
        </div>

        <div id="chart">

        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    /// @ts-ignore
    import { SankeyController, Flow } from "node_modules/chartjs-chart-sankey/dist/chartjs-chart-sankey.esm.js";
    import Chart, { LegendItem } from "chart.js/auto/auto.esm";
    Chart.register(SankeyController, Flow);

    import { Loadable, Loading } from "Loading";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsOutfit, OutfitApi } from "api/OutfitApi";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import DateTimeInput from "components/DateTimeInput.vue";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ProgressBar from "components/ProgressBar.vue";

    import TimeUtils from "util/Time";

    /// @ts-ignore
    import * as Plotly from "plotly.js/dist/plotly";

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
                chart: null as Chart | null,

                outfit: Loadable.idle() as Loading<PsOutfit>,
                sessions: Loadable.idle() as Loading<Session[]>,
                outfits: Loadable.idle() as Loading<PsOutfit[]>,

                progress: {
                    initial: false as boolean,
                    current: 0 as number,
                    total: 0 as number
                },

                p: [] as PD[],

                data: [] as DataEntry[]
            }
        },

        mounted: function(): void {
            this.$nextTick(async () => {
                await this.getSessions();
                await this.getOutfits();
            });
        },

        methods: {
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

            makeGraph: function(): void {
                if (this.chart != null) {
                    this.chart.destroy();
                    this.chart = null;
                }

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

                const interval: number = 1000 * 60 * 60 * 24 * 7; // 1000 ms, 60 seconds, 60 minutes, 24 hours

                const c: Map<string, string> = new Map();
                const labels: any = {};

                for (const session of this.sessions.data) {
                    if (c.has(session.characterID) == true) {
                        continue;
                    }
                    c.set(session.characterID, session.outfitID ?? "0");
                }

                let ii: number = 0;

                for (let i = day.getTime(); i <= now.getTime(); i += interval) {
                    const slice: Session[] = this.sessions.data.filter((iter: Session) => {
                        const time: number = iter.start.getTime();
                        return time >= i && time <= (i + interval);
                    });

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

                        const toDate: string = TimeUtils.format(new Date(i + interval), "YYYY-MM-DD");
                        const fromDate: string = TimeUtils.format(new Date(i), "YYYY-MM-DD");

                        const to: string = `${currentOutfitID}-${toDate}`;
                        const from: string = `${previousOutfitID}-${fromDate}`;

                        labels[to] = currentOutfitID == "0" ? "No outfit" : (this.outfits.data.find(iter => iter.id == currentOutfitID)?.name ?? `missing ${currentOutfitID}`);
                        labels[from] = previousOutfitID == "0" ? "No outfit" : (this.outfits.data.find(iter => iter.id == previousOutfitID)?.name ?? `missing ${previousOutfitID}`);

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

                    ++ii;
                    this.p.push(...entries);

                    /*
                    const entries: DataEntry[] = [];
                    const curr: Map<string, string> = new Map();

                    console.log(`getting sessions on ${new Date(i)} ${slice.length}`);

                    for (const session of slice) {
                        const outfitID: string = session.outfitID ?? "0";
                        curr.set(session.characterID, outfitID);
                    }

                    for (const kvp of c.entries()) {
                        const charID: string = kvp[0];

                        const previousOutfitID: string = kvp[1];
                        const currentOutfitID: string = curr.get(charID) ?? previousOutfitID;

                        const toDate: string = TimeUtils.format(new Date(i + interval), "YYYY-MM-DD");
                        const fromDate: string = TimeUtils.format(new Date(i), "YYYY-MM-DD");

                        const to: string = `${currentOutfitID}-${toDate}`;
                        const from: string = `${previousOutfitID}-${fromDate}`;

                        labels[to] = currentOutfitID == "0" ? "No outfit" : (this.outfits.data.find(iter => iter.id == currentOutfitID)?.name ?? `missing ${currentOutfitID}`);
                        labels[from] = previousOutfitID == "0" ? "No outfit" : (this.outfits.data.find(iter => iter.id == previousOutfitID)?.name ?? `missing ${previousOutfitID}`);

                        let entry: DataEntry | undefined = entries.find(iter => iter.to == to && iter.from == from);
                        if (entry == undefined) {
                            //console.log(`new entry ${from} => ${to}`);
                            entry = {
                                to: to,
                                from: from,
                                flow: 0
                            };
                            entries.push(entry);
                        }

                        ++entry.flow;
                        //console.log(`${from} => ${to} :: ${entry.flow}`);
                    }

                    for (const session of slice) {
                        c.set(session.characterID, session.outfitID ?? "0");
                    }

                    this.data.push(...entries);

                    ++ii;
                    */
                }

                console.log(`first session at ${firstSession.start}`);

                const getOutfitIndex = (outfitID: string): number => {
                    if (this.outfits.state != "loaded") {
                        return 0;
                    }
                    return this.outfits.data.findIndex(iter => iter.id == outfitID) + 1;
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

                const outfitCount: number = this.outfits.data.length;

                const flows: PDE[] = [];
                for (const pd of this.p) {
                    flows.push(...pd.flow);
                }

                const getIndex = (outfitID: string, timestamp: number): number => {
                    return this.p.findIndex(i => i.outfitID == outfitID && i.timestamp == timestamp) ?? 0;
                };

                const data = [{
                    type: "sankey",
                    arragement: "fixed",
                    node: {
                        label: this.p.map(iter => getOutfitLabel(iter.outfitID)),
                        y: this.p.map((value) => getOutfitIndex(value.outfitID) / outfitCount),
                        x: this.p.map((value) => (value.timestamp - day.getTime()) / (now.getTime() - day.getTime())),
                        pad: 10
                    },
                    link: {
                        source: flows.map(iter => getIndex(iter.from, iter.timestamp)),
                        target: flows.map(iter => getIndex(iter.to, iter.timestamp + interval)),
                        value: flows.map(iter => iter.members.length)
                    }
                }];

                console.log(data);

                Plotly.newPlot(document.getElementById("chart")!, data);

                /*
                const canvas: HTMLCanvasElement | null = document.getElementById("outfit-sankey") as HTMLCanvasElement | null;
                if (canvas == null) {
                    throw `missing canvas`;
                }

                document.getElementById("canvas-parent")!.style.width = `${ii * 60}px`;
                document.getElementById("canvas-parent")!.style.height = `${c.size * 20}px`;
                canvas.width = ii * 60;
                canvas.height = c.size * 20;

                this.chart = new Chart(canvas.getContext("2d")!, {
                    type: "sankey" as any,
                    data: {
                        datasets: [
                            {
                                data: [
                                    ...this.data
                                ] as DataEntry[],
                                labels: labels,
                                colorMode: "from",
                                size: "max"
                            } as any
                        ]
                    },
                    options: {
                        maintainAspectRatio: false,
                        tooltips: {
                            callbacks: {
                                title() {
                                    return "penis";
                                },
                                label() {
                                    return "";
                                }
                            }
                        }
                    }
                });
                */
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