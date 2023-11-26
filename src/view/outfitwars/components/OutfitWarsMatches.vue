<template>
    <div>
        <h1>
            Matches
        </h1>

        <h4>
            Matches may not be current, as they are not all released at the same time
        </h4>

        <a-table default-sort-field="timestamp" default-sort-order="asc"
            :entries="entries" display-type="table" :hover="true" :show-filters="true">

            <a-col>
                <a-header>
                    <b>Server</b>
                </a-header>

                <a-filter method="dropdown" type="number" field="worldID" :source="filterSources.world"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.worldID | world}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Outfit A</b>
                </a-header>

                <a-filter method="input" type="string" field="outfitADisplay"
                          :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <a :href="'/o/' + entry.outfitAId">
                        <faction-image :faction-id="entry.outfitAFactionId" style="max-width: 32px;"></faction-image>

                        {{entry.outfitADisplay}}
                    </a>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Outfit B</b>
                </a-header>

                <a-filter method="input" type="string" field="outfitBDisplay"
                          :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <a :href="'/o/' + entry.outfitBId">
                        <faction-image :faction-id="entry.outfitBFactionId" style="max-width: 32px;"></faction-image>

                        {{entry.outfitBDisplay}}
                    </a>
                </a-body>
            </a-col>

            <a-col sort-field="timestamp">
                <a-header>
                    <b>Timestamp</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.timestamp | moment}}
                    (in {{entry.timestamp | til2}})
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Discord timestamps</b>
                    <info-hover text="Timestamps you can copy and paste into Discord that will display it in the timezone of the viewer"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    &lt;t:{{(entry.timestamp.getTime() / 1000)}}:F&gt;
                    &lt;t:{{(entry.timestamp.getTime() / 1000)}}:R&gt;
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { FlatOutfitWarsMatch, OutfitWarsMatchApi } from "api/OutfitWarsApi";

    import "MomentFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";
    import "filters/TilFilter";

    import WorldUtils from "util/World";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ApiError from "components/ApiError";
    import FactionImage from "components/FactionImage";

    export const OutfitWarsMatches = Vue.extend({
        props: {

        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<FlatOutfitWarsMatch[]>
            }
        },

        mounted: function(): void {
            this.bind();
        },

        methods: {

            bind: async function(): Promise<void> {
                this.entries = Loadable.loading();
                this.entries = await OutfitWarsMatchApi.getAll();
            }

        },

        computed: {
            filterSources: function() {
                return {
                    faction: [
                        { value: null, key: "All" },
                        { value: 1, key: "VS" } ,
                        { value: 2, key: "NC" } ,
                        { value: 3, key: "TR" } ,
                        { value: 4, key: "NS" } ,
                    ],

                    world: [
                        { value: null, key: "All" },
                        { value: WorldUtils.Connery, key: "Connery" },
                        { value: WorldUtils.Cobalt, key: "Cobalt" },
                        { value: WorldUtils.Miller, key: "Miller" },
                        { value: WorldUtils.Emerald, key: "Emerald" },
                        { value: WorldUtils.Jaeger, key: "Jaeger" },
                        { value: WorldUtils.SolTech, key: "SolTech" }
                    ]
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover, Busy, ApiError, FactionImage
        }

    });
    export default OutfitWarsMatches;

</script>