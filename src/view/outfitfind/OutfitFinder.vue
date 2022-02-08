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

        <table class="table table-sm">
            <tr>
                <th>Tag</th>
                <th>Name</th>
                <th>Player count</th>
                <th>Faction</th>
                <th>Permalink</th>
            </tr>

            <tr v-if="outfits.state == 'idle'">
            </tr>

            <tr v-else-if="outfits.state == 'loading'">
                <td colspan="5">
                    Loading...
                </td>
            </tr>

            <tbody v-else-if="outfits.state == 'loaded'">
                <tr v-for="outfit in outfits.data">
                    <td>{{outfit.tag}}</td>
                    <td>'{{outfit.name}}'</td>
                    <td>{{outfit.memberCount}}</td>
                    <td>{{outfit.factionID | faction}}</td>
                    <td>
                        <a :href="'/o/' + outfit.id" type="button" class="btn btn-success">
                            Open
                        </a>
                    </td>
                </tr>
            </tbody>

        </table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import { PsOutfit, OutfitApi } from "api/OutfitApi";

    import "filters/FactionNameFilter";

    export const OutfitFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfits: Loadable.idle() as Loading<PsOutfit[]>,

                name: "" as string,
                searchName: "" as string,
                scrollIndex: 0 as number
            }
        },

        methods: {
            scrollOptions: function(ev: KeyboardEvent): void {
                if (this.outfits.state != "loaded") {
                    return;
                }

                if (ev.key == "ArrowUp") {
                    if (this.scrollIndex == 0) {
                        return;
                    }
                    --this.scrollIndex;
                } else if (ev.key == "ArrowDown") {
                    if (this.scrollIndex - 1 == this.outfits.data.length) {
                        return;
                    }
                    ++this.scrollIndex;
                } else {
                    return;
                }

                ev.preventDefault();
            },

            openEnter: function(ev: KeyboardEvent): void {
                if (this.outfits.state != "loaded") {
                    return;
                }

                const outfit: PsOutfit = this.outfits.data[this.scrollIndex];

                if (ev.ctrlKey == true) {
                    console.log(`opening ${outfit.id} in new tab`);
                    window.open(`/o/${outfit.id}`, "_blank");
                } else {
                    console.log(`opening ${outfit.id} in this tab`);
                    location.href = `/o/${outfit.id}`;
                }

                ev.preventDefault();
            },

            search: async function(): Promise<void> {
                this.scrollIndex = 0;
                this.searchName = this.name;
                this.outfits = Loadable.loading();
                this.outfits = await OutfitApi.searchByName(this.searchName);

                const lowerName: string = this.name.toLowerCase();

                if (this.outfits.state == "loaded") {
                    this.outfits.data = this.outfits.data.sort((a, b) => {
                        let tagOnly: boolean = this.searchName.at(0) == '[';

                        if ((!tagOnly && b.name.toLowerCase() == lowerName) || b.tag?.toLowerCase() == lowerName) {
                            return 1;
                        }
                        if ((!tagOnly && a.name.toLowerCase() == lowerName) || a.tag?.toLowerCase() == lowerName) {
                            return -1;
                        }

                        return b.memberCount - a.memberCount;
                    });
                }
            }

        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default OutfitFinder;
</script>