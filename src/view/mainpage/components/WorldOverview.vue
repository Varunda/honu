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
                <span v-for="zone in data.zones">
                    <button v-if="zone.isOpened == true" class="btn btn-sm btn-primary mr-2">
                        {{zone.zoneID | zone}}
                        <span v-if="zone.alertEnd != null">
                            <span class="fas fa-exclamation-triangle" title="Active alert!"></span>
                            {{(new Date(zone.alertEnd) - new Date()) / 1000 | duration}}
                        </span>
                    </button>
                </span>
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
    import Vue from "vue";

    import "filters/ZoneNameFilter";
    import "MomentFilter";

    import { Fragment } from "vue-fragment";

    export const WorldOverview = Vue.extend({
        props: {
            name: { type: String, required: true },
            DisplayName: { type: String, required: true },
            data: { required: false }
        },

        created: function(): void {
            document.title = `Honu / Homepage`;
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        components: {
            Fragment
        }
    });
    export default WorldOverview;
</script>