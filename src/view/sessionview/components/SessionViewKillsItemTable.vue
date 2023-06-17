<template>
    <a-table :entries="entries"
        :show-filters="true"
        default-sort-field="uses" default-sort-order="desc"
        :default-page-size="10" :page-sizes="[5, 10, 20, 50, 200]"
        display-type="table" row-padding="compact">

        <a-col sort-field="name">
            <a-header>
                <b v-if="ShowCategory == true">
                    Weapon
                </b>
                <b v-else>Category</b>
            </a-header>

            <a-filter method="input" type="string" field="name" max-width="10ch"
                :conditions="[ 'contains', 'equals' ]">
            </a-filter>

            <a-body v-slot="entry">
                <span v-if="ShowCategory == true">
                    <span v-if="entry.id == 0">
                        no weapon
                    </span>
                    <a v-else-if="entry.id > 0" :href="'/i/' + entry.id">
                        {{entry.name}}
                    </a>
                </span>
                <span v-else>
                    {{entry.name}}
                </span>
            </a-body>
        </a-col>

        <a-col v-if="ShowCategory">
            <a-header>
                <b>Category</b>
            </a-header>

            <a-filter field="categoryName" type="string" method="dropdown"
                :conditions="[ 'equals' ]">
            </a-filter>

            <a-body v-slot="entry">
                {{entry.categoryName}}
            </a-body>
        </a-col>

        <a-col sort-field="uses">
            <a-header>
                <b>Kills</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.uses}}
            </a-body>
        </a-col>

        <a-col sort-field="uses">
            <a-header>
                <b>%</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.uses / total * 100 | locale(2)}}%
            </a-body>
        </a-col>

        <a-col sort-field="headshot">
            <a-header>
                HSR (%)
            </a-header>

            <a-body v-slot="entry">
                {{entry.headshot | locale(0)}}
                ({{entry.headshot / entry.uses * 100 | locale(2)}}%)
            </a-body>
        </a-col>

        <a-col sort-field="hip">
            <a-header>
                <b>Hip (%)</b>
                <info-hover text="What percent were made in hipfire"></info-hover>
            </a-header>

            <a-body v-slot="entry">
                {{entry.hip}}
                ({{entry.hip / entry.uses * 100 | locale(2)}}%)
            </a-body>
        </a-col>

        <a-col sort-field="ads">
            <a-header>
                <b>ADS (%)</b>
                <info-hover text="What percent were made in ADS"></info-hover>
            </a-header>

            <a-body v-slot="entry">
                {{entry.ads}}
                ({{entry.ads / entry.uses * 100 | locale(2)}}%)
            </a-body>
        </a-col>
    </a-table>
</template>

<script lang="ts">
    import Vue from "vue";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

    export const SessionViewKillsItemTable = Vue.extend({
        props: {
            entries: { type: Object, required: true },
            ShowCategory: { type: Boolean, required: false, default: true },
            total: { type: Number, required: true }
        },

        data: function() {
            return {

            }
        },

        components: {
            InfoHover,
            ATable, ACol, ABody, AFilter, AHeader
        }
    });
    export default SessionViewKillsItemTable;
</script>