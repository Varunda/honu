<template>
    <collapsible header-text="Facility">

        <div>
            <h3 class="wt-header mb-0 border-0" style="background-color: var(--red)">
                Bases
            </h3>

            <a-table :entries="facilityData"
                     :paginate="true"
                     :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                     default-sort-field="captures" default-sort-order="desc"
                     class="border-top-0"
            >

                <a-col>
                    <a-header>
                        Facility
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.facilityName}}
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        Type
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.facilityType}}
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        Continent
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.zoneID | zone}}
                    </a-body>
                </a-col>

                <a-col sort-field="captures">
                    <a-header>
                        Captures
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.captures | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="defenses">
                    <a-header>
                        Defenses
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.defenses | locale}}
                    </a-body>
                </a-col>

            </a-table>

        </div>

    </collapsible>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // models
    import { PsFacility } from "api/MapApi";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/ZoneNameFilter";

    type WrappedFacilityData = {
        facilityID: number;
        facilityName: string;
        facility: PsFacility | null;

        facilityType: string;

        zoneID: number;
        zoneName: string;

        captures: number;
        defenses: number;
        adr: number;
    }

    export const WrappedViewFacility = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

            makeFacilityData: function(facilityID: number): WrappedFacilityData {
                const fac: PsFacility | null = this.wrapped.facilities.get(facilityID) ?? null;

                return {
                    facilityID: facilityID,
                    facilityName: fac?.name ?? `<missing ${facilityID}>`,
                    facility: fac,

                    facilityType: fac?.typeName ?? "unknown",

                    zoneID: fac?.zoneID ?? 0,
                    zoneName: "",

                    captures: 0,
                    defenses: 0,
                    adr: 0
                };
            }

        },

        computed: {
            facilityData: function(): Loading<WrappedFacilityData[]> {
                const map: Map<number, WrappedFacilityData> = new Map();

                for (const ev of this.wrapped.controlEvents) {
                    const fac: WrappedFacilityData = map.get(ev.facilityID) ?? this.makeFacilityData(ev.facilityID);

                    if (ev.oldFactionID == ev.newFactionID) {
                        fac.defenses += 1;
                    } else {
                        fac.captures += 1;
                    }

                    map.set(ev.facilityID, fac);
                }

                const arr: WrappedFacilityData[] = Array.from(map.values()).map((iter: WrappedFacilityData) => {
                    iter.adr = iter.captures / Math.max(1, iter.defenses);
                    return iter;
                });

                return Loadable.loaded(arr);
            }
        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewFacility;
</script>