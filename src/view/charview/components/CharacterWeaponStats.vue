<template>
    <div>
        <div>
            <button type="button" class="btn btn-secondary" @click="showDebug = !showDebug">
                Show IDs
            </button>

            <button type="button" class="btn btn-primary" @click="loadEntries">
                Reload
            </button>
        </div>

        <a-table
            :entries="entries"
            :show-filters="true"
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
                    <div style="height: 3rem; position: relative;">
                        <a :href="'/i/' + entry.itemID" style="position: absolute;">
                            {{entry.itemName}}
                            <span v-if="showDebug == true">
                                / {{entry.itemID}}
                            </span>
                        </a>

                        <census-image v-if="entry.item.imageID && entry.item.imageID != 0" :image-id="entry.item.imageID"
                            style="position: absolute; text-align: center; height: 100%; right: 0;">
                        </census-image>
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="kills">
                <a-header>
                    <b>Kills</b>
                </a-header>

                <a-filter method="input" type="number" field="kills"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.kills}}
                </a-body>
            </a-col>

            <a-col sort-field="killsPerMinute">
                <a-header>
                    <b title="Kills per minute">KPM</b>
                    <info-hover text="Kills per minute"></info-hover>
                </a-header>

                <a-filter method="input" type="number" field="killsPerMinute"
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
                    <percentile-cell :value="entry.kpmPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="killDeathRatio">
                <a-header>
                    <b title="Kills / Deaths (revive are not counted)">K/D</b>
                </a-header>

                <a-filter method="input" type="number" field="killDeathRatio"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.killDeathRatio.toFixed(2)}}
                </a-body>
            </a-col>

            <a-col sort-field="kdPercent">
                <a-header>
                    <b>KD%</b>
                    <info-hover text="What percentage of players have lower KD.<br> E.G. 80% would mean this KD is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell :value="entry.kdPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="accuracy">
                <a-header>
                    <b title="Accuracy">ACC</b>
                </a-header>

                <a-filter method="input" type="number" field="accuracy"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.accuracy.toFixed(2)}}%
                </a-body>
            </a-col>

            <a-col sort-field="accPercent">
                <a-header>
                    <b>ACC%</b>
                    <info-hover text="What percentage of players have lower accuracy.<br> E.G. 80% would mean this accuracy is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell :value="entry.accPercent"></percentile-cell>
                </a-body>
            </a-col>

            <a-col sort-field="headshotRatio">
                <a-header>
                    <b title="Percentage of kills that end with a headshot">HSR</b>
                </a-header>

                <a-filter method="input" type="number" field="headshotRatio"
                    :conditions="[ 'greater_than', 'less_than' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.headshotRatio.toFixed(2)}}%
                </a-body>
            </a-col>

            <a-col sort-field="hsrPercent">
                <a-header>
                    <b>HSR%</b>
                    <info-hover text="What percentage of players have lower HSR.<br> E.G. 80% would mean this HSR is in the 80th percentile" :allow-html="true"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <percentile-cell :value="entry.hsrPercent"></percentile-cell>
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

    import PercentileCell from "./PercentileCell.vue";

    import "MomentFilter";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter } from "api/CharacterApi";
    import { CharacterWeaponStatEntry, CharacterWeaponStatApi } from "api/CharacterWeaponStatApi";

    export const CharacterWeaponStats = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                entries: Loadable.idle() as Loading<CharacterWeaponStatEntry[]>,
                showDebug: false as boolean
            }
        },

        beforeMount: function(): void {
            console.log(`char id is ${this.character.id}`);
            this.loadEntries();
        },

        methods: {
            loadEntries: async function(): Promise<void> {
                this.entries = Loadable.loading();
                this.entries = await CharacterWeaponStatApi.getByCharacterID(this.character.id);
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
            CensusImage
        }

    });
    export default CharacterWeaponStats;
</script>
