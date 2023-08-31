<template>
    <collapsible header-text="Vehicle stats">
        <div>
            <h3 class="wt-header mb-0 border-0" style="background-color: var(--purple)">
                Vehicles
            </h3>

            <a-table :entries="vehicleData"
                     :paginate="true"
                     :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                     class="border-top-0"
            >

                <a-col>
                    <a-header>
                        Vehicle
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.vehicleName}}
                    </a-body>
                </a-col>

                <a-col sort-field="killsAs">
                    <a-header>
                        Kills in this vehicle
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.killsAs | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="killed">
                    <a-header>
                        How many destroyed
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.killed | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="suicides">
                    <a-header>
                        Suicides
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.suicides | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="teamkills">
                    <a-header>
                        Teamkills
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.teamkills}}
                    </a-body>
                </a-col>

                <a-col sort-field="deathsAs">
                    <a-header>
                        How many were destroyed
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.deathsAs}}
                    </a-body>
                </a-col>

                <a-col sort-field="deathsFrom">
                    <a-header>
                        How many killed you
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.deathsFrom}}
                    </a-body>
                </a-col>

                <a-col sort-field="teamdeaths">
                    <a-header>
                        Team deaths
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.teamdeaths}}
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

    import { WrappedVehicleUsage } from "../data/vehicles";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";

    export const WrappedViewVehicle = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            vehicleData: function(): Loading<WrappedVehicleUsage[]> {
                return Loadable.loaded(this.wrapped.extra.vehicleUsage);
            }

        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewVehicle;
</script>