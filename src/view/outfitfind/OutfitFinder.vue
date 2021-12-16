<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="#">Outfit</a>
            </h1>
        </div>

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
                <th>Links</th>
            </tr>

            <tr v-if="outfits.state == 'idle'">
            </tr>

            <tr v-else-if="outfits.state == 'loading'">
                <td colspan="4">
                    Loading...
                </td>
            </tr>

            <tbody v-else-if="outfits.state == 'loaded'">
                <tr v-for="outfit in outfits.data">
                    <td>{{outfit.tag}}</td>
                    <td>'{{outfit.name}}'</td>
                    <td>{{outfit.memberCount}}</td>
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

    import { PsOutfit, OutfitApi } from "api/OutfitApi";

    export const OutfitFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                outfits: Loadable.idle() as Loading<PsOutfit[]>,

                name: "" as string,
                searchName: "" as string
            }
        },

        methods: {
            search: async function(): Promise<void> {
                this.searchName = this.name;
                this.outfits = Loadable.loading();
                this.outfits = await OutfitApi.searchByName(this.searchName);

                if (this.outfits.state == "loaded") {
                    this.outfits.data = this.outfits.data.sort((a, b) => b.memberCount - a.memberCount);
                }
            }

        }

    });
    export default OutfitFinder;
</script>