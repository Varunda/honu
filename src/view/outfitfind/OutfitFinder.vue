<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/outfitfinder">Outfits</a>
            </li>
        </honu-menu>

        <div class="input-group">
            <input v-model="name" class="form-control" @keyup.enter="search" />

            <div class="input-group-append">
                <button @click="search" type="button" class="btn btn-primary">
                    Search
                </button>
            </div>
        </div>

        <a-table :entries="outfits" default-sort-field="index" default-sort-order="asc">
            <a-col sort-field="tag">
                <a-header>
                    <b>Tag</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.tag}}
                </a-body>
            </a-col>

            <a-col sort-field="name">
                <a-header>
                    <b>Name</b>
                </a-header>

                <a-body v-slot="entry">
                    '{{entry.name}}'
                </a-body>
            </a-col>

            <a-col sort-field="memberCount">
                <a-header>
                    <b>Members</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.memberCount}}
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
                    <b>Link</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/o/' + entry.id" type="button" class="btn btn-success">
                        Open
                    </a>
                </a-body>
            </a-col>
        </a-table>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import { PsOutfit, OutfitApi } from "api/OutfitApi";

    import "filters/FactionNameFilter";

    // ordered type to prevent the <a-table> used to display this data to order the data by default
    type OutfitSearch = PsOutfit & { index: number };

    export const OutfitFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfits: Loadable.idle() as Loading<OutfitSearch[]>,

                name: "" as string,
                searchName: "" as string,
            }
        },

        methods: {
            search: async function(): Promise<void> {
                this.searchName = this.name;
                this.outfits = Loadable.loading();
                const l: Loading<PsOutfit[]> = await OutfitApi.searchByName(this.searchName);
                if (l.state != "loaded") {
                    this.outfits = Loadable.rewrap(l);
                    return;
                }

                const lowerName: string = this.name.toLowerCase();

                if (l.state == "loaded") {
                    this.outfits = Loadable.loaded(l.data.sort((a, b) => {
                        let tagOnly: boolean = this.searchName.at(0) == '[';

                        if ((!tagOnly && b.name.toLowerCase() == lowerName) || b.tag?.toLowerCase() == lowerName) {
                            return 1;
                        }
                        if ((!tagOnly && a.name.toLowerCase() == lowerName) || a.tag?.toLowerCase() == lowerName) {
                            return -1;
                        }

                        return b.memberCount - a.memberCount;
                    }).map((iter, index) => {
                        return {
                            ...iter,
                            index: index
                        };
                    }));
                }
            }

        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ATable, ACol, AHeader, ABody, AFilter
        }

    });
    export default OutfitFinder;
</script>