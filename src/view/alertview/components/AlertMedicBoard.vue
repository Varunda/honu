<template>
    <a-table
        :entries="entries"
        :show-filters="true"
        default-sort-field="revives" default-sort-order="desc" :default-page-size="10"
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

        <a-col sort-field="revives">
            <a-header>
                <b>Revives</b>
            </a-header>

            <a-body v-slot="entry">
                {{entry.revives}}
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

        <a-col sort-field="heals">
            <a-header>
                <b>Heals</b>
            </a-header>

            <a-body v-slot="entry">
                <a @click="openCharacter($event, entry.characterID)">
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

    import { PopperModalData } from "popper/PopperModalData";
    import EventBus from "EventBus";

    import { AlertParticipantApi, AlertPlayerProfileData, FlattendParticipantDataEntry } from "api/AlertParticipantApi";
    import { ExpandedExpEvent, ExpStatApi, Experience } from "api/ExpStatApi";
    import { PsAlert } from "api/AlertApi";
    import { MedicTableData, TableData } from "../TableData";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";
    import CharacterUtil from "util/Character";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

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

            openCharacter: async function(event: any, characterID: string): Promise<void> {
                if (this.participants.state != "loaded") {
                    return;
                }

                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Player kills";
                modalData.columnFields = [ "characterName", "amount", "percent" ];
                modalData.columnNames = [ "Character", "Amount", "Percent" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                const expEvents: Loading<ExpandedExpEvent[]> = await ExpStatApi.getByCharacterIDAndRange(characterID, this.alert.timestamp, this.alert.end);
                if (expEvents.state == "loaded") {
                    const healEvents: ExpandedExpEvent[] = expEvents.data.filter(iter => {
                        return iter.event.experienceID == Experience.HEAL || iter.event.experienceID == Experience.SQUAD_HEAL;
                    });

                    const healedCharacters: string[] = healEvents.map(iter => iter.event.otherID).filter((v, i, a) => a.indexOf(v) == i);

                    modalData.data = healedCharacters.map((characterID: string) => {
                        const charEvents: ExpandedExpEvent[] = healEvents.filter(iter => iter.event.otherID == characterID);
                        if (charEvents.length == 0) {
                            throw `how does ${characterID} have 0 heal events`;
                        }

                        return {
                            characterName: charEvents[0].other != null ? CharacterUtil.getDisplay(charEvents[0].other) : `<missing ${charEvents[0].event.otherID}>`,
                            amount: charEvents.length,
                            percent: `${(charEvents.length / healEvents.length * 100).toFixed(2)}%`
                        };
                    }).sort((a, b) => b.amount - a.amount);
                }

                modalData.loading = false;
                EventBus.$emit("set-modal-data", modalData);
            },
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
