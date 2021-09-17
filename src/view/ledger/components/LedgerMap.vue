<template>
    <div id="ledger-map-main" style="height: 1024px;">
        howdy
    </div>
</template>

<script lang="ts">
    import Vue, { PropType }  from "vue";

    import * as L from "leaflet";
    import { ZoneRegion } from "../LedgerModels";
    import { Loading } from "Loading";
    import { RGB, colorGradient } from "util/Color";
    import { Quartile } from "util/Quartile";

    import { PsMapHex, PsFacility, PsFacilityLink, ZoneMap, MapApi } from "api/MapApi";
    import { FacilityControlEntry } from "api/LedgerApi";

    type FacilityControlEntryColored = FacilityControlEntry & { color: string };

    export const LedgerMap = Vue.extend({
        props: {
            entries: { type: Object as PropType<Loading<FacilityControlEntry[]>>, required: true },
            ZoneId: { type: Number, required: true },
            ZoneName: { type: String, required: true }
        },

        data: function() {
            return {
                map: null as L.Map | null,
                ledgerEntries: [] as FacilityControlEntryColored[]
            }
        },

        mounted: function(): void {
            this.setColors();
            this.$nextTick(() => {
                this.createMap(this.ZoneId, this.ZoneName);
            });
        },

        methods: {
            createMap: async function(zoneID: number, zoneName: string): Promise<void> {
                const zoneData: ZoneMap | null = await MapApi.getZone(zoneID);
                if (zoneData == null) {
                    throw `Failed to get zone data for ${zoneID}`;
                }

                const regions: ZoneRegion[] = ZoneRegion.setupMapRegions(zoneData.hexes);

                this.map = new L.Map("ledger-map-main", {
                    crs: L.CRS.Simple
                }).setView([0.00, 0.00], 1);

                this.map.createPane("regions");

                L.tileLayer(`/img/zones/${zoneName}/zoom{z}/${zoneName}_{z}_{x}_{y}.jpg`, {
                    attribution: "Rhett &amp; Lampjaw",
                    minZoom: 1,
                    maxZoom: 5,
                    noWrap: true,
                    bounds: L.latLngBounds(L.latLng(-128, -128), L.latLng(128, 128))
                }).addTo(this.map);

                for (const region of regions) {
                    const fac: PsFacility | null = zoneData.facilities.find(iter => iter.regionID == region.regionID) || null;
                    region.facility = fac;

                    let name: string = "";

                    if (fac) {
                        name += `${fac.name}`;

                        const stats: FacilityControlEntryColored | null = this.ledgerEntries.find(iter => iter.facilityID == region.facility?.facilityID) || null;
                        if (stats != null) {
                            region.setStyle({
                                fill: true,
                                fillColor: stats.color,
                                fillOpacity: 0.4
                            });

                            name += ` - ${stats.ratio.toFixed(2)}`;

                            console.log(`${fac.name} ${stats.color}`);
                        } else {
                            console.error(`missing facility stats for ${region.regionID}`);
                        }
                    }

                    region.bindTooltip(name, {
                        permanent: true,
                        direction: "center",
                        className: "ledger-base-name"
                    });

                    region.addTo(this.map);
                }
            },

            setColors: function(): void {
                if (this.entries.state != "loaded") {
                    console.warn(`cannot set colors, this.entries is not loaded`);
                    return;
                }

                const sorted: FacilityControlEntry[] = [...this.entries.data].filter(iter => iter.zoneID == this.ZoneId)
                    //.sort((a, b) => a.total - b.total);
                    .sort((a, b) => a.ratio - b.ratio);

                const values: number[] = [...sorted].map(iter => iter.ratio);
                const quartile: Quartile = Quartile.get(values);

                const min = quartile.q1;
                const max = quartile.q3;

                const t_min = sorted[0].ratio;
                const t_max = sorted[sorted.length - 1].ratio;

                for (const entry of sorted) {
                    let v = (entry.ratio - min) / (max - min);
                    let color: RGB | null = null;

                    if (entry.ratio < min) {
                        v = (entry.ratio - t_min) / min;
                        color = colorGradient(
                            v,
                            { red: 0, green: 255, blue: 0 },
                            { red: 64, green: 255, blue: 0 }
                        );
                    } else if (entry.ratio > max) {
                        v = (entry.ratio - t_min) / t_max;
                        color = colorGradient(
                            v,
                            { red: 255, green: 64, blue: 0 },
                            { red: 255, green: 0, blue: 0 }
                        );
                    } else {
                        color = colorGradient(
                            v,
                            { red: 64, green: 255, blue: 0 },
                            { red: 255, green: 255, blue: 0 },
                            { red: 255, green: 64, blue: 0 }
                        );
                    }

                    const cc = ((color.red << 16) + (color.green << 8) + (color.blue)).toString(16).padStart(6, "0");

                    this.ledgerEntries.push({
                        ...entry,
                        color: `#${cc}`
                    });
                }

            }
        },

        computed: {

        }

    });
    export default LedgerMap;

    (window as any).MapApi = MapApi;
</script>