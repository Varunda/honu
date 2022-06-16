<template>
    <table class="table table-striped table-sm">
        <tr class="table-secondary">
            <th>World</th>
            <td>
                Stream duration
                <info-hover text="How long this stream has stayed health"></info-hover>
            </td>
            <th>
                Event count (per sec)
                <info-hover text="How many events from this world since the last failure"></info-hover>
            </th>
            <th>Last event</th>
            <th>Time ago</th>
            <th>Failed checks</th>
        </tr>

        <tr v-for="entry in entries" :key="entry.id">
            <td>{{entry.worldID}} / {{entry.worldID | world}}</td>
            <td>
                <span :title="entry.firstEvent | moment('YYYY-MM-DD hh:mm:ss A')">
                    {{entry.firstEvent | timeAgo}}
                </span>
            </td>
            <td>
                {{entry.eventCount | locale}}
                <span v-if="entry.eventCount == 0">
                    (--)
                </span>
                <span v-else-if="entry.firstEvent != null">
                    ({{entry.eventCount / (entry.lastEvent.getTime() - entry.firstEvent.getTime()) * 1000 | locale(2)}})
                </span>
            </td>
            <td>{{entry.lastEvent | moment("YYYY-MM-DD hh:mm:ssA")}}</td>
            <td>{{entry.lastEvent | timeAgo}}</td>
            <td :class="{ 'table-danger': entry.failureCount > 0 }">
                {{entry.failureCount}}
            </td>
        </tr>
    </table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { CensusRealtimeHealthEntry } from "api/HonuHealthApi";

    import InfoHover from "components/InfoHover.vue";

    import "MomentFilter";
    import "filters/TimeAgoFilter";
    import "filters/WorldNameFilter";
    import "filters/LocaleFilter";

    export const StreamHealthTable = Vue.extend({
        props: {
            entries: { type: Array as PropType<CensusRealtimeHealthEntry[]>, required: true }
        },

        components: {
            InfoHover,
        }
    });
    export default StreamHealthTable;
</script>