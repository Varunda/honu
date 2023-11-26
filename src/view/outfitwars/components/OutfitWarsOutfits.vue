<template>
    <div>
        <h1>
            List of outfits registered
        </h1>

        <h4>
            This data can take up to 5 minutes to update when changes are made in game!
        </h4>

        <div class="w-100 mb-2 border border-light p-2 rounded">
            <h5>Select server</h5>

            <div class="btn-group w-100">
                <button class="btn btn-outline-light text-white" :class="[ worldID == 13 ? 'btn-success' : 'btn-secondary' ]" @click="worldID = 13">
                    Cobalt
                </button>
                <button class="btn btn-outline-light text-white" :class="[ worldID == 1 ? 'btn-success' : 'btn-secondary' ]" @click="worldID = 1">
                    Connery
                </button>
                <button class="btn btn-outline-light text-white" :class="[ worldID == 17 ? 'btn-success' : 'btn-secondary' ]" @click="worldID = 17">
                    Emerald
                </button>
                <button class="btn btn-outline-light text-white" :class="[ worldID == 10 ? 'btn-success' : 'btn-secondary' ]" @click="worldID = 10">
                    Miller
                </button>
                <button class="btn btn-outline-light text-white" :class="[ worldID == 40 ? 'btn-success' : 'btn-secondary' ]" @click="worldID = 40">
                    SolTech
                </button>
                <button class="btn btn-outline-light text-white" :class="[ worldID == -1 ? 'btn-success' : 'btn-secondary' ]" @click="worldID = -1">
                    All
                </button>
            </div>

        </div>

        <a-table default-sort-field="signupCount" default-sort-order="desc"
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
                    <b>Outfit</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/o/' + entry.outfitID">
                        <span v-if="entry.outfit != null">
                            [{{entry.outfit.tag}}] {{entry.outfit.name}}
                        </span>
                        <span v-else>
                            &lt;unknown outfit {{entry.outfitID}}&gt;
                        </span>
                    </a>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Faction</b>
                </a-header>

                <a-filter method="dropdown" type="number" field="factionID" :source="filterSources.faction"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.factionID | faction}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Status</b>
                </a-header>

                <a-filter method="dropdown" type="string" field="status"
                          :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.status}}
                </a-body>
            </a-col>

            <a-col sort-field="signupCount">
                <a-header>
                    <b>Player count</b>
                    <info-hover text="API only returns at max 24 players. It is possible to have more!"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.signupCount >= 24" class="text-success">
                        {{entry.signupCount}} / 24
                    </span>
                    <span v-else class="text-warning">
                        {{entry.signupCount}} / 24
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="registrationOrder">
                <a-header>
                    <b>Registration order</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.registrationOrder | locale(0) }}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Member count</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.outfit == null">
                        &gt;unknown&gt;
                    </span>
                    <span v-else>
                        {{entry.outfit.memberCount | locale(0)}}
                    </span>
                </a-body>
            </a-col>
        </a-table>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { FlatOutfitWarsOutfit, OutfitWarsOutfitApi } from "api/OutfitWarsApi";

    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";

    import WorldUtils from "util/World";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Collapsible from "components/Collapsible.vue";
    import Busy from "components/Busy.vue";
    import ApiError from "components/ApiError";

    export const OutfitWarsOutfits = Vue.extend({
        props: {

        },

        data: function() {
            return {
                worldID: 1 as number,
                data: Loadable.idle() as Loading<FlatOutfitWarsOutfit[]>
            }
        },

        mounted: function(): void {
            this.bind();
        },

        methods: {
            bind: async function(): Promise<void> {
                this.data = Loadable.loading();
                this.data = await OutfitWarsOutfitApi.getAll();
            }

        },

        computed: {

            entries: function(): Loading<FlatOutfitWarsOutfit[]> {
                if (this.data.state != "loaded") {
                    return this.data;
                }

                if (this.worldID != -1) {
                    return Loadable.loaded(this.data.data.filter(iter => iter.worldID == this.worldID));
                }

                return this.data;
            },

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
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Collapsible,
            Busy,
            ApiError
        }

    });

    export default OutfitWarsOutfits;

</script>
