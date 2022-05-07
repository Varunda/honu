<template>
    <a-table
        :entries="entries"
        :show-filters="true"
        default-sort-field="resupplies" default-sort-order="desc" :default-page-size="10"
        display-type="table" row-padding="compact">

        <a-col sort-field="characterName">
            <a-header>
                <b>Character</b>
            </a-header>

            <a-filter field="characterName" type="string" method="input"
                :conditions="[ 'contains' ]">
            </a-filter>

            <a-body v-slot="entry">
                <a :href="'/c/' + entry.id" :style="{ color: getFactionColor(entry.factionID) }">
                    <span v-if="entry.outfitID != null">
                        [{{entry.outfitTag}}]
                    </span>
                    {{entry.name}}
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
                {{entry.kills}}
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

        <a-col sort-field="deaths">
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

        <a-col sort-field="resupplies">
            <a-header>
                <b>Resupplies</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.resupplies}}
            </a-body>
        </a-col>

        <a-col sort-field="resuppliesPerMinute">
            <a-header>
                <b>Resupplies per minute</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.resuppliesPerMinute | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="repairs">
            <a-header>
                <b>Repairs</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.repairs}}
            </a-body>
        </a-col>

        <a-col sort-field="repairsPerMinute">
            <a-header>
                <b>Repairs per minute</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.repairsPerMinute | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="timeAs">
            <a-header>
                <b>Time online</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.timeAs | mduration}}
            </a-body>
        </a-col>
    </a-table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import { AlertParticipantApi, FlattendParticipantDataEntry } from "api/AlertParticipantApi";
    import { EngineerTableData, TableData } from "../TableData";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    export const AlertEngineeringBoard = Vue.extend({
        props: {
            participants: { type: Object as PropType<Loading<FlattendParticipantDataEntry[]>>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            getFactionColor: function(factionID: number): string {
                return ColorUtils.getFactionColor(factionID) + " !important";
            }
        },

        computed: {
            entries: function(): Loading<EngineerTableData[]> {
                if (this.participants.state != "loaded") {
                    return Loadable.rewrap<FlattendParticipantDataEntry[], EngineerTableData[]>(this.participants);
                }

                return Loadable.loaded(TableData.getEngineerData(this.participants.data));
            },

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
    export default AlertEngineeringBoard;
</script>
