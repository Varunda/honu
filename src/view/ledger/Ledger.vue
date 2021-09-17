<template>
    <div>
        <div class="mb-3 form-row align-items-center">
            <div class="col-auto">
                <label>Player theshold</label>
                <input class="form-control" type="number" v-model.number="filter.playerThreshold" />
            </div>

            <!--
            <div class="form-group">
                <label>Unstable</label>
                <select class="form-control" v-model="filter.unstableState">
                    <option :value="null">All</option>
                    <option :value="1">Fully unlocked</option>
                    <option :value="2">Double lane</option>
                    <option :value="3">Single lane</option>
                </select>
            </div>
            -->

            <div class="col-auto">
                <label class="d-block">
                    Servers
                </label>
                
                <div class="form-check form-check-inline">
                    <input type="checkbox" class="form-check-input" value="1" v-model.number="filter.worldID" />
                    <label class="form-check-label">Connery</label>
                </div>

                <div class="form-check form-check-inline">
                    <input type="checkbox" value="17" class="form-check-input" v-model.number="filter.worldID" />
                    <label class="form-check-label">Emerald</label>
                </div>

                <div class="form-check form-check-inline">
                    <input type="checkbox" value="10" class="form-check-input" v-model.number="filter.worldID" />
                    <label class="form-check-label">Cobalt</label>
                </div>

                <div class="form-check form-check-inline">
                    <input type="checkbox" value="13" class="form-check-input" v-model.number="filter.worldID" />
                    <label class="form-check-label">Miller</label>
                </div>

                <div class="form-check form-check-inline">
                    <input type="checkbox" value="40" class="form-check-input" v-model.number="filter.worldID" />
                    <label class="form-check-label">SolTech</label>
                </div>

            </div>

            <div class="col-auto">
                <button type="button" @click="bindLedgerData" class="btn btn-primary">Load</button>
            </div>

            <!--
            <div class="form-group">
                <label>Start date</label>
                <input type="datetime-local" class="form-control" />
            </div>

            <div class="form-group">
                <label>End date</label>
                <input type="datetime-local" class="form-control" />
            </div>
            -->
        </div>

        <ledger-list :entries="entries"></ledger-list>

        <ledger-map></ledger-map>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import { LedgerApi, FacilityControlEntry, LedgerOptions } from "api/LedgerApi";

    import LedgerList from "./components/LedgerList.vue";
    import LedgerMap from "./components/LedgerMap.vue";

    import "filters/ZoneNameFilter";

    export const Ledger = Vue.extend({
        data: function () {
            return {
                entries: Loadable.idle() as Loading<FacilityControlEntry[]>,

                filter: {
                    worldID: [1, 10, 13, 17, 40] as number[],
                    playerThreshold: 12 as number
                }
            }
        },

        created: function(): void {
            this.bindLedgerData();
        },

        methods: {
            bindLedgerData: async function(): Promise<void> {
                const options: LedgerOptions = new LedgerOptions();
                options.playerThreshold = this.filter.playerThreshold;
                options.worldID = this.filter.worldID;

                this.entries = Loadable.loading();
                this.entries = Loadable.loaded(await LedgerApi.getLedger(options));
            }
        },

        computed: {
            listSources: function() {
                return {
                    ledger: LedgerApi.getLedger
                };
            },

            filterSources: function() {
                return {
                    zone: [
                        { value: null, key: "All" },
                        { value: 2, key: "Indar" },
                        { value: 4, key: "Hossin" },
                        { value: 6, key: "Amerish" },
                        { value: 8, key: "Esamir" }
                    ]
                }
            }
        },

        components: {
            ATable,
            ACol,
            AHeader,
            ABody,
            AFilter,
            LedgerList,
            LedgerMap
        }

    });
    export default Ledger;

    (window as any).LedgerApi = LedgerApi;
</script>