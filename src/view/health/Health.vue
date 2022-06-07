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

                    <table class="table table-striped table-sm">
                        <thead>
                            <tr class="table-secondary">
                                <th>Queue</th>
                                <th>Length</th>
                                <th>Average process time</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr v-for="queue in health.data.queues">
                                <td>{{queue.queueName}}</td>
                                <td>{{queue.count}}</td>
                                <td>
                                    <span v-if="queue.average">
                                        {{queue.average}}ms
                                    </span>
                                    <span v-else>
                                        not tracked
                                    </span>
                                </td>
                            </tr>
                        </tbody>    
                    </table>
                </div>
            </div>

            <!--
            <div style="width: 100%; height: 15vh;">
                <canvas id="event-queue-canvas" class="w-100 h-100"></canvas>
            </div>
            -->

            <div class="row">
                <div class="col-lg-6 col-md-12">
                    <h1 class="wt-header">Realtime - Death</h1>

                    <table class="table table-striped table-sm">
                        <tr class="table-secondary">
                            <th>World</th>
                            <td>
                                Stream start
                                <info-hover text="When the last connection was made"></info-hover>
                            </td>
                            <th>Time ago</th>
                            <th>Last event</th>
                            <th>
                                Event count (per min)
                                <info-hover text="How many events from this world since the last failure"></info-hover>
                            </th>
                            <th>Failed checks</th>
                        </tr>

                        <tr v-for="entry in health.data.death">
                            <td>{{entry.worldID}} / {{entry.worldID | world}}</td>
                            <td>{{entry.firstEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
                            <td>{{entry.lastEvent | timeAgo}}</td>
                            <td>{{entry.lastEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
                            <td>
                                {{entry.eventCount | locale}}
                                <span v-if="entry.eventCount == 0">
                                    (--)
                                </span>
                                <span v-else-if="entry.firstEvent != null">
                                    ({{entry.eventCount / (entry.lastEvent.getTime() - entry.firstEvent.getTime()) * 1000 | locale(2)}})
                                </span>
                            </td>
                            <td>{{entry.failureCount}}</td>
                        </tr>
                    </table>
                </div>

                <div class="col-lg-6 col-md-12">
                    <h1 class="wt-header">Realtime - Exp</h1>

                    <table class="table table-striped table-sm">
                        <tr class="table-secondary">
                            <th>World</th>
                            <td>
                                Stream start
                                <info-hover text="When the last connection was made"></info-hover>
                            </td>
                            <th>Time ago</th>
                            <th>Last event</th>
                            <th>
                                Event count (per min)
                                <info-hover text="How many events from this world since the last failure"></info-hover>
                            </th>
                            <th>Failed checks</th>
                        </tr>

                        <tr v-for="entry in health.data.exp">
                            <td>{{entry.worldID}} / {{entry.worldID | world}}</td>
                            <td>{{entry.firstEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
                            <td>{{entry.lastEvent | timeAgo}}</td>
                            <td>{{entry.lastEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
                            <td>
                                {{entry.eventCount | locale}}
                                <span v-if="entry.eventCount == 0">
                                    (--)
                                </span>
                                <span v-else-if="entry.firstEvent != null">
                                    ({{entry.eventCount / (entry.lastEvent.getTime() - entry.firstEvent.getTime()) * 1000 | locale(2)}})
                                </span>
                            </td>
                            <td>{{entry.failureCount}}</td>
                        </tr>
                    </table>
                </div>
            </div>

            <div>
                <h2 class="wt-header mb-1">
                    Realtime Reconnects
                    <info-hover text="Only 50 reconnects are kept in memory"></info-hover>
                </h2>

                <div class="mb-2">
                    <div>
                        Have reconnected {{realtime24hCount}}<span v-if="realtime24hCount == 50">+</span> times in the last 24 hours
                    </div>
                    <div>
                        Have reconnected {{realtime1hCount}} times in the last hour
                    </div>
                    <div>
                        Have reconnected {{realtime5mCount}} times in the last 5 minutes
                    </div>
                </div>

                <div style="max-height: max(300px, 20vh; overflow: auto;" class="mb-3">
                    <table class="table w-100 table-sticky-header table-sm table-striped">
                        <thead>
                            <tr class="table-secondary border-top-0">
                                <th class="bg-secondary">When</th>
                                <th class="bg-secondary">What</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr v-if="health.data.realtimeHealthFailures.length == 0">
                                <td colspan="2">
                                    <span class="text-muted">
                                        No failures recorded yet. Did Honu just restart? Failures are only kept in memory and are lost on restart
                                    </span>
                                </td>
                            </tr>
                            <tr v-for="fail in health.data.realtimeHealthFailures">
                                <td>
                                    {{fail.when | moment("YYYY-MM-DD hh:mm:ss A")}}
                                </td>

                                <td>
                                    {{fail.what}}
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

                eventQueue: [] as EventQueueEntry[],

                c: null as CanvasRenderingContext2D | null,

                timerID: undefined as number | undefined
            }
        },

        created: function(): void {
            document.title = `Honu / Health`;

            this.updateHealth();
            this.timerID = setInterval(async () => {
                await this.updateHealth();
            }, 1000) as unknown as number;

            /*
            setInterval(() => {
                this.renderEventGraph();
            }, 1000);
            */

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
            setupGraph: function(): void {
                if (document.getElementById("event-queue-canvas") == null) {
                    return;
                }

                const elem: HTMLCanvasElement = document.getElementById("event-queue-canvas") as HTMLCanvasElement;

                elem.height = elem.parentElement!.clientHeight;
                elem.width = elem.parentElement!.clientWidth;

                this.c = elem.getContext("2d");
            },

            updateHealth: async function(): Promise<void> {
                this.health = await HonuHealthApi.getHealth();
                if (this.health.state == "loaded") {
                    if (this.c == null) {
                        this.setupGraph();
                    }

                    this.health.data.realtimeHealthFailures.sort((a, b) => b.when.getTime() - a.when.getTime());

                    this.eventQueue.unshift({
                        when: new Date(),
                        count: this.health.data.queues.find(iter => iter.queueName == "task_queue")?.count ?? 0
                    });

                    if (this.eventQueue.length > 50) {
                        this.eventQueue.pop();
                    }

                    this.latestUpdate = new Date();
                }
            },

            /*
            renderEventGraph: function(): void {
                if (this.c == null) {
                    return;
                }

                const h: number = this.c.canvas.getBoundingClientRect().height;
                const w: number = this.c.canvas.getBoundingClientRect().width / 50;

                console.log(h, w);

                this.c.clearRect(0, 0, w, h);

                const max: number = Math.max(...this.eventQueue.map(iter => iter.count));

                for (let i = 0; i < this.eventQueue.length; ++i) {
                    const iter: EventQueueEntry = this.eventQueue[i];

                    const p: number = iter.count / max * h;

                    this.c.fillRect(i * w, h - (2 * p), w, p * h);
                }
            }
            */
        },

        computed: {
            realtime24hCount: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const cutoff: number = (new Date().getTime()) - (1000 * 60 * 60 * 24);

                return this.health.data.realtimeHealthFailures.filter(iter => iter.when.getTime() > cutoff).length;
            },

            realtime1hCount: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const cutoff: number = (new Date().getTime()) - (1000 * 60 * 60);

                return this.health.data.realtimeHealthFailures.filter(iter => iter.when.getTime() > cutoff).length;
            },

            realtime5mCount: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const cutoff: number = (new Date().getTime()) - (1000 * 60 * 5);

                return this.health.data.realtimeHealthFailures.filter(iter => iter.when.getTime() > cutoff).length;
            }

        },

        components: {
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            InfoHover
        }
    });
    export default Health;
</script>