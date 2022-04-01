<template>
    <a-table
        :entries="outfits"
        :show-filters="true"
        default-sort-field="engResupplies" default-sort-order="desc" :default-page-size="10"
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

        <a-col sort-field="engKills">
            <a-header>
                <b>Kills</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engKills}}
            </a-body>
        </a-col>

        <a-col sort-field="engKPM">
            <a-header>
                <b>KPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engKPM | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="engDeaths">
            <a-header>
                <b>Deaths</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engDeaths}}
            </a-body>
        </a-col>

        <a-col sort-field="engKD">
            <a-header>
                <b>K/D</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engKD | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="engResupplies">
            <a-header>
                <b>Resupplies</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engResupplies}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>RPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engResuppliesPerMinute | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="repairs">
            <a-header>
                <b>MAX repairs</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engRepairs}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>MRPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.engRepairsPerMinute | locale(2)}}
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

    export const AlertOutfitEngineerBoard = Vue.extend({
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
    export default AlertOutfitEngineerBoard;
</script>
