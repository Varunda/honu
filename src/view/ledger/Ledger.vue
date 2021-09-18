<template>
    <div>
        <div class="d-flex align-items-center">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                 / Ledger
            </h1>

        </div>

        <div>
            <div class="w-100 mb-1">
                <button class="btn btn-success w-100" data-toggle="collapse" href="#ledger-readme">
                    View help
                </button>
            </div>

            <div class="btn-group w-100 mb-2">
                <button type="button" class="btn btn-secondary" @click="view = 'map'">
                    View map
                </button>

                <button type="button" class="btn btn-secondary" @click="view = 'list'">
                    View list
                </button>
            </div>
        </div>

        <div class="mb-2">
            <div id="ledger-readme" class="collapse">
                <h3>
                    What is this?
                </h3>
                <p>
                    Ledger is a facility control tracker. Everytime a base is captured or defended (since collection started), it is recorded, along with how many players particpated.
                    This is a display of that data, viewable as a map or a list.
                </p>

                <h3>When did data collection start?</h3>
                <p>2021-09-06</p>

                <h3>Do you include captures/defenses when a continent is not fully unlocked?</h3>
                <p>No</p>

                <h3>What do the different numbers mean?</h3>
                <ul>
                    <li><b>Ratio: </b>How many times a base is defended per one capture. For example, a ratio of 3 means a base is defended 3 times per 1 capture</li>
                    <li><b>Total: </b>How many times in total a base has been captured or defended</li>
                    <li><b>Avg capture: </b>On average, the number of players who get credit for a capture</li>
                    <li><b>Avg defend: </b>On average, the number of players who get credit for a defense</li>
                    <li><b>Avg players: </b>On average, the number of players who get credit for a capture or defense</li>
                </ul>

                <h3>When is defend credit given out?</h3>
                <p>
                    A defense happens when any point is flipped in favor of the attacker, and once all points have been flipped back to the defenders.
                    For bases where attackers never flip the point, this is not recorded.
                </p>
            </div>
        </div>

        <div v-if="view == 'list'" class="mb-3 form-row align-items-center">
            <div class="col-auto">
                <label>Player theshold</label>
                <input class="form-control" type="number" v-model.number="filter.playerThreshold" />
            </div>

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

        <ledger-list v-if="view == 'list'" :entries="entries"></ledger-list>

        <ledger-map v-if="view == 'map'"></ledger-map>
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
                view: "map" as string,

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