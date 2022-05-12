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

            <a-filter field="name" type="string" method="input"
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
                {{entry.kd | locale}}
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

    import { PopperModalData } from "popper/PopperModalData";
    import EventBus from "EventBus";

    import { PsAlert } from "api/AlertApi";
    import { AlertParticipantApi, FlattendParticipantDataEntry } from "api/AlertParticipantApi";
    import { ExpandedKillEvent, KillStatApi } from "api/KillStatApi";
    import { Session, SessionApi } from "api/SessionApi";

    import "filters/LocaleFilter";
    import "filters/FactionNameFilter";
    import "filters/FixedFilter";
    import "MomentFilter";

    import ColorUtils from "util/Color";
    import TimeUtils from "util/Time";

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
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Sessions";
                modalData.columnFields = [ "session", "start", "end", "duration" ];
                modalData.columnNames = [ "Session", "Start", "End", "Duration" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                const data: Loading<Session[]> = await SessionApi.getByCharacterIDAndPeriod(characterID, this.alert.timestamp, this.alert.end);

                if (data.state == "loaded") {
                    modalData.data = data.data.map(iter => {
                        return {
                            session: iter.id,
                            start: TimeUtils.format(iter.start),
                            end: iter.end == null ? "<in progress>" : TimeUtils.format(iter.end),
                            duration: TimeUtils.duration(((iter.end ?? new Date()).getTime() - iter.start.getTime()) / 1000)
                        }
                    }).sort((a, b) => a.session - b.session);
                }

                modalData.renderers.set("session", (data: any): string => {
                    return `<a href="/s/${data.session}">${data.session}</a>`;
                });

                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            },

            openCharacterKills: async function(event: any, characterID: string): Promise<void> {
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Weapons used";
                modalData.columnFields = [ "itemName", "kills", "headshotRatio", "percent" ];
                modalData.columnNames = [ "Weapon", "Kills", "Headshots", "Usage" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                const data: Loading<ExpandedKillEvent[]> = await KillStatApi.getByRange(characterID, this.alert.timestamp, this.alert.end);
                if (data.state == "loaded") {
                    const kills = data.data.filter(iter => iter.event.attackerCharacterID == characterID && iter.event.attackerTeamID != iter.event.killedTeamID);

                    const weaponsUsed: number[] = kills.map(iter => iter.event.weaponID)
                        .filter((v, i, a) => a.indexOf(v) == i);

                    modalData.data = weaponsUsed.map(weaponID => {
                        const itemKills: ExpandedKillEvent[] = kills.filter(iter => iter.event.weaponID == weaponID);

                        if (itemKills.length == 0) {
                            throw `why is there 0 kills but we have a weapon ID for it`;
                        }

                        let itemName: string = itemKills[0].item?.name ?? `<unknown ${itemKills[0].event.weaponID}`;
                        if (weaponID == 0) {
                            itemName = "<no weapon>";
                        }

                        const headshotKills: number = itemKills.filter(iter => iter.event.isHeadshot == true).length;

                        return {
                            itemID: weaponID,
                            itemName: itemName,
                            kills: itemKills.length,
                            headshotRatio: `${(headshotKills / itemKills.length * 100).toFixed(2)}%`,
                            percent: `${(itemKills.length / kills.length * 100).toFixed(2)}%`
                        }
                    }).sort((a, b) => b.kills - a.kills);
                }

                modalData.renderers.set("itemName", (data: any): string => {
                    if (data.itemID == 0) {
                        return `<span>&lt;no weapon&gt;</span>`;
                    }
                    return `<a href="/i/${data.itemID}">${data.itemName}</a>`;
                });

                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
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