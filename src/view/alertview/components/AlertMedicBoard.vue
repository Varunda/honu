<template>
    <a-table
        :entries="entries"
        :show-filters="true"
        default-sort-field="revives" default-sort-order="desc" :default-page-size="10"
        display-type="table" row-padding="compact">

        <a-col sort-field="name">
            <a-header>
                <b>Character</b>
            </a-header>

            <a-filter field="name" type="string" method="input"
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

            <a-filter field="factionID" type="number" method="dropdown" :source="sources.factions" source-key="key" source-value="value" :sort-values="false"
                :conditions="['equals']">
            </a-filter>

            <a-body v-slot="entry">
                {{entry.factionID | faction}}
            </a-body>
        </a-col>

        <a-col sort-field="revives">
            <a-header>
                <b>Revives</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacterRevives($event, entry.id)">
                    {{entry.revives}}
                </a>
            </a-body>
        </a-col>

        <a-col sort-field="revivesPerMinute">
            <a-header>
                <b>RPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.revivesPerMinute | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="heals">
            <a-header>
                <b>Heals</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacterHeals($event, entry.id)">
                    {{entry.heals}}
                </a>
            </a-body>
        </a-col>

        <a-col sort-field="healsPerMinute">
            <a-header>
                <b>HPM</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.healsPerMinute | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="kills">
            <a-header>
                <b>Kills</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacterKills($event, entry.id)">
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

        <a-col sort-field="krd">
            <a-header>
                <b>K+R/D</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.krd | locale(2)}}
            </a-body>
        </a-col>

        <a-col sort-field="timeAs">
            <a-header>
                <b>Time online</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacterSessions($event, entry.id)">
                    {{entry.timeAs | mduration}}
                </a>
            </a-body>
        </a-col>

    </a-table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import { AlertParticipantApi, AlertPlayerProfileData, FlattendParticipantDataEntry } from "api/AlertParticipantApi";
    import { PsAlert } from "api/AlertApi";
    import { MedicTableData, TableData } from "../TableData";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";
    import CharacterUtil from "util/Character";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import TableDataSource from "../TableDataSource";

    export const AlertMedicBoard = Vue.extend({
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

            openCharacterHeals: async function(event: any, characterID: string): Promise<void> {
                await TableDataSource.openCharacterHeals(event, this.alert, characterID);
            },

            openCharacterRevives: async function(event: any, characterID: string): Promise<void> {
                await TableDataSource.openCharacterRevives(event, this.alert, characterID);
            },

            openCharacterKills: async function(event: any, characterID: string): Promise<void> {
                await TableDataSource.openCharacterKills(event, this.alert, characterID);
            },

            openCharacterSessions: async function(event: any, characterID: string): Promise<void> {
                await TableDataSource.openCharacterSessions(event, this.alert, characterID);
            }
        },

        computed: {
            entries: function(): Loading<MedicTableData[]> {
                if (this.participants.state != "loaded") {
                    return Loadable.rewrap<FlattendParticipantDataEntry[], MedicTableData[]>(this.participants);
                }

                return Loadable.loaded(TableData.getMedicData(this.participants.data));
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
    export default AlertMedicBoard;
</script>
