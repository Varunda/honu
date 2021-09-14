<template>
    <div>
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

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

    import "filters/ZoneNameFilter";

    export const LedgerList = Vue.extend({
        props: {
            entries: { required: true }
        },

        data: function () {
            return {

            }
        },

        methods: {

        },

        computed: {
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
    export default LedgerList;
</script>