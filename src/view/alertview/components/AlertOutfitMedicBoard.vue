<template>
    <a-table
        :entries="outfits"
        :show-filters="true"
        default-sort-field="heals" default-sort-order="desc"
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

        <a-col sort-field="heals">
            <a-header>
                <b>Heals</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.heals}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>HPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.heals / entry.medicPlaytime * 60 | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="deaths">
            <a-header>
                <b>Revives</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.revives}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>RPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.revives / entry.medicPlaytime * 60 | locale}}
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
                        { key: null, value: "All" },
                        { key: 1, value: "VS" },
                        { key: 2, value: "NC" },
                        { key: 3, value: "TR" },
                        { key: 4, value: "NS" },
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
