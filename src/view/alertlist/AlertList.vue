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

                <a-body v-slot="entry">
                    {{entry.id}}
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
                    {{entry.participants | locale}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>View</b>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/alert/' + entry.id" class="btn btn-sm btn-primary">
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
                        { key: null, value: "All" },
                        { key: ZoneUtils.Indar, value: "Indar" },
                        { key: ZoneUtils.Hossin, value: "Hossin" },
                        { key: ZoneUtils.Amerish, value: "Amerish" },
                        { key: ZoneUtils.Esamir, value: "Esamir" },
                        { key: ZoneUtils.Oshur, value: "Oshur" },
                    ],

                    world: [
                        { key: null, value: "All" },
                        { key: 1, value: "Connery" },
                        { key: 10, value: "Miller" },
                        { key: 13, value: "Cobalt" },
                        { key: 17, value: "Emerald" },
                        { key: 19, value: "Jaeger" },
                        { key: 40, value: "SolTech" },
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