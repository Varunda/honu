<template>
    <div class="ledger-map-parent row" style="height: 1024px;">
        <div class="col-2">
            <div v-if="selectedFacility == null">
                Click a facility!
            </div>

            <div v-else>
                <table class="table table">
                    <tr>
                        <td>Facility</td>
                        <td>{{selectedFacility.facilityName}}</td>
                    </tr>

                    <tr>
                        <td>ID</td>
                        <td>{{selectedFacility.facilityID}}</td>
                    </tr>

                    <tr>
                        <td>Type</td>
                        <td>{{selectedFacility.typeName}}</td>
                    </tr>

                    <tr>
                        <td>
                            Ratio
                            <info-hover text="The ration of defenses to captures. For example, a ratio of 4.5 means for every 1 capture, there are 4.5 defenses"></info-hover>
                        </td>
                        <td>{{selectedFacility.ratio.toFixed(2)}}</td>
                    </tr>

                    <tr>
                        <td>
                            Total fights
                            <info-hover text="The total number of captures and defenses that have occured at this facility"></info-hover>
                        </td>
                        <td>{{selectedFacility.total}}</td>
                    </tr>

                    <tr>
                        <td>
                            Capture count
                            <info-hover text="The number of times this base has been captured"></info-hover>
                        </td>
                        <td>{{selectedFacility.captured}}</td>
                    </tr>

                    <tr>
                        <td>
                            Defended count
                            <info-hover text="The number of times this base has been defended"></info-hover>
                        </td>
                        <td>{{selectedFacility.defended}}</td>
                    </tr>

                    <tr>
                        <td>
                            Average players for capture
                            <info-hover text="The average number of players who get credit when a capture occurs"></info-hover>
                        </td>
                        <td>{{selectedFacility.captureAverage.toFixed(2)}}</td>
                    </tr>

                    <tr>
                        <td>
                            Average players for defense
                            <info-hover text="The average number of players who get credit when a defense occurs"></info-hover>
                        </td>
                        <td>{{selectedFacility.defenseAverage.toFixed(2)}}</td>
                    </tr>
                </table>
            </div>
        </div>

        <div id="ledger-map-main" class="ledger-map-map col-8" style="height: 1024px;"></div>

        <div class="ledger-map-list col-2 d-flex" style="flex-direction: column; ">
            <div class="flex-grow-0 mb-1" style="position: relative;">
                <label>
                    Select continent:
                </label>

                <select class="form-control mb-2" v-model.number="zoneID">
                    <option :value="2">Indar</option>
                    <option :value="4">Hossin</option>
                    <option :value="6">Amerish</option>
                    <option :value="8">Esamir</option>
                    <option :value="344">Oshur</option>
                </select>

                <label>
                    Sort by:
                </label>

                <select class="form-control mb-2" v-model="orderBy">
                    <option value="ratio">Ratio</option>
                    <option value="total">Total # of fights</option>
                    <option value="cap_total"># of captures</option>
                    <option value="def_total"># of defenses</option>
                    <option value="capture">Avg # of players who get capture credit</option>
                    <option value="defend">Avg # of players who get defense credit</option>
                    <option value="all">Avg players</option>
                    <option value="layna">Fight inequality</option>
                </select>

                <label>Order:</label>

                <select class="form-control mb-2" v-model="orderAsc">
                    <option value="asc">Ascending (0-9)</option>
                    <option value="desc">Descending (9-0)</option>
                </select>

                <span>
                    <button type="button" class="btn btn-primary d-inline-block" @click="loadData">
                        Load
                    </button>

                    <button type="button" class="btn btn-primary d-inline-block" @click="toggleNames">
                        Toggle names
                    </button>

                    <button type="button" class="btn btn-primary d-inline-block" @click="toggleBlackwhite">
                        Colorblind
                    </button>
                </span>

            </div>

            <div style="flex-grow: 1; position: relative; overflow-y: scroll;">
                <table class="table table-sm flex-grow-1" style="position: absolute;">
                    <thead>
                        <tr>
                            <th>Facility</th>
                            <th>{{orderByName}}</th>
                        </tr>
                    </thead>

                    <tbody v-if="ledgerEntries.state == 'idle'">
                        <tr>
                            <td colspan="2">No continent selected</td>
                        </tr>
                    </tbody>

                    <tbody v-else-if="ledgerEntries.state == 'loading'">
                        <tr>
                            <td colspan="2">Loading...</td>
                        </tr>
                    </tbody>

                    <tbody v-else-if="ledgerEntries.state == 'loaded'">
                        <tr v-for="entry in ledgerEntries.data">
                            <td>
                                {{entry.name}}
                            </td>
                            <td>
                                {{entry.value.toFixed(2)}}
                            </td>
                        </tr>
                    </tbody>

                    <tbody v-else-if="ledgerEntries.state == 'error'">
                        <tr>
                            <td colspan="2">Error! {{ledgerEntries.message}}</td>
                        </tr>
                    </tbody>

                    <tbody v-else>
                        <tr>
                            <td colspan="2">Unchecked state of ledgerEntries: {{ledgerEntries.state}}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType }  from "vue";

    import * as L from "leaflet";
    import { Loading, Loadable } from "Loading";

    import InfoHover from "components/InfoHover.vue";

    import ColorUtils, { RGB } from "util/Color";
    import { Quartile } from "util/Quartile";
    import ZoneUtils from "util/Zone";

    import { PsMapHex, PsFacility, PsFacilityLink, ZoneMap, MapApi } from "api/MapApi";
    import { LedgerApi, FacilityControlEntry, LedgerOptions } from "api/LedgerApi";

    import { ZoneRegion, LatticeLink } from "map/LedgerModels";
    import { ColorSet, colorMin, colorNormal, colorMax, bwMin, bwNormal, bwMax } from "../ColorSet"

    type LedgerEntry = { id: number, name: string, value: number, color: string };

    export const LedgerMap = Vue.extend({
        props: {
            entries: { type: Object as PropType<Loading<FacilityControlEntry[]>>, required: true }
        },

        data: function() {
            return {
                map: null as L.Map | null,

                ledgerEntries: Loadable.idle() as Loading<LedgerEntry[]>,
                zoneID: 2 as number,

                panes: {
                    color: null as HTMLElement | null,
                    lattice: null as HTMLElement | null
                },

                colors: {
                    blackwhite: false as boolean,
                    min: colorMin as ColorSet,
                    normal: colorNormal as ColorSet,
                    max: colorMax as ColorSet,
                    fillOpacity: 0.4 as number
                },

                //ledgerData: Loadable.idle() as Loading<FacilityControlEntry[]>,
                selectedFacility: null as FacilityControlEntry | null,
                zoneData: null as ZoneMap | null,

                tooltipsOn: true as boolean,

                regions: [] as ZoneRegion[],

                orderBy: "ratio" as "ratio" | "total" | "cap_total" | "def_total" | "capture" | "defend" | "all" | "layna",
                orderAsc: "desc" as "asc" | "desc",
                orderByName: "" as string
            }
        },

        mounted: function(): void {
            this.$nextTick(() => {
                this.loadData();
            });
        },

        methods: {
            loadData: function(): void {
                let zoneName: string = "";

                if (this.zoneID == ZoneUtils.Indar) { // 2
                    zoneName = "indar";
                } else if (this.zoneID == ZoneUtils.Hossin) { // 4
                    zoneName = "hossin";
                } else if (this.zoneID == ZoneUtils.Amerish) { // 6
                    zoneName = "amerish";
                } else if (this.zoneID == ZoneUtils.Esamir) { // 8
                    zoneName = "esamir";
                } else if (this.zoneID == ZoneUtils.Oshur) { // 334
                    zoneName = "oshur";
                } else {
                    throw `Missing zoneName from zoneID ${this.zoneID}`;
                }

                this.createMap(this.zoneID, zoneName);
            },

            loadFacility: async function(facilityID: number): Promise<void> {
                this.selectedFacility = null;

                const entry: FacilityControlEntry | null = this.ledgerData.find(iter => iter.facilityID == facilityID) || null;
                if (entry == null) {
                    return console.error(`Failed to find facility ID ${facilityID}`);
                }

                this.selectedFacility = entry;
            },

            loadLedgerData: async function(zoneID: number): Promise<void> {
                if (this.entries.state != "loaded") {
                    return;
                }

                this.ledgerEntries = Loadable.loading();
                const data = this.entries.data.filter(iter => iter.zoneID == zoneID);

                const arr: LedgerEntry[] = [];
                for (const datum of data) {
                    let value: number = 0;
                    if (this.orderBy == "ratio") {
                        value = datum.ratio;
                    } else if (this.orderBy == "total") {
                        value = datum.total;
                    } else if (this.orderBy == "cap_total") {
                        value = datum.captured;
                    } else if (this.orderBy == "def_total") {
                        value = datum.defended;
                    } else if (this.orderBy == "capture") {
                        value = datum.captureAverage;
                    } else if (this.orderBy == "defend") {
                        value = datum.defenseAverage;
                    } else if (this.orderBy == "all") {
                        value = datum.totalAverage;
                    } else if (this.orderBy == "layna") {
                        value = datum.captureAverage / datum.defenseAverage;
                    } else {
                        throw `Unchecked orderBy value: '${this.orderBy}'`;
                    }

                    arr.push({
                        id: datum.facilityID,
                        name: datum.facilityName,
                        value: value,
                        color: ""
                    });
                }

                let sorted: LedgerEntry[] = this.setColors(arr);
                if (this.orderAsc == "desc") {
                    sorted = sorted.sort((a, b) => b.value - a.value);
                }

                this.ledgerEntries = Loadable.loaded(sorted);
            },

            createMap: async function(zoneID: number, zoneName: string): Promise<void> {
                this.panes.color = null;
                this.panes.lattice = null;
                this.regions = [];

                await this.loadLedgerData(zoneID);

                const zoneData: Loading<ZoneMap> = await MapApi.getZone(zoneID);
                if (zoneData.state != "loaded") {
                    throw `Failed to get zone data for ${zoneID}`;
                }
                this.zoneData = zoneData.data;

                const regions: ZoneRegion[] = ZoneRegion.setupMapRegions(zoneData.data);

                // If there is currently a map, the view will be copied to the new map
                let center: L.LatLng = L.latLng(0.00, 0.00);
                let zoom: number = 2;

                if (this.map != null) {
                    center = this.map.getBounds().getCenter();
                    zoom = this.map.getZoom();

                    this.map.remove();
                    this.map = null;
                }

                this.map = new L.Map("ledger-map-main", {
                    crs: L.CRS.Simple,
                    zoomSnap: 0.1,
                    zoomDelta: 0.1,
                }).setView(center, zoom);

                this.panes.color = this.map.createPane("regions");
                this.panes.lattice = this.map.createPane("lattices");

                L.tileLayer(`/img/zones/${zoneName}/zoom{z}/${zoneName}_{z}_{x}_{y}.jpg`, {
                    minZoom: 1,
                    maxZoom: 5,
                    noWrap: true,
                    bounds: L.latLngBounds(L.latLng(-128, -128), L.latLng(128, 128))
                }).addTo(this.map);

                if (this.ledgerEntries.state != "loaded") { throw `ughhh`; }

                for (const region of regions) {
                    const fac: PsFacility | null = zoneData.data.facilities.find(iter => iter.regionID == region.regionID) || null;
                    region.facility = fac;

                    let name: string = "";

                    if (fac != null) {
                        name += `${fac.name}`;

                        const stats: LedgerEntry | null = this.ledgerEntries.data.find(iter => iter.id == region.facility?.facilityID) || null;
                        if (stats != null) {
                            region.setStyle({
                                fill: true,
                                fillColor: stats.color,
                                fillOpacity: this.colors.fillOpacity
                            });

                            name += ` - ${stats.value.toFixed(2)}`;
                        } else {
                            console.error(`missing facility stats for ${region.regionID}`);
                        }
                    }

                    region.bindTooltip(name, {
                        permanent: true,
                        direction: "center",
                        className: "ledger-base-name",
                        interactive: true
                    });

                    region.addTo(this.map);

                    region.on("click", this.regionClickHandler);
                    region.on("mouseover", this.regionMouseoverHandler);
                    region.on("mouseout", this.regionMouseoutHandler);

                    this.regions.push(region);

                    if (this.tooltipsOn == false) {
                        region.closeTooltip();
                    }
                }

                // Not super effiecient but meh
                for (const region of regions) {
                    if (region.facility == null) {
                        continue;
                    }

                    for (const link of zoneData.data.links) {
                        if (link.facilityA == region.facility.facilityID) {
                            const regionB: ZoneRegion | null = regions.find(iter => iter.facility != null && iter.facility.facilityID == link.facilityB) || null;
                            if (regionB != null) {
                                const lattice: LatticeLink = new LatticeLink(region, regionB);
                                lattice.addTo(this.map);
                            }
                        }
                    }
                }
            },

            toggleNames: function(): void {
                this.tooltipsOn = !this.tooltipsOn;

                for (const region of this.regions) {
                    if (this.tooltipsOn == true) {
                        region.openTooltip();
                    } else {
                        region.closeTooltip();
                    }
                }
            },

            regionClickHandler: function(ev: L.LeafletEvent): void {
                const region: ZoneRegion = ev.target;

                if (region.facility) {
                    this.loadFacility(region.facility.facilityID);
                }
            },

            regionMouseoverHandler: function(ev: L.LeafletEvent): void {
                const region: ZoneRegion = ev.target;

                region.setStyle({
                    fillOpacity: 0.75
                });
            },

            regionMouseoutHandler: function(ev: L.LeafletEvent): void {
                const region: ZoneRegion = ev.target;

                region.setStyle({
                    fillOpacity: this.colors.fillOpacity
                });
            },

            toggleBlackwhite: function(): void {
                this.colors.blackwhite = !this.colors.blackwhite;

                if (this.colors.blackwhite == true) {
                    this.colors.min = bwMin;
                    this.colors.normal = bwNormal;
                    this.colors.max = bwMax;
                } else {
                    this.colors.min = colorMin;
                    this.colors.normal = colorNormal;
                    this.colors.max = colorMax;
                }

                this.loadData();
            },

            setColors: function(data: LedgerEntry[]): LedgerEntry[] {
                if (data.length == 0) {
                    console.warn(`cannot set colors, got passed 0 data`);
                    return [];
                }

                const sorted: LedgerEntry[] = [...data].sort((a, b) => a.value - b.value);

                const values: number[] = [...sorted].map(iter => iter.value);
                const quartile: Quartile = Quartile.get(values);

                const min = quartile.q1;
                const max = quartile.q3;

                const t_min = sorted[0].value;
                const t_max = sorted[sorted.length - 1].value;

                for (const entry of sorted) {
                    let v = (entry.value - min) / (max - min);
                    let color: RGB | null = null;

                    if (entry.value < min) {
                        v = (entry.value - t_min) / min;
                        color = ColorUtils.colorGradient(
                            v,
                            this.colors.min.start,
                            this.colors.min.end,
                            this.colors.min.middle
                        );
                    } else if (entry.value > max) {
                        v = (entry.value - t_min) / t_max;
                        color = ColorUtils.colorGradient(
                            v,
                            this.colors.max.start,
                            this.colors.max.end,
                            this.colors.max.middle
                        );
                    } else {
                        color = ColorUtils.colorGradient(
                            v,
                            this.colors.normal.start,
                            this.colors.normal.end,
                            this.colors.normal.middle
                        );
                    }

                    const cc = ((color.red << 16) + (color.green << 8) + (color.blue)).toString(16).padStart(6, "0");

                    entry.color = `#${cc}`;
                }

                return sorted;
            }
        },

        computed: {
            ledgerData: function(): FacilityControlEntry[] {
                if (this.entries.state != "loaded") {
                    return [];
                }
                return this.entries.data;
            }
        },

        components: {
            InfoHover
        }
    });
    export default LedgerMap;

    (window as any).MapApi = MapApi;
</script>