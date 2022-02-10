<template>
    <div>
        <div v-if="showUI">
            <honu-menu>
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    Realtime map
                </li>
            </honu-menu>
        </div>

        <div v-if="showUI" class="row mb-2">
            <div class="col-auto">
                <toggle-button v-model="layers.terrain">
                    Terrain map
                </toggle-button>

                <toggle-button v-model="layers.owner">
                    Ownership
                </toggle-button>

                <toggle-button v-model="layers.outline">
                    Outline
                </toggle-button>

                <toggle-button v-model="layers.name">
                    Names
                </toggle-button>

                <toggle-button v-model="layers.lattice">
                    Lattice
                </toggle-button>
            </div>

            <div class="col-2 input-group">
                <span class="input-group-text input-group-prepend">
                    Server
                </span>

                <select v-model.number="settings.worldID" class="form-control">
                    <option :value="1">Connery</option>
                    <option :value="10">Miller</option>
                    <option :value="13">Cobalt</option>
                    <option :value="17">Emerald</option>
                    <option :value="19">Jaeger</option>
                    <option :value="40">SolTech</option>
                </select>
            </div>

            <div class="col-2 input-group">
                <span class="input-group-text input-group-prepend">
                    Continent
                </span>

                <select v-model.number="settings.zoneID" class="form-control">
                    <option :value="2">Indar</option>
                    <option :value="4">Hossin</option>
                    <option :value="6">Amerish</option>
                    <option :value="8">Esamir</option>
                    <option :value="344">Oshur</option>
                </select>
            </div>

            <div class="col-auto">
                <button @click="copyUrl" class="btn btn-success" type="button">
                    Copy URL
                </button>
            </div>
        </div>

        <div id="realtime-map" class="realtime-map" style="height: 100vh;"></div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";
    import FactionColors from "FactionColors";

    import ZoneUtils from "util/Zone";

    import * as L from "leaflet";
    import * as sR from "signalR";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuDropdown } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import ToggleButton from "components/ToggleButton";

    import { ZoneRegion, LatticeLink } from "map/LedgerModels";
    import { PsMapHex, PsFacility, PsFacilityLink, ZoneMap, MapApi } from "api/MapApi";

    type PsZone = {
        zoneID: number;
        facilities: Map<number, PsFacilityOwner>;
    };

    type PsFacilityOwner = {
        facilityID: number;
        owner: number;
    }

    export const RealtimeMap = Vue.extend({
        props: {

        },

        data: function() {
            return {
                showUI: true as boolean,
                map: null as L.Map | null,

                zoneData: null as ZoneMap | null,
                ownershipData: null as PsZone | null,
                regionData: [] as ZoneRegion[],
                latticeData: [] as LatticeLink[],

                socketState: "unconnected" as string,
                connection: null as sR.HubConnection | null,
                lastUpdate: null as Date | null,

                settings: {
                    worldID: 1 as number | null,
                    zoneID: 6 as number | null
                },

                panes: {
                    terrain: null as L.TileLayer | null,
                    color: null as HTMLElement | null,
                    lattice: null as HTMLElement | null,
                },

                layers: {
                    terrain: true as boolean,
                    name: true as boolean,
                    outline: true as boolean,
                    lattice: true as boolean,
                    owner: true as boolean
                }
            }
        },

        mounted: function(): void {
            this.parseQueryParams();
            this.makeSignalR();
            this.$nextTick(() => {
                this.createMap();
            });
        },

        methods: {
            makeSignalR: function(): void {
                this.connection = new sR.HubConnectionBuilder()
                    .withUrl("/ws/realtime-map")
                    .withAutomaticReconnect([5000, 10000, 20000, 20000])
                    .build();

                this.connection.on("UpdateMap", this.updateMap);

                this.connection.start().then(() => {
                    this.socketState = "opened";
                    console.log(`connected`);

                    this.updateConnection();
                }).catch(err => {
                    console.error(err);
                });

                this.connection.onreconnected(() => {
                    console.log(`reconnected`);
                    this.socketState = "opened";
                });

                this.connection.onclose((err?: Error) => {
                    this.socketState = "closed";
                    if (err) {
                        console.error("onclose: ", err);
                    }
                });

                this.connection.onreconnecting((err?: Error) => {
                    this.socketState = "reconnecting";
                    if (err) {
                        console.error("onreconnecting: ", err);
                    }
                });
            },

            createMap: async function(): Promise<void> {
                if (this.settings.zoneID == null) {
                    console.warn(`realtime-map> Cannot create map, no zone ID selected`);
                    return;
                }

                if (this.map != null) {
                    this.map.remove();
                    this.map = null;
                }

                if (document.getElementById("realtime-map") == null) {
                    throw `Cannot load map, the element #realtime-map is null`;
                }

                this.map = new L.Map("realtime-map", {
                    crs: L.CRS.Simple,
                    zoomSnap: 0.1,
                    zoomDelta: 0.1,
                }).setZoom(2.63).setView(L.latLng(0, 0));

                const zoneName: string = ZoneUtils.getZoneName(this.settings.zoneID).toLowerCase();

                this.panes.terrain = L.tileLayer(`/img/zones/${zoneName}/zoom{z}/${zoneName}_{z}_{x}_{y}.jpg`, {
                    minZoom: 1,
                    maxZoom: 5,
                    noWrap: true,
                    bounds: L.latLngBounds(L.latLng(-128, -128), L.latLng(128, 128))
                }).addTo(this.map);

                this.panes.color = this.map.createPane("regions");
                this.panes.lattice = this.map.createPane("lattices");

                const zoneData: Loading<ZoneMap> = await MapApi.getZone(this.settings.zoneID);
                if (zoneData.state != "loaded") {
                    throw `Failed to get zone data for ${this.settings.zoneID}`;
                }
                this.zoneData = zoneData.data;

                const regions: ZoneRegion[] = ZoneRegion.setupMapRegions(zoneData.data);

                this.regionData = [];
                for (const region of regions) {
                    const fac: PsFacility | null = zoneData.data.facilities.find(iter => iter.regionID == region.regionID) || null;
                    region.facility = fac;

                    if (fac != null && this.ownershipData != null) {
                        const ownership: PsFacilityOwner | undefined = this.ownershipData.facilities.get(fac.facilityID);
                        if (ownership != undefined) {
                            region.ownerID = ownership.owner;
                        }
                    }

                    let name: string = fac?.name ?? "";

                    region.bindTooltip(name, {
                        permanent: true,
                        direction: "center",
                        className: "realtime-map-base-name",
                        interactive: true
                    });

                    region.addTo(this.map);
                    this.regionData.push(region);

                    if (this.layers.name == false) {
                        region.closeTooltip();
                    }
                }

                this.latticeData = [];
                for (const region of regions) {
                    if (region.facility == null) {
                        continue;
                    }

                    for (const link of zoneData.data.links) {
                        if (link.facilityA == region.facility.facilityID) {
                            const regionB: ZoneRegion | null = regions.find(iter => iter.facility != null && iter.facility.facilityID == link.facilityB) || null;
                            if (regionB != null) {
                                const lattice: LatticeLink = new LatticeLink(region, regionB, "#aaaaaa");
                                lattice.setThickness(10);
                                lattice.addTo(this.map);

                                this.latticeData.push(lattice);
                            }
                        }
                    }
                }

                this.redrawMap();
            },

            updateMap: function(data: any): void {
                // Transform the object into a map with numberic keys
                const zone: PsZone = data;
                zone.facilities = new Map(Object.entries(zone.facilities).map(iter => {
                    return [Number.parseInt(iter[0]), iter[1]];
                }));
                this.ownershipData = zone;

                console.log(this.ownershipData);

                this.redrawMap();
            },

            redrawMap: function(): void {
                if (this.ownershipData == null) {
                    console.warn(`realtime-map> this.ownershipData is null, cannot redraw cuz i got no data`);
                    return;
                }

                if (this.panes.terrain != null) {
                    this.panes.terrain.setOpacity(this.layers.terrain == true ? 1 : 0);
                    this.panes.terrain.redraw();
                }

                // Fill in each region based on who owns it
                this.ownershipData.facilities.forEach((owner: PsFacilityOwner, facilityID: number) => {
                    const region: ZoneRegion | undefined = this.regionData.find(iter => iter.facility?.facilityID == facilityID);
                    if (region != undefined) {
                        region.setStyle({
                            opacity: this.layers.outline == true ? 1 : 0,
                            fill: true,
                            fillColor: FactionColors.getFactionColor(owner.owner),
                            fillOpacity: (this.layers.owner == true) ? this.regionFillOpacity : 0
                        });
                        region.ownerID = owner.owner;
                        region.redraw();

                        //console.log(`${region.facility?.name} => ${owner.owner}`);

                        // Update the lattice link colors for the lattice links on this facility
                        for (const link of this.latticeData) {
                            if (link.facilityA.regionID == region.regionID && link.facilityB.regionID == region.regionID) { // Skip connections to self
                                continue;
                            }
                            if (link.facilityA.regionID != region.regionID && link.facilityB.regionID != region.regionID) { // Skip connections not adjacent
                                continue;
                            }

                            let other: ZoneRegion = (link.facilityA == region) ? link.facilityB : link.facilityA;
                            if (other == null) {
                                continue;
                            }

                            if (other.ownerID == 0 || region.ownerID == 0) {
                                link.setOpacity(0);
                            } else {
                                link.setOpacity(1);
                            }

                            if (other.ownerID == region.ownerID) {
                                link.setLinkColor(FactionColors.getFactionColor(region.ownerID));
                            } else {
                                link.setLinkColor("#ffff00");
                            }
                        }
                    }
                });

                if (this.panes.lattice != null) {
                    this.panes.lattice.style.opacity = this.layers.lattice == true ? "1" : "0";
                }
            },

            updateConnection: async function(): Promise<void> {
                if (this.connection == null) {
                    console.warn(`realtime-map> this.connection is null, cannot update connection`);
                    return;
                }

                await this.connection.send("Initalize", this.settings.worldID, this.settings.zoneID);
            },

            parseQueryParams: function(): void {
                const params: URLSearchParams = new URLSearchParams(location.search);

                const worldID: number = Number.parseInt(params.get("worldID") || "");
                if (Number.isNaN(worldID) == false) {
                    this.showUI = false;
                    this.settings.worldID = worldID;
                }

                const zoneID: number = Number.parseInt(params.get("zoneID") || "");
                if (Number.isNaN(zoneID) == false) {
                    this.showUI = false;
                    this.settings.zoneID = zoneID;
                }

                const hidden: string | null = params.get("hide");
                if (hidden != null) {
                    this.showUI = false;
                    const parts: string[] = hidden.split(",");

                    for (const part of parts) {
                        console.log(`realtime-map> turning off ${part}`);
                        if (part == "terrain") { this.layers.terrain = false; }
                        if (part == "name") { this.layers.name = false; }
                        if (part == "outline") { this.layers.outline = false; }
                        if (part == "lattice") { this.layers.lattice = false; }
                        if (part == "owner") { this.layers.owner = false; }
                    }
                }
            },

            updateQueryParams: function(): void {
                const params: URLSearchParams = new URLSearchParams();

                if (this.settings.worldID != null) {
                    params.set("worldID", this.settings.worldID.toString());
                }
                if (this.settings.zoneID != null) {
                    params.set("zoneID", this.settings.zoneID.toString());
                }

                const hide: string[] = [];

                if (this.layers.terrain == false) { hide.push("terrain"); }
                if (this.layers.lattice == false) { hide.push("lattice"); }
                if (this.layers.name == false) { hide.push("name"); }
                if (this.layers.outline == false) { hide.push("outline"); }
                if (this.layers.owner == false) { hide.push("owner"); }

                if (hide.length > 0) {
                    params.set("hide", hide.join(","));
                }

                const newUrl: string = `${window.location.protocol}//${window.location.host}${window.location.pathname}?${params.toString()}`;
                window.history.pushState({ path: newUrl }, '', newUrl);
            },

            copyUrl: function(): void {
                navigator.clipboard.writeText(location.href);
            }
        },

        watch: {
            "layers.terrain": function(): void {
                if (this.panes.terrain == null) {
                    return;
                }

                if (this.layers.terrain == true) {
                    this.panes.terrain.setOpacity(1);
                } else {
                    this.panes.terrain.setOpacity(0);
                }
                this.panes.terrain.redraw();

                for (const region of this.regionData) {
                    region.setStyle({
                        fillOpacity: this.layers.owner == true ? this.regionFillOpacity : 0
                    });

                    region.redraw();
                }
            },

            "layers.owner": function(): void { this.redrawMap(); },
            "layers.outline": function(): void { this.redrawMap(); },
            "layers.lattice": function(): void { this.redrawMap(); },

            "layers.name": function(): void {
                for (const region of this.regionData) {
                    if (this.layers.name == true) {
                        region.openTooltip();
                    } else {
                        region.closeTooltip();
                    }
                }
            },

            "settings.zoneID": async function(): Promise<void> {
                if (this.socketState == "opened") {
                    await this.updateConnection();
                    this.createMap();
                }
            },

            "settings.worldID": function(): void {
                if (this.socketState == "opened") {
                    this.updateConnection();
                }
            }
        },

        computed: {
            regionFillOpacity: function(): number {
                if (this.layers.terrain == false) {
                    return 1;
                }
                return 0.6;
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover,
            HonuMenu, MenuSep, MenuDropdown,
            ToggleButton
        }
    });
    export default RealtimeMap;
</script>