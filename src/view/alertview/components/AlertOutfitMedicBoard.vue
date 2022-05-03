<template>
    <a-table
        :entries="outfits"
        :show-filters="true"
        default-sort-field="medicRevives" default-sort-order="desc" :default-page-size="10"
        display-type="table" row-padding="compact">

        <a-col sort-field="outfitDisplay">
            <a-header>
                <b>Outfit</b>
                <info-hover text="An outfit must have had at least 5 members online during the course of the alert"></info-hover>
            </a-header>

            <a-filter field="outfitDisplay" type="string" method="input"
                :conditions="[ 'contains' ]">
            </a-filter>

            <a-body v-slot="entry">
                <a :href="'/o/' + entry.outfitID" :style="{ color: getFactionColor(entry.factionID) }">
                    {{entry.outfitDisplay}}
                </a>
            </a-body>
        </a-col>

        <a-col sort-field="factionID">
            <a-header>
                <b>Faction</b>
            </a-header>

            <a-filter field="factionID" type="number" method="dropdown" :source="sources.factions" source-key="key" source-value="value"
                :conditions="['equals']">
            </a-filter>

            <a-body v-slot="entry">
                {{entry.factionID | faction}}
            </a-body>
        </a-col>

        <a-col sort-field="medicKills">
            <a-header>
                <b>Kills</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicKills}}
            </a-body>
        </a-col>

        <a-col sort-field="medicKPM">
            <a-header>
                <b>KPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicKPM | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="medicRevives">
            <a-header>
                <b>Revives</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicRevives}}
            </a-body>
        </a-col>

        <a-col sort-field="medicRevivesPerMinute">
            <a-header>
                <b>RPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicRevivesPerMinute | locale(2)}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>Deaths</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicDeaths | locale}}
            </a-body>
        </a-col>

        <a-col sort-field="medicKD">
            <a-header>
                <b>K/D</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicKD | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="medicKRD">
            <a-header>
                <b>K+R/D</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicKRD | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="medicHeals">
            <a-header>
                <b>Heals</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicHeals}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>HPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.medicHealsPerMinute | locale(2)}}
            </a-body>
        </a-col>
    </a-table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";
    import InfoHover from "components/InfoHover.vue";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    export const AlertOutfitMedicBoard = Vue.extend({
        props: {
            outfits: { type: Object, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            getFactionColor: function(factionID: number): string {
                return ColorUtils.getFactionColor(factionID) + " !important";
            }
        },

        computed: {
            sources: function() {
                return {
                    factions: [
                        { key: "All", value: null },
                        { key: "VS", value: 1 },
                        { key: "NC", value: 2 },
                        { key: "TR", value: 3 },
                        { key: "NS", value: 4 },
                    ]
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover
        }
    });
    export default AlertOutfitMedicBoard;
</script>
