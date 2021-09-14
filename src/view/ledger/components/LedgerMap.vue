<template>
    <div id="ledger-map-main" style="height: 300px;">
        howdy
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import * as L from "leaflet";
    import { PsMapHex, PsFacility, PsFacilityLink, MapApi } from "api/MapApi";

    export const LedgerMap = Vue.extend({
        props: {
            entries: { required: true }
        },

        data: function() {
            return {
                map: null as L.Map | null
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.createMap();
            });
        },

        methods: {
            createMap: function(): void {
                this.map = new L.Map("ledger-map-main").setView([51.505, -0.09], 13);

                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                }).addTo(this.map);
            }

        }

    });
    export default LedgerMap;

    (window as any).MapApi = MapApi;
</script>