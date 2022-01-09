<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/ Items</span>
            </h1>
        </div>

        <a-table
            :entries="items"
            :show-filters="true"
            default-sort-field="name" default-sort-order="asc"
            display-type="table">

            <a-col sort-field="name">
                <a-header>
                    <b>Name</b>
                </a-header>

                <a-filter method="input" type="string" field="name"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <a :href="'/i/' + entry.id">
                        {{entry.name}}
                    </a>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Faction</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.factionID | faction}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Type</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.typeID}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Category</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.categoryID}}
                </a-body>
            </a-col>

        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import CensusImage from "components/CensusImage";

    import "filters/FactionNameFilter";

    import { PsItem, ItemApi } from "api/ItemApi";

    export const ItemFind = Vue.extend({
        props: {

        },

        data: function() {
            return {
                items: Loadable.idle() as Loading<PsItem[]>,

                showImages: true as boolean
            }
        },

        created: function(): void {
            document.title = `Honu / Item list`;
        },

        mounted: function(): void {
            this.bindItems();
        },

        methods: {
            bindItems: async function(): Promise<void> {
                this.items = Loadable.loading();
                this.items = await ItemApi.getAll();
            }
        },

        computed: {

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            CensusImage,
        }

    });

    export default ItemFind;
</script>