<template>
    <div>
        <a-table
            :entries="items"
            :show-filters="true"
            default-sort-field="itemName" default-sort-order="asc"
            display-type="table">

            <a-col>
                <a-header>
                    <b>Item</b>
                </a-header>

                <a-filter method="input" type="string" field="itemName"
                    :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.itemName}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Item ID</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.itemID}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Account unlock</b>
                </a-header>

                <a-filter method="input" type="boolean" field="accountLevel"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.accountLevel}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Stack count</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.entry.stackCount == null">
                        &lt;not stacked&gt;
                    </span>
                    <span v-else>
                        {{entry.entry.stackCount}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Census link</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'https://census.daybreakgames.com/s:example/get/ps2:v2/item?item_id=' + entry.entry.itemID" target="_blank">
                        Census
                        <span class="fas fa-external-link-alt"></span>
                    </a>
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import "filters/LocaleFilter";
    import "filters/FixedFilter";

    import { PsCharacter } from "api/CharacterApi";
    import { ExpandedCharacterItem, CharacterItemApi } from "api/CharacterItemApi";

    export const CharacterItems = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                items: Loadable.idle() as Loading<ExpandedCharacterItem[]>,
            }
        },

        mounted: function(): void {
            this.bindData();
        },

        methods: {
            bindData: async function(): Promise<void> {
                this.items = Loadable.loading();
                this.items = await CharacterItemApi.getByID(this.character.id);
            }
        },

        components: {
            ATable,
            ACol,
            AHeader,
            ABody,
            AFilter,
        }
    });
    export default CharacterItems;
</script>