<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Health
            </li>
        </honu-menu>

        <div>
            <h3 class="d-inline">
                Latest update -
                <span v-if="latestUpdate != null">
                    {{latestUpdate | moment("YYYY-MM-DD hh:mm:ss A")}}
                    ::
                    {{latestUpdate | timeAgo}}
                </span>
            </h3>
        </div>

        <div v-if="health.state == 'loaded'">
            <div class="row">
                <div class="col-12">
                    <h1 class="wt-header">Queues</h1>

                    <table class="table table-striped table-sm" style="table-layout: fixed;">
                        <thead>
                            <tr class="table-secondary">
                                <th>Queue</th>
                                <th>Length</th>
                                <th>Average</th>
                                <th>Median</th>
                                <th>Min</th>
                                <th>Max</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr v-for="queue in health.data.queues">
                                <td>{{queue.queueName}}</td>
                                <td>{{queue.count}}</td>

                                <td>
                                    <span v-if="queue.average != null">
                                        {{queue.average | locale(2)}}ms
                                    </span>
                                    <span v-else>
                                        --
                                    </span>
                                </td>
                                <td>
                                    <span v-if="queue.median != null">
                                        {{queue.median | locale(2)}}ms
                                    </span>
                                    <span v-else>
                                        --
                                    </span>
                                </td>
                                <td>
                                    <span v-if="queue.min != null">
                                        {{queue.min | locale(2)}}ms
                                    </span>
                                    <span v-else>
                                        --
                                    </span>
                                </td>
                                <td>
                                    <span v-if="queue.max != null">
                                        {{queue.max | locale(2)}}ms
                                    </span>
                                    <span v-else>
                                        --
                                    </span>
                                </td>
                            </tr>
                        </tbody>    
                    </table>
                </div>
            </div>

            <div class="row">
                <div class="col-lg-6 col-md-12">
                    <h1 class="wt-header">Realtime - Death</h1>

                    <stream-health-table :entries="health.data.death"></stream-health-table>
                </div>

                <div class="col-lg-6 col-md-12">
                    <h1 class="wt-header">Realtime - Exp</h1>

                    <stream-health-table :entries="health.data.exp"></stream-health-table>
                </div>
            </div>

            <div>
                <h2 class="wt-header mb-1">
                    Realtime Reconnects

                    <toggle-button v-model="settings.showGraph">
                        Show graph
                    </toggle-button>
                </h2>

                <div class="mb-2">
                    <div>
                        Have reconnected {{realtime24hCount}} times in the last 24 hours
                    </div>
                    <div>
                        Have reconnected {{realtime1hCount}} times in the last hour
                    </div>
                    <div>
                        Have reconnected {{realtime5mCount}} times in the last 5 minutes
                    </div>
                </div>

                <reconnect-graph v-if="settings.showGraph" :reconnects="health.data.reconnects"></reconnect-graph>

                <div style="max-height: max(300px, 20vh); overflow: auto;" class="mb-3">
                    <table class="table w-100 table-sticky-header table-sm table-striped">
                        <thead>
                            <tr class="table-secondary border-top-0">
                                <th class="bg-secondary">When</th>
                                <th class="bg-secondary">What</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr v-if="health.data.reconnects.length == 0">
                                <td colspan="2">
                                    <span class="text-muted">
                                        No failures in the last 24 hours
                                    </span>
                                </td>
                            </tr>
                            <tr v-for="reconnect in health.data.reconnects.slice(0, 50)" :key="reconnect.id">
                                <td>
                                    {{reconnect.timestamp | moment("YYYY-MM-DD hh:mm:ss A")}}
                                </td>
                                <td>
                                    World {{reconnect.worldID}}/{{reconnect.worldID | world}} {{reconnect.streamType}} stream {{reconnect.duration}}s without an event,
                                    failed {{reconnect.failedCount}} times before reconnect,
                                    had {{reconnect.eventCount}} events from the stream
                                </td>
                            </tr>
                            <tr v-if="health.data.reconnects.length > 50">
                                <td colspan="2">
                                    {{health.data.reconnects.length - 50}} not shown
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { HonuHealth, HonuHealthApi } from "api/HonuHealthApi";

    import "MomentFilter";
    import "filters/TimeAgoFilter";
    import "filters/WorldNameFilter";
    import "filters/LocaleFilter";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuHomepage, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";
    import StreamHealthTable from "./StreamHealthTable.vue";
    import ReconnectGraph from "./ReconnectGraph.vue";

    interface EventQueueEntry {
        when: Date;
        count: number;
    }

    export const Health = Vue.extend({
        props: {

        },

        data: function() {
            return {
                health: Loadable.idle() as Loading<HonuHealth>,
                latestUpdate: null as | Date | null,

                timerID: undefined as number | undefined,

                settings: {
                    showGraph: true as boolean
                }
            }
        },

        created: function(): void {
            document.title = `Honu / Health`;

            this.updateHealth();
            this.timerID = setInterval(async () => {
                await this.updateHealth();
            }, 1000) as unknown as number;

            // Force an update in a separate interval from the API update to ensure the time ago
            //      displays are updated. This is useful if you have a poor connection, or nothing
            //      is getting updated for another reason
            setInterval(() => {
                let needsRefresh: boolean = this.latestUpdate == null;

                if (this.latestUpdate != null) {
                    const diff: number = new Date().getTime() - this.latestUpdate.getTime();

                    // Only refresh if new data hasn't been found in 1500ms. This prevents flickering
                    //      if the two intervals are out of sync. For example, if the updateHealth interval
                    //      ran every 500ms, then this interval ran 500ms after the updateHealth interval,
                    //      this would cause the data to flicker every 500ms, very annoying
                    if (diff > 1500) {
                        console.warn(`data is ${diff}ms old, forcing a refresh`);
                        needsRefresh = true;
                    }
                }

                if (needsRefresh == true) {
                    this.$forceUpdate();
                }
            }, 1000);
        },

        methods: {
            updateHealth: async function(): Promise<void> {
                this.health = await HonuHealthApi.getHealth();
                if (this.health.state == "loaded") {
                    this.latestUpdate = new Date();
                }
            },
        },

        computed: {
            realtime24hCount: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const cutoff: number = (new Date().getTime()) - (1000 * 60 * 60 * 24);

                return this.health.data.reconnects.filter(iter => iter.timestamp.getTime() > cutoff).length;
            },

            realtime1hCount: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const cutoff: number = (new Date().getTime()) - (1000 * 60 * 60);
                return this.health.data.reconnects.filter(iter => iter.timestamp.getTime() > cutoff).length;
            },

            realtime5mCount: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const cutoff: number = (new Date().getTime()) - (1000 * 60 * 5);
                return this.health.data.reconnects.filter(iter => iter.timestamp.getTime() > cutoff).length;
            },

        },

        components: {
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            InfoHover, ToggleButton,
            StreamHealthTable,
            ReconnectGraph
        }
    });
    export default Health;
</script>
