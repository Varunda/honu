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
            <h1 class="wt-header border-0">Queues</h1>

            <table class="table table-striped">
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

            <h1 class="wt-header border-0">Realtime - Death</h1>

            <table class="table">
                <tr class="table-secondary">
                    <th>World</th>
                    <th>Time ago</th>
                    <th>Last event</th>
                    <th>Failure count</th>
                </tr>

                <tr v-for="entry in health.data.death">
                    <td>{{entry.worldID}} / {{entry.worldID | world}}</td>
                    <td>{{entry.lastEvent | timeAgo}}</td>
                    <td>{{entry.lastEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
                    <td>{{entry.failureCount}}</td>
                </tr>
            </table>

            <h1 class="wt-header border-0">Realtime - Exp</h1>

            <table class="table">
                <tr class="table-secondary">
                    <th>World</th>
                    <th>Time ago</th>
                    <th>Last event</th>
                    <th>Failure count</th>
                </tr>

                <tr v-for="entry in health.data.exp">
                    <td>{{entry.worldID}} / {{entry.worldID | world}}</td>
                    <td>{{entry.lastEvent | timeAgo}}</td>
                    <td>{{entry.lastEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
                    <td>{{entry.failureCount}}</td>
                </tr>
            </table>
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

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuHomepage, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    export const Health = Vue.extend({
        props: {

        },

        data: function() {
            return {
                health: Loadable.idle() as Loading<HonuHealth>,
                latestUpdate: null as | Date | null,

                timerID: undefined as number | undefined
            }
        },

        created: function(): void {
            document.title = `Honu / Health`;

            this.timerID = setInterval(async () => {
                await this.updateHealth();
            }, 1000) as unknown as number;

            // Force an update in a separate interval from the API update to ensure the time ago
            //      displays are updated. This is useful if you have a poor connection, or nothing
            //      is getting updated for another reason
            setInterval(() => {
                this.$forceUpdate();
            }, 1000);
        },

        methods: {
            updateHealth: async function(): Promise<void> {
                this.health = await HonuHealthApi.getHealth();
                if (this.health.state == "loaded") {
                    this.latestUpdate = new Date();
                }
            }
        },

        components: {
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });
    export default Health;
</script>