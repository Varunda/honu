<template>
    <div>
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
                    {{entry.itemName}} / {{entry.itemID}}
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
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.kpmPercent != null">
                        {{(entry.kpmPercent).toFixed(2)}}%
                    </span>
                    <span v-else>
                        --
                    </span>
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
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.kdPercent != null">
                        {{(entry.kdPercent).toFixed(2)}}%
                    </span>
                    <span v-else>
                        --
                    </span>
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
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.accPercent != null">
                        {{(entry.accPercent).toFixed(2)}}%
                    </span>
                    <span v-else>
                        --
                    </span>
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
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.hsrPercent != null">
                        {{(entry.hsrPercent).toFixed(2)}}%
                    </span>
                    <span v-else>
                        --
                    </span>
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
                entries: Loadable.idle() as Loading<CharacterWeaponStatEntry[]>
            }
        },

        beforeMount: function(): void {
            console.log(`char id is ${this.character.id}`);
            this.loadEntries();
        },

        methods: {
            loadEntries: async function(): Promise<void> {
                this.entries = Loadable.loading();
                try {
                    this.entries = Loadable.loaded(await CharacterWeaponStatApi.getByCharacterID(this.character.id));
                } catch (err: any) {
                    this.entries = Loadable.error(err);
                }
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
        }

    });
    export default CharacterWeaponStats;
</script>
