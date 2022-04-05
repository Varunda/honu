<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/alerts">Alerts</a>
            </li>
        </honu-menu>

        <a-table :entries="alerts" display-type="table" :show-filters="true" row-padding="compact" default-sort-order="desc" default-sort-field="timestamp">
            <a-col>
                <a-header>
                    <b>ID</b>
                </a-header>

                <a-filter field="displayID" type="string" method="input" max-width="8ch"
                    :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.displayID}}
                </a-body>
            </a-col>

            <a-col>
                <a-header sort-field="timestamp">
                    <b>Start</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.timestamp | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header sort-field="end">
                    <b>End</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.end | moment}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Duration</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.duration | mduration}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Continent</b>
                </a-header>

                <a-filter field="zoneID" type="number" method="dropdown" :source="sources.zone"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.zoneID | zone}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Server</b>
                </a-header>

                <a-filter field="worldID" type="number" method="dropdown" :source="sources.world"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.worldID | world}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Players</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="new Date() < entry.end">
                        --
                    </span>
                    <span v-else>
                        {{entry.participants | locale}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>View</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="new Date() < entry.end" class="text-warning">
                        In progress
                    </span>
                    <a v-else :href="'/alert/' + entry.id">
                        View
                    </a>
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { Loadable, Loading } from "Loading";
    import { AlertApi, PsAlert } from "api/AlertApi";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import "filters/ZoneNameFilter";
    import "filters/WorldNameFilter";
    import "filters/LocaleFilter";
    import "MomentFilter";

    import ZoneUtils from "util/Zone";

    export const AlertList = Vue.extend({
        props: {

        },

        data: function() {
            return {
                alerts: Loadable.idle() as Loading<PsAlert[]>
            }
        },

        created: function(): void {
            document.title = `Honu / Alerts`;
        },

        mounted: function(): void {
            this.bindAlerts();
        },

        methods: {
            bindAlerts: async function(): Promise<void> {
                this.alerts = Loadable.loading();
                this.alerts = await AlertApi.getAll();
            }
        },

        computed: {
            sources: function() {
                return {
                    zone: [
                        { key: "All", value: "All" },
                        { key: "Indar", value: ZoneUtils.Indar },
                        { key: "Hossin", value: ZoneUtils.Hossin },
                        { key: "Amerish", value: ZoneUtils.Amerish },
                        { key: "Esamir", value: ZoneUtils.Esamir },
                        { key: "Oshur", value: ZoneUtils.Oshur },
                    ],

                    world: [
                        { key: "All", value: null },
                        { key: "Connery", value: 1 },
                        { key: "Cobalt", value: 13 },
                        { key: "Emerald", value: 17 },
                        { key: "Miller", value: 10 },
                        { key: "Jaeger", value: 19 },
                        { key: "SolTech", value: 40 },
                    ]
                }
            }

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
        }
    });
    export default AlertList;
</script>