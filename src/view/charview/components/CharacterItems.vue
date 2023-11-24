<template>
    <div>

        <div class="my-1 mb-2 p-2 border rounded">
            <h5>
                Options
            </h5>

            <toggle-button v-model="showImages">
                Show images
            </toggle-button>

            <span class="border mx-2">

            </span>

            <button class="btn btn-primary mr-2" @click="showRank5Implants">
                Show rank 5 implants
            </button>

            <button class="btn btn-primary mr-2" @click="showAspPoints">
                Show ASP points
            </button>

            <button class="btn btn-secondary mr-2" @click="resetFilters">
                Reset
            </button>
        </div>

        <a-table ref="itemTable"
            :entries="items"
            :show-filters="true"
            default-sort-field="itemName" default-sort-order="asc"
            display-type="table">

            <a-col sort-field="itemName">
                <a-header>
                    <b>Item</b>
                </a-header>

                <a-filter method="input" type="string" field="itemName"
                    :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <div :style="{ height: (showImages == true) ? '3rem' : '', position: 'relative' }">
                        <census-image v-if="showImages == true && entry.item != null && entry.item.imageID && entry.item.imageID != 0" :image-id="entry.item.imageID"
                            style="position: absolute; text-align: center; height: 100%; right: 0;" class="mr-1">
                        </census-image>

                        <span :href="entry.itemID != 0 ? ('/i/' + entry.itemID) : 'javascript:void(0);'" class="ml-1"
                            style="
                                position: absolute;
                                text-shadow: -1px -1px 2px rgb(32, 32, 32), -1px 1px 2px rgb(32, 32, 32), 1px -1px 2px rgb(32, 32, 32), 1px 1px 2px rgb(32, 32, 32);
                            ">

                            {{entry.itemName}}
                            <span v-if="showDebug == true">
                                / {{entry.itemID}}
                            </span>

                            <info-hover v-if="entry.item != null" :text="entry.item.description"></info-hover>
                        </span>
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="itemID">
                <a-header>
                    <b>Item ID</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.itemID}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Category</b>
                </a-header>

                <a-filter method="dropdown" type="string" field="categoryName"
                          :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.categoryName}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Type</b>
                </a-header>

                <a-filter method="dropdown" type="string" field="typeName"
                          :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.typeName}}
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

    import ATable, { ACol, ABody, AFilter, AHeader, ATableType } from "components/ATable";
    import CensusImage from "components/CensusImage";
    import ToggleButton from "components/ToggleButton";
    import InfoHover from "components/InfoHover.vue";

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

                itemsCopy: [] as ExpandedCharacterItem[],

                showImages: true as boolean,
                showWrongFactionItems: false as boolean,
                showDebug: false as boolean
            }
        },

        mounted: function(): void {
            this.bindData();
        },

        methods: {
            bindData: async function(): Promise<void> {
                this.itemsCopy = [];

                this.items = Loadable.loading();
                this.items = await CharacterItemApi.getByID(this.character.id);

                if (this.items.state == "loaded") {
                    this.itemsCopy = this.items.data;
                }
            },

            resetFilters: function(): void {
                if (this.items.state != "loaded") {
                    console.warn(`items is not loaded, not resetting filters`);
                    return;
                }

                this.items = Loadable.loaded(this.itemsCopy);
            },

            showRank5Implants: function(): void {
                if (this.items.state != "loaded") {
                    return;
                }

                this.resetFilters();

                this.items.data = this.items.data.filter(iter => {
                    if (iter.item == null) {
                        return false;
                    }

                    return (
                        iter.item.categoryID == 133
                        && iter.item.typeID == 45
                        && (iter.item.name.indexOf("5") > -1 || iter.item.name.match("\\d") == null)
                    );
                });
            },

            showAspPoints: function(): void {
                if (this.items.state != "loaded") {
                    return;
                }

                this.resetFilters();

                this.items.data = this.items.data.filter(iter => {
                    if (iter.item == null) {
                        return false;
                    }

                    return (
                        iter.item.categoryID == 133
                        && iter.item.typeID == 1
                    );
                });
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            CensusImage, ToggleButton, InfoHover
        }
    });
    export default CharacterItems;
</script>