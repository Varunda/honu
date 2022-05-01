<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Big Beans
            </li>
        </honu-menu>

        <hr class="border" />

        <div class="row">
            <div class="col-2">
                <label>World</label>
                <select class="form-control mb-2" v-model.number="worldID">
                    <option :value="13">Cobalt</option>
                    <option :value="1">Connery</option>
                    <option :value="17">Emerald</option>
                    <option :value="19">Jaeger</option>
                    <option :value="10">Miller</option>
                    <option :value="40">SolTech</option>
                </select>

                <label>
                    Time (uses local time)
                </label>
                <input v-model="time" class="form-control mb-2" type="datetime-local" />

                <div>
                    {{time}}
                </div>

                <button @click="load" type="button" class="btn btn-primary">
                    Load
                </button>
            </div>

            <div class="col-10">
                <div class="d-flex">
                    <div class="flex-grow-1">
                        <outfit-list :outfits="data" :faction-id="1"></outfit-list>
                    </div>
                    <div class="flex-grow-1">
                        <outfit-list :outfits="data" :faction-id="2"></outfit-list>
                    </div>
                    <div class="flex-grow-1">
                        <outfit-list :outfits="data" :faction-id="3"></outfit-list>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import { OutfitPopulation, OutfitPopulationApi } from "api/OutfitPopulationApi";

    import OutfitList from "./components/OutfitList.vue";

    export const OutfitPop = Vue.extend({
        props: {

        },

        created: function(): void {
            document.title = `Honu / PEEPERS`;
        },

        data: function() {
            return {
                worldID: 1 as number,
                time: new Date() as Date,

                data: Loadable.idle() as Loading<OutfitPopulation[]>
            }
        },

        methods: {
            load: async function(): Promise<void> {
                this.data = Loadable.loading();

                this.time = new Date(this.time);

                this.data = await OutfitPopulationApi.getPopulation(this.worldID, this.time);
            }

        },

        components: {
            OutfitList,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });
    export default OutfitPop;

    (window as any).OutfitPopulationApi = OutfitPopulationApi;
</script>