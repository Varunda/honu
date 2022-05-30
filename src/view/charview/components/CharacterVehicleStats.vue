<template>
    <div>
        <h3 class="text-warning text-center">
            work in progress
        </h3>

        <div class="mb-2">
            <button type="button" class="btn btn-primary" @click="loadEntries">
                Reload
            </button>

            <toggle-button v-model="showDebug">
                Show IDs
            </toggle-button>

            <toggle-button v-model="showImages">
                Show images
            </toggle-button>
        </div>

        <a-table
            :entries="vehicleStats"
            :show-filters="true"
            :striped="true"
            default-sort-field="secondsWith" default-sort-order="desc"
            display-type="table">

            <a-col sort-field="name">
                <a-header>
                    <b>Vehicle</b>
                </a-header>

                <a-filter method="input" type="string" field="itemName"
                    :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <div :style="{ height: (showImages == true) ? '3rem' : '', position: 'relative' }">
                        <div
                            class="d-inline-block position-absolute"
                            :style="getWeaponNameStyle(entry) ">
                        </div>

                        <census-image v-if="showImages == true && entry.imageID != 0" :image-id="entry.imageID"
                            style="position: absolute; text-align: center; height: 100%; right: 0;" class="mr-1">
                        </census-image>

                        <a :href="'/i/' + entry.itemID" class="ml-1"
                            style="
                                position: absolute;
                                text-shadow: -1px -1px 2px rgb(32, 32, 32), -1px 1px 2px rgb(32, 32, 32), 1px -1px 2px rgb(32, 32, 32), 1px 1px 2px rgb(32, 32, 32);
                            ">

                            {{entry.name}}
                            <span v-if="showDebug == true">
                                / {{entry.vehicleID}}
                            </span>
                        </a>
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="secondsWith">
                <a-header>
                    <b>Time</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.secondsWith | mduration}}
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import CensusImage from "components/CensusImage";
    import ToggleButton from "components/ToggleButton";

    import PercentileCell from "./PercentileCell.vue";

    import "MomentFilter";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter } from "api/CharacterApi";
    import { CharacterWeaponStatEntry, CharacterWeaponStatApi, WeaponStatEntry } from "api/CharacterWeaponStatApi";

    type VehicleStatEntry = {
        vehicleID: number;
        name: string;
        secondsWith: number;
        imageID: number;
    };

    function weaponStatToVehicleStat(entry: CharacterWeaponStatEntry): VehicleStatEntry {
        return {
            vehicleID: entry.vehicleID,
            name: entry.vehicle == null ? `<unknown ${entry.vehicleID}>` : entry.vehicle.name,
            secondsWith: entry.secondsWith,
            imageID: entry.vehicle?.imageID ?? 0
        }
    }

    export const CharacterVehicleStats = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<CharacterWeaponStatEntry[]>,
                showImages: true as boolean,
                showDebug: false as boolean
            }
        },

        beforeMount: function(): void {
            this.loadEntries();
        },

        methods: {
            getWeaponNameStyle: function(entry: CharacterWeaponStatEntry): object {
                let background: string = "rgb(32, 32, 32)";

                if (entry.kills >= 10) {
                    background = "#5c2b00";
                }
                if (entry.kills >= 60) {
                    background = "#4c4c4c";
                }
                if (entry.kills >= 160) {
                    background = "#544e01";
                }
                if (entry.kills >= 1160) {
                    background = "#5c005c";
                }

                return {
                    'height': (this.showImages == true) ? '3rem' : '1.5rem',
                    'background-color': background,
                    width: Math.min(100, (entry.kills / 1160 * 100)) + '%',
                    'border-radius': '3px' 
                }
            },

            loadEntries: async function(): Promise<void> {
                this.entries = Loadable.loading();
                this.entries = await CharacterWeaponStatApi.getByCharacterID(this.character.id);
            }
        },

        computed: {
            vehicleStats: function(): Loading<VehicleStatEntry[]> {
                if (this.entries.state != "loaded") {
                    return Loadable.rewrap(this.entries);
                }

                return Loadable.loaded(
                    this.entries.data.filter(iter => iter.vehicleID > 0 && iter.itemID == "0" && iter.secondsWith > 0)
                        .map(iter => weaponStatToVehicleStat(iter))
                );
            }
        },

        watch: {
            "character.id": function(): void {
                console.log(`NEW CHAR ID ${this.character.id}`);
            }
        },

        components: {
            ATable,
            ACol,
            AHeader,
            ABody,
            AFilter,
            InfoHover,
            PercentileCell,
            CensusImage,
            ToggleButton
        }

    });
    export default CharacterVehicleStats;
</script>