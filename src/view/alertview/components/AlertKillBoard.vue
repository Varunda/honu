<template>
    <a-table
        :entries="participants"
        :show-filters="true" :default-page-size="25"
        default-sort-field="kills" default-sort-order="desc"
        display-type="table" row-padding="compact">

        <a-col sort-field="characterName">
            <a-header>
                <b>Character</b>
            </a-header>

            <a-filter field="characterName" type="string" method="input"
                :conditions="[ 'contains' ]">
            </a-filter>

            <a-body v-slot="entry">
                <a :href="'/c/' + entry.characterID" :style="{ color: getFactionColor(entry.factionID) }">
                    <span v-if="entry.outfitID != null">
                        [{{entry.outfitTag}}]
                    </span>
                    {{entry.characterName}}
                </a>
            </a-body>
        </a-col>

        <a-col sort-field="factionID">
            <a-header>
                <b>Faction</b>
            </a-header>

            <a-filter field="factionID" type="number" method="dropdown" :source="sources.factions" source-key="key" source-value="value"
                :conditions="['equals']">
            </a-filter>

            <a-body v-slot="entry">
                {{entry.factionID | faction}}
            </a-body>
        </a-col>

        <a-col sort-field="kills">
            <a-header>
                <b>Kills</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacterKills($event, entry.characterID)">
                    {{entry.kills}}
                </a>
            </a-body>
        </a-col>

        <a-col sort-field="kpm">
            <a-header>
                <b>KPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.kpm | locale(2)}}
            </a-body>
        </a-col>

        <a-col>
            <a-header>
                <b>Deaths</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.deaths}}
            </a-body>
        </a-col>

        <a-col sort-field="kd">
            <a-header>
                <b>K/D</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.kd | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="secondsOnline">
            <a-header>
                <b>Time online</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacterSessions($event, entry.characterID)">
                    {{entry.secondsOnline | mduration}}
                </a>
            </a-body>
        </a-col>
    </a-table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import { PsAlert } from "api/AlertApi";
    import { AlertParticipantApi, FlattendParticipantDataEntry } from "api/AlertParticipantApi";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "filters/FixedFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";

    import TableDataSource from "view/alertview/TableDataSource";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    export const AlertKillBoard = Vue.extend({
        props: {
            alert: { type: Object as PropType<PsAlert>, required: true },
            participants: { type: Object as PropType<Loading<FlattendParticipantDataEntry[]>>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            getFactionColor: function(factionID: number): string {
                return ColorUtils.getFactionColor(factionID) + " !important";
            },

            openCharacterSessions: async function(event: any, characterID: string): Promise<void> {
                await TableDataSource.openCharacterSessions(event, this.alert, characterID);
            },

            openCharacterKills: async function(event: any, characterID: string): Promise<void> {
                await TableDataSource.openCharacterKills(event, this.alert, characterID);
            }
        },

        computed: {
            sources: function() {
                return {
                    factions: [
                        { key: "All", value: null },
                        { key: "VS", value: 1 },
                        { key: "NC", value: 2 },
                        { key: "TR", value: 3 },
                        { key: "NS", value: 4 },
                    ]
                }
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader
        }
    });
    export default AlertKillBoard;
</script>