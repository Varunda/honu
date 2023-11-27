<template>
    <fragment class="border">
        <div class="mp-grid-server-name mp-grid-cell my-1">
            <h3>{{DisplayName}}</h3>
        </div>

        <div class="mp-grid-online mp-grid-cell">
            <div class="mp-button w-100">
                Players online:
                <template v-if="data != null">
                    {{data.playersOnline}}
                </template>
                <template v-else>
                    Loading...
                </template>
            </div>
        </div>

        <div class="mp-grid-zones mp-grid-cell">
            <div>
                Continents:
            </div>

            <div v-if="data != null">
                <button v-for="zone in openedZones" class="btn btn-sm btn-primary mr-2">
                    {{zone.zoneID | zone}}
                    <span v-if="zone.alertEnd != null">
                        <span class="fas fa-exclamation-triangle" title="Active alert!"></span>
                        {{(new Date(zone.alertEnd) - new Date()) / 1000 | duration}}
                    </span>
                </button>

                <span v-if="closedZones.length > 0" class="border-right mr-2"></span>

                <button v-for="zone in closedZones" class="btn btn-sm btn-secondary mr-2">
                    {{zone.zoneID | zone}}
                </button>
            </div>
        </div>

        <div class="mp-grid-links mp-grid-cell">
            <a :href="'/view/' + name" class="btn btn-primary">
                Realtime
                <span class="fas fa-external-link-alt"></span>
            </a>
        </div>
    </fragment>
</template>

<style>

</style>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import "filters/ZoneNameFilter";
    import "MomentFilter";

    import { WorldOverview as WorldOverviewData } from "api/WorldOverviewApi";
    import { ZoneState } from "api/ZoneStateApi";

    import { Fragment } from "vue-fragment";

    export const WorldOverview = Vue.extend({
        props: {
            name: { type: String, required: true },
            DisplayName: { type: String, required: true },
            data: { type: Object as PropType<WorldOverviewData | null>, required: false }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            openedZones: function(): ZoneState[] {
                if (this.data == null) {
                    return [];
                }
                return this.data.zones.filter(iter => iter.isOpened == true);
            },

            closedZones: function(): ZoneState[] {
                if (this.data == null) {
                    return [];
                }

                return [...this.data.zones.filter(iter => iter.isOpened == false)].sort((a, b) => {
                    return (a.lastLocked?.getTime() ?? 0) - (b.lastLocked?.getTime() ?? 0);
                });
            }
        },

        components: {
            Fragment
        }
    });
    export default WorldOverview;
</script>