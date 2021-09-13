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

        <a-table
            :entries="entries"
            :show-filters="true"
            default-sort-field="facilityName"
            display-type="table">

            <a-col sort-field="facilityName">
                <a-header>
                    <b>Facility</b>
                </a-header>

                <a-filter method="input" type="string" field="facilityName"
                    :conditions="['contains']" placeholder="Facility name">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.facilityName}}
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
                    <b>Zone</b>
                </a-header>

                <a-filter method="dropdown" type="string" field="zoneID" :source="filterSources.zone" source-key="key" source-value="value"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.zoneID | zone}}
                </a-body>
            </a-col>

            <a-col sort-field="captured">
                <a-header>
                    <b>Captured</b>
                </a-header>

                <a-filter method="input" type="number" field="captured" max-width="12ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.captured}}
                </a-body>
            </a-col>

            <a-col sort-field="defended">
                <a-header>
                    <b>Defended</b>
                </a-header>

                <a-filter method="input" type="number" field="defended" max-width="12ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.defended}}
                </a-body>
            </a-col>

            <a-col sort-field="total">
                <a-header>
                    <b>Total</b>
                </a-header>

                <a-filter method="input" type="number" field="total" max-width="12ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.total}}
                </a-body>
            </a-col>

            <a-col sort-field="captureAverage">
                <a-header>
                    <b>Capture average</b>
                    <info-hover text="How many on average get credit for a capture"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.captureAverage.toFixed(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="defenseAverage">
                <a-header>
                    <b>Defend average</b>
                    <info-hover text="How many on average get credit for a capture"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.defenseAverage.toFixed(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="ratio">
                <a-header>
                    <b>Ratio</b>
                    <info-hover text="For every one capture, how many defenses happen?"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    {{(entry.ratio).toFixed(2)}}
                </a-body>
            </a-col>

        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { LedgerApi, FacilityControlEntry, LedgerOptions } from "api/LedgerApi";

    import { Loading, Loadable } from "Loading";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

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
            InfoHover,
            ATable,
            ACol,
            AHeader,
            ABody,
            AFilter
        }

    });
    export default Ledger;

    (window as any).LedgerApi = LedgerApi;
</script>