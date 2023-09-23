<template>
    <div>
        <h1>
            <span @click="debug = !debug">
                Fights
            </span>
            <info-hover text="What fights are currently happening on this server. There will always be some delay due to how this data is collected, and is not guaranteed to be accurate"></info-hover>
        </h1>

        <div v-if="history.showUI == true">
            <h3>
                Fight history for

                <span v-if="history.facility != null">
                    {{history.facility.name}}
                </span>
                <span v-else>
                    &gt;unknown&lt;
                </span>

                <span @click="history.showUI = false">
                    &times;
                </span>
            </h3>

            <div v-if="history.data.state == 'loading'" style="height: 300px;">
                <busy class="honu-busy"></busy>
            </div>

            <div v-else-if="history.data.state == 'loaded'">
                <canvas id="history-chart" style="width: 100%; height: 300px;"></canvas>

                <div class="text-muted">
                    This data is NOT guaranteed to be 100% accurate!
                </div>
                hey!
            </div>

            <div v-else-if="history.data.state == 'error'">
                errored: {{history.data.error}}
            </div>
        </div>

        <div class="row mx-0">
            <div class="mb-2 px-1 col-12 col-lg-6 col-xl-4" v-for="fight in fights">
                <div class="border rounded" :style="{ 'background-color': 'var(--color-dark-bg-' + fight.mapState.owningFactionID + ')'  }">
                    <div @click="getFightHistory(fight.mapState.regionID)">
                        <h2 class="px-1 d-inline-block">
                            <span style="width: 2rem; line-height: 2.5rem;">
                                <faction-image :faction-id="fight.mapState.owningFactionID" style="width: 2rem;"></faction-image>
                            </span>

                            <span v-if="fight.facility != null">
                                {{fight.facility.name}}
                            </span>
                            <span v-else>
                                &lt;unknown region {{fight.mapState.regionID}}&gt;
                            </span>

                            <span v-if="debug">
                                / {{fight.mapState.regionID}}
                            </span>
                        </h2>
                        <h5 v-if="fight.facility != null" class="d-inline-block">
                            ({{fight.facility.typeName}})
                        </h5>
                    </div>

                    <div class="mb-1 px-1">
                        <span v-if="fight.mapState.captureFlagsCount > 0">
                            {{fight.mapState.captureFlagsLeft}} / {{fight.mapState.captureFlagsCount}} flags left
                        </span>
                        <span v-else>
                            <span v-if="fight.mapState.captureTimeLeftMs == -1 && fight.mapState.captureTimeMs == -1">
                                (no points touched)
                            </span>
                            <span v-else>
                                <span v-if="fight.mapState.captureTimeLeftMs == -1">
                                    {{fight.mapState.captureTimeMs / 1000 | mduration}}
                                </span>
                                <span v-else>
                                    {{fight.mapState.captureTimeLeftMs / 1000 | mduration}}
                                </span>

                                /
                                {{fight.mapState.captureTimeMs / 1000 | mduration}}
                            </span>
                        </span>

                        --

                        as of {{fight.mapState.timestamp | timeAgo}} ago
                    </div>

                    <div class="d-flex w-100 text-center border-top" style="height: 2rem; font-size: 1rem; line-height: 2rem;">
                        <span v-if="fight.mapState.factionPercentage.vs > 0" :style="{ 'flex-grow': fight.mapState.factionPercentage.vs }" style="background-color: var(--color-vs)">
                            <faction-bounds :number="fight.mapState.factionBounds.vs" :percent="fight.mapState.factionPercentage.vs"></faction-bounds>
                        </span>
                        <span v-if="fight.mapState.factionPercentage.nc > 0" :style="{ 'flex-grow': fight.mapState.factionPercentage.nc }" style="background-color: var(--color-nc)">
                            <faction-bounds :number="fight.mapState.factionBounds.nc" :percent="fight.mapState.factionPercentage.nc"></faction-bounds>
                        </span>
                        <span v-if="fight.mapState.factionPercentage.tr > 0" :style="{ 'flex-grow': fight.mapState.factionPercentage.tr }" style="background-color: var(--color-tr)">
                            <faction-bounds :number="fight.mapState.factionBounds.tr" :percent="fight.mapState.factionPercentage.tr"></faction-bounds>
                        </span>
                    </div>
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { RealtimeMapState, RealtimeMapStateApi } from "api/RealtimeMapStateApi";
    import { PsFacility } from "api/MapApi";

    import InfoHover from "components/InfoHover.vue";
    import FactionImage from "components/FactionImage";
    import Busy from "components/Busy.vue";

    import "filters/TimeAgoFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";
    import "node_modules/chartjs-adapter-moment/dist/chartjs-adapter-moment.esm.js";

    const LowerBound = Vue.extend({
        props: {
            number: { type: Number, required: true }
        },

        template: `
            <span>
                <span v-if="number == 0">
                    0
                </span>
                <span v-else-if="number == 12">
                    1
                </span>
                <span v-else-if="number == 24">
                    12
                </span>
                <span v-else-if="number == 48">
                    24
                </span>
                <span v-else-if="number == 96">
                    48
                </span>
                <span v-else-if="number == 192">
                    96
                </span>
                <span v-else>
                    {{number}}!
                </span>
            </span>
        `
    });

    const FactionBounds = Vue.extend({
        props: {
            number: { type: Number, required: true },
            percent: { type: Number, required: true }
        },

        template: `
            <span>
                <span v-if="number == 0">
                    0
                </span>
                <span v-else-if="number == 192">
                    96+
                </span>
                <span v-else>
                    <lower-bound :number="number"></lower-bound> - {{number}}
                </span>
                <span>
                    ({{percent | locale(0)}}%)
                </span>
            </span>
        `,

        components: {
            LowerBound
        }
    });

    class RealtimeMapCount {
        public vs: number = 0;
        public nc: number = 0;
        public tr: number = 0;
    }

    export const FightData = Vue.extend({
        props: {
            fights: { type: Array, required: true },
            WorldId: { type: Number, required: true }
        },

        data: function () {
            return {
                debug: false as boolean,

                history: {
                    showUI: false as boolean,
                    data: Loadable.idle() as Loading<RealtimeMapState[]>,
                    chart: null as Chart | null,
                    count: [] as RealtimeMapCount[],
                    facility: null as PsFacility | null
                },
            }
        },

        methods: {
            getFightHistory: async function(regionID: number): Promise<void> {

                // TODO: please strongly type this
                this.history.facility = null;
                for (const iter of (this.fights as any[])) {
                    if (iter.facility != null && iter.facility.regionID == regionID) {
                        this.history.facility = iter.facility;
                        break;
                    }
                }

                this.history.showUI = true;
                this.history.data = Loadable.loading();
                this.history.data = await RealtimeMapStateApi.getHistorical(this.WorldId, regionID);

                if (this.history.data.state == "loaded") {
                    await this.makeGraph();
                }
            },

            makeGraph: async function(): Promise<void> {
                if (this.history.data.state != "loaded") {
                    console.error(`cannot make graph, history.data is ${this.history.data.state} not 'loaded'`);
                    return;
                }

                if (this.history.chart != null) {
                    this.history.chart.destroy();
                }
                this.history.chart = null;

                await this.$nextTick();
                const canvas: any = document.getElementById("history-chart");

                this.history.chart = new Chart(canvas.getContext("2d"), {
                    type: "line",
                    data: {
                        labels: [],
                        datasets: []
                    },
                    options: {
                        plugins: {
                            tooltip: {
                                mode: "index",
                                intersect: false
                            }
                        },
                        responsive: false,
                        maintainAspectRatio: false,
                        scales: {
                            x: {
                                type: "time",
                                /*
                                ticks: {
                                    maxRotation: 90,
                                    minRotation: 90,
                                    callback: function(label, index, ticks) {
                                        return TimeUtils.format(label, "hh:mm:ss A");
                                    }
                                }
                                */
                            }
                        }
                    }
                });

                this.history.chart.data.labels = this.history.data.data.map(iter => iter.timestamp);
                //this.history.chart.data.labels = this.history.data.data.map(iter => "");

                this.history.count = [];

                for (const datum of this.history.data.data) {
                    const bounds: number = datum.factionBounds.vs + datum.factionBounds.nc + datum.factionBounds.tr;

                    this.history.count.push({
                        vs: Math.round((datum.factionPercentage.vs / 100) * bounds),
                        nc: Math.round((datum.factionPercentage.nc / 100) * bounds),
                        tr: Math.round((datum.factionPercentage.tr / 100) * bounds),
                    });
                }

                this.history.chart.data.datasets.length = 0;
                this.history.chart.data.datasets.push({
                    data: this.history.count.map(iter => iter.vs),
                    //label: this.history.data.data.map(iter => TimeUtils.format(iter.timestamp, "hh:mm:ss A")),
                    label: "VS",
                    borderColor: "purple"
                });
                this.history.chart.data.datasets.push({
                    data: this.history.count.map(iter => iter.nc),
                    //label: this.history.data.data.map(iter => TimeUtils.format(iter.timestamp, "hh:mm:ss A")),
                    label: "NC",
                    borderColor: "blue"
                });
                this.history.chart.data.datasets.push({
                    data: this.history.count.map(iter => iter.tr),
                    //label: this.history.data.data.map(iter => TimeUtils.format(iter.timestamp, "hh:mm:ss A")),
                    label: "TR",
                    borderColor: "red"
                });

                this.history.chart.update();
            }
        },

        components: {
            InfoHover, LowerBound, FactionBounds, FactionImage, Busy
        }
    });
    export default FightData;
</script>
