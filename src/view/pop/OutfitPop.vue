<template>
    <div>
        <div class="d-flex align-items-center">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                 / Big Beans
            </h1>
        </div>

        <div class="row">
            <div class="col-2">
                <label>World</label>
                <select class="form-control mb-2" v-model.number="worldID">
                    <option :value="1">Connery</option>
                    <option :value="10">Cobalt</option>
                    <option :value="13">Miller</option>
                    <option :value="17">Emerald</option>
                    <option :value="19">Jaeger</option>
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

    import { OutfitPopulation, OutfitPopulationApi } from "api/OutfitPopulationApi";

    import OutfitList from "./components/OutfitList.vue";

    export const OutfitPop = Vue.extend({
        props: {

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

                this.data = Loadable.loaded(await OutfitPopulationApi.getPopulation(this.worldID, this.time));
            }

        },

        components: {
            OutfitList
        }

    });
    export default OutfitPop;

    (window as any).OutfitPopulationApi = OutfitPopulationApi;
</script>