<template>
    <div>
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

            <toggle-button v-model="showExtra">
                Show extra
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

                <a-filter method="input" type="string" field="name"
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

                        <span class="ml-1"
                            style="
                                position: absolute;
                                text-shadow: -1px -1px 2px rgb(32, 32, 32), -1px 1px 2px rgb(32, 32, 32), 1px -1px 2px rgb(32, 32, 32), 1px 1px 2px rgb(32, 32, 32);
                            ">

                            {{entry.name}}
                            <span v-if="showDebug == true">
                                / {{entry.vehicleID}}
                            </span>
                        </span>
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="kills">
                <a-header>
                    <b>Kills</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.kills | locale}}
                </a-body>
            </a-col>

            <a-col sort-field="kpm">
                <a-header>
                    <b>KPM</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.kpm | locale(3)}}
                </a-body>
            </a-col>

            <a-col sort-field="weaponKills">
                <a-header>
                    <b>Weapon kills</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.weaponKills | locale(0)}}
                </a-body>
            </a-col>

            <a-col sort-field="roadKills">
                <a-header>
                    <b>Road kills</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.roadKills | locale(0)}}
                </a-body>
            </a-col>

            <a-col sort-field="vkills">
                <a-header>
                    <b>Vehicle kills</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.vkills | locale(0)}}
                </a-body>
            </a-col>

            <a-col sort-field="vkpm">
                <a-header>
                    <b>VKPM</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.vkpm | locale(3)}}
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

            <a-col sort-field="kd">
                <a-header>
                    <b>K/D</b>
                    <info-hover text="For multi-seat vehicles, deaths only occur if you pulled the vehicle"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.kd | locale(2)}}
                    <span v-if="showExtra">
                        ({{entry.kills | locale(0)}} / {{entry.deaths | locale(0)}})
                    </span>
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
        imageID: number;

        vkills: number;
        roadKills: number;
        weaponKills: number;
        kills: number;

        secondsWith: number;
        weaponSecondsWith: number;
        roadSecondsWith: number;

        kpm: number;
        vkpm: number;

        deaths: number;
        kd: number;
        accuracy: number;
    };

    function weaponStatToVehicleStat(entries: CharacterWeaponStatEntry[]): VehicleStatEntry {
        let baseEntry: CharacterWeaponStatEntry | undefined = entries.find(iter => iter.itemID == "0");
        if (baseEntry == undefined) {
            baseEntry = entries[0];
        }

        console.log(JSON.stringify(baseEntry));

        const weaponKills: number = entries.filter(iter => iter.itemID != "0").reduce((acc, iter) => acc += iter.kills, 0);
        const weaponTime: number = entries.filter(iter => iter.itemID != "0").reduce((acc, iter) => acc += iter.secondsWith, 0);

        return {
            vehicleID: baseEntry.vehicleID,
            name: baseEntry.vehicle == null ? `<unknown ${baseEntry.vehicleID}>` : baseEntry.vehicle.name,
            imageID: baseEntry.vehicle?.imageID ?? 0,

            secondsWith: baseEntry.secondsWith,
            weaponSecondsWith: weaponTime,
            roadSecondsWith: baseEntry.secondsWith - weaponTime,

            kills: baseEntry.kills,
            vkills: baseEntry.vehicleKills,
            roadKills: baseEntry.kills - weaponKills,
            weaponKills: weaponKills,

            kpm: baseEntry.killsPerMinute,
            vkpm: baseEntry.vehicleKills / Math.max(1, baseEntry.secondsWith) * 60,

            deaths: baseEntry.deaths,
            kd: baseEntry.killDeathRatio,
            accuracy: baseEntry.accuracy
        };
    }

    export const CharacterVehicleStats = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<CharacterWeaponStatEntry[]>,

                showImages: true as boolean,
                showDebug: false as boolean,
                showExtra: false as boolean
            }
        },

        beforeMount: function(): void {
            this.loadEntries();
        },

        methods: {
            getWeaponNameStyle: function(entry: CharacterWeaponStatEntry): object {
                return {
                    'height': (this.showImages == true) ? '3rem' : '1.5rem',
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

                const entries: Map<number, CharacterWeaponStatEntry[]> = new Map();

                for (const datum of this.entries.data) {
                    if (datum.vehicleID == 0) {
                        continue;
                    }

                    if (entries.has(datum.vehicleID) == false) {
                        entries.set(datum.vehicleID, []);
                    }

                    entries.get(datum.vehicleID)!.push(datum);
                }

                return Loadable.loaded(
                    Array.from(entries.values()).map(iter => weaponStatToVehicleStat(iter))
                );
            }
        },

        watch: {
            "character.id": function(): void {
                console.log(`NEW CHAR ID ${this.character.id}`);
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover,
            PercentileCell,
            CensusImage,
            ToggleButton
        }

    });
    export default CharacterVehicleStats;
</script>