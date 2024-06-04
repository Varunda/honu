<template>
    <div>
        <div class="mb-2">
            <toggle-button v-model="showDebug">
                Show IDs
            </toggle-button>

            <toggle-button v-model="showImages">
                Show images
            </toggle-button>

            <toggle-button v-model="showVehicleNames">
                Show vehicle names
                <info-hover text="If vehicle names will be included in parenthesis for vehicle weapons" class="px-1"></info-hover>
            </toggle-button>

            <toggle-button v-model="showNonWeapons">
                Show non-weapons
                <info-hover text="Show all item stats, not just weapons. This will include items such as medkits and spawn beacons" class="px-1"></info-hover>
            </toggle-button>

            <toggle-button v-model="showExtraInfo">
                Show extra info
                <info-hover text="Show the raw values used to calculate each column. For example, show the number of shots and the number of shots hit, instead of just accuracy" class="px-1"></info-hover>
            </toggle-button>

            <toggle-button v-model="showPercent">
                Show percentiles
            </toggle-button>

            <toggle-button v-model="showCategory">
                Show item category
            </toggle-button>

            <a :href="'/api/character/' + character.id + '/weapon_stats'" class="btn btn-primary" target="_blank"
                :download="'honu-character-weapons-' + character.name + '-' + character.id + '.json'">

                Download JSON
                <span class="fa-fw fas fa-download"></span>
            </a>
        </div>

        <a-table v-if="showTable" class="mb-0"
            :entries="filteredEntries"
            :show-filters="true"
            :striped="false"
            default-sort-field="kills" default-sort-order="desc"
            display-type="table">

            <a-col sort-field="itemName">
                <a-header>
                    <b>Item</b>
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

                        <census-image v-if="showImages == true && entry.item != null && entry.item.imageID && entry.item.imageID != 0" :image-id="entry.item.imageID"
                            style="position: absolute; text-align: center; height: 100%; right: 0;" class="mr-1">
                        </census-image>

                        <a :href="entry.itemID != 0 ? ('/i/' + entry.itemID) : 'javascript:void(0);'" class="ml-1"
                            style="
                                position: absolute;
                                text-shadow: -1px -1px 2px rgb(32, 32, 32), -1px 1px 2px rgb(32, 32, 32), 1px -1px 2px rgb(32, 32, 32), 1px 1px 2px rgb(32, 32, 32);
                            ">

                            {{entry.itemName}}
                            <span v-if="showVehicleNames == true && entry.vehicleID != 0">
                                <span v-if="entry.vehicle != null">
                                    ({{entry.vehicle.name}})
                                </span>
                                <span v-else>
                                    (unknown {{entry.vehicleID}})
                                </span>
                            </span>
                            <span v-if="showDebug == true">
                                / {{entry.itemID}}
                            </span>
                        </a>
                    </div>
                </a-body>
            </a-col>

            <a-col v-if="showCategory" sort-field="categoryName">
                <a-header>
                    <b>Category</b>
                </a-header>

                <a-filter method="dropdown" type="string" field="categoryName" max-width="20ch"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.categoryName}}
                </a-body>
            </a-col>

            <a-col sort-field="kills">
                <a-header>
                    <span v-if="showNonWeapons == true">
                        <b>Kills / Uses</b>
                    </span>
                    
                    <b v-else>Kills</b>
                </a-header>

                <a-filter method="input" type="number" field="kills" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.kills | locale}}
                </a-body>
            </a-col>

            <a-col sort-field="killsPerMinute">
                <a-header>
                    <b title="Kills per minute">KPM</b>
                    <info-hover text="Kills per minute"></info-hover>
                </a-header>

                <a-filter method="input" type="number" field="killsPerMinute" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.killsPerMinute.toFixed(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="kpmPercent">
                <a-header>
                    <b>KPM%</b>
                    <info-hover text="What percentage of players have lower KPM.<br> E.G. 80% would mean this KPM is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell v-show="showPercent" :value="entry.kpmPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="killDeathRatio">
                <a-header>
                    <b title="Kills / Deaths (revive are not counted)">K/D</b>
                </a-header>

                <a-filter method="input" type="number" field="killDeathRatio" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.killDeathRatio.toFixed(2)}}

                    <span v-if="showExtraInfo == true">
                        ({{entry.kills | locale}}/{{entry.deaths | locale}})
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="kdPercent">
                <a-header>
                    <b>KD%</b>
                    <info-hover text="What percentage of players have lower KD.<br> E.G. 80% would mean this KD is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell v-show="showPercent" :value="entry.kdPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="accuracy">
                <a-header>
                    <b title="Accuracy">ACC</b>
                </a-header>

                <a-filter method="input" type="number" field="accuracy" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.accuracy.toFixed(2)}}%
                    <span v-if="showExtraInfo == true">
                        ({{entry.shotsHit | locale}}/{{entry.shots | locale}})
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="accPercent">
                <a-header>
                    <b>ACC%</b>
                    <info-hover text="What percentage of players have lower accuracy.<br> E.G. 80% would mean this accuracy is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell v-show="showPercent" :value="entry.accPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="headshotRatio">
                <a-header>
                    <b title="Percentage of kills that end with a headshot">HSR</b>
                </a-header>

                <a-filter method="input" type="number" field="headshotRatio" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.headshotRatio.toFixed(2)}}%

                    <span v-if="showExtraInfo == true">
                        ({{entry.headshots | locale}}/{{entry.kills | locale}})
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="hsrPercent">
                <a-header>
                    <b>HSR%</b>
                    <info-hover text="What percentage of players have lower HSR.<br> E.G. 80% would mean this HSR is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell v-show="showPercent" :value="entry.hsrPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="vehicleKills">
                <a-header>
                    <b>VKills</b>
                </a-header>

                <a-filter method="input" type="number" field="vehicleKills" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]"> 
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.vehicleKills | locale(0)}}
                </a-body>
            </a-col>

            <a-col sort-field="vehicleKillsPerMinute">
                <a-header>
                    <b>VKPM</b>
                </a-header>

                <a-filter method="input" type="number" field="headshotRatio" max-width="14ch"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.vehicleKillsPerMinute.toFixed(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="vkpmPercent">
                <a-header>
                    <b>VKPM%</b>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell v-show="showPercent" :value="entry.vkpmPercent"></percentile-cell>
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

        <div class="text-center mb-3 alert alert-secondary">
            The aurax progress colors behind weapons (purple, gold, silver, and bronze) are based on how many kills with the weapon, not the actual progress in game.
        </div>


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
    import { ItemCategory, ItemCategoryApi } from "api/ItemCategoryApi";

    export const CharacterWeaponStats = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<CharacterWeaponStatEntry[]>,

                showTable: true as boolean,

                showDebug: false as boolean,
                showImages: true as boolean,
                showNonWeapons: false as boolean,
                showExtraInfo: false as boolean,
                showPercent: true as boolean,
                showVehicleNames: true as boolean,
                showCategory: true as boolean
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

                if (this.entries.state == "loaded") {
                    const map: Map<string, CharacterWeaponStatEntry> = new Map();

                    // removes possible duplicates by only using the most recent item data
                    // this is due to bad data, where some vehicle weapons have duplicate entries
                    for (const entry of this.entries.data) {
                        let key = `${entry.itemID}-${entry.kills}`;

                        if (entry.itemID == "0") {
                            key = `${entry.itemID}-${entry.vehicleID}`;
                            map.set(key, entry);
                            continue;
                        }

                        if (map.has(key) == false) {
                            map.set(key, entry);
                        } else {
                            const prev: CharacterWeaponStatEntry = map.get(key)!;
                            if (entry.timestamp.getTime() > prev.timestamp.getTime() && entry.kills > 0) {
                                map.set(key, entry);
                            }
                        }
                    }

                    this.entries = Loadable.loaded(Array.from(map.values()));
                }
            },

            toggleTable: async function(): Promise<void> {
                this.showTable = false;
                await this.$nextTick();
                this.showTable = true;

            }
        },

        computed: {
            filteredEntries: function(): Loading<CharacterWeaponStatEntry[]> {
                if (this.entries.state != "loaded") {
                    return this.entries;
                }

                return Loadable.loaded(this.entries.data.filter(iter => {
                    if (iter.itemID == "0" && iter.vehicleID != 0) {
                        return false;
                    }

                    if (this.showNonWeapons == false) {
                        return iter.item == null || (iter.item.categoryID != 139 && (((iter.item.categoryID == 104 && iter.kills > 0) || iter.item.categoryID != 104)));
                    }
                    return true;
                }));
            }
        },

        watch: {
            "character.id": function(): void {
                console.log(`NEW CHAR ID ${this.character.id}`);
            },

            showCategory: function(): void {
                this.toggleTable();
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
    export default CharacterWeaponStats;
</script>
