<template>
    <table class="wt-block table table-sm">
        <thead>
            <tr class="table-secondary th-border-top-0">
                <th style="width: 30ch">Player</th>
                <th>Kills</th>
                <th title="Kills / Minutes Online">KPM</th>
                <th title="Revives remove deaths">
                    Deaths
                    <info-hover text="Revives remove a death, like in game"></info-hover>
                </th>
                <th>Assists</th>
                <th title="Kills / Deaths">K/D</th>
                <th title="(Kills + Assists) / Deaths">KDA</th>
            </tr>
        </thead>

        <tbody>
            <tr v-for="entry in block.playerKills.entries">
                <td :title="entry.name">
                    <span style="display: flex;">
                        <span v-if="entry.online == true" style="color: green;" title="Online">
                            ●
                        </span>
                        <span v-else style="color: red;" title="Offline">
                            ●
                        </span>

                        <a style="flex-grow: 1; overflow: hidden; text-overflow: ellipsis;" :style="{ color: getFactionColor(entry.factionID) }" :href="'/c/' + entry.id">
                            {{entry.name}}
                        </a>

                        <a style="flex-grow: 0;" @click="openCharacterSessions($event, entry.id);" href="javascript:void(0);" class="wt-click">
                            {{entry.secondsOnline | duration}}
                        </a>
                    </span>
                </td>
                <td>
                    <a @click="openCharacterWeaponKills($event, entry.id);" href="javascript:void(0);" class="wt-click">
                        {{entry.kills}}
                    </a>
                </td>
                <td>{{(entry.kills / (entry.secondsOnline / 60)).toFixed(2)}}</td>
                <td>{{entry.deaths}}</td>
                <td>{{entry.assists}}</td>
                <td>
                    {{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
                </td>
                <td>
                    {{((entry.kills + entry.assists) / (entry.deaths || 1)).toFixed(2)}}
                </td>
            </tr>
            <tr class="table-secondary th-border-top-0">
                <td><b>Total</b></td>
                <td colspan="2">{{block.totalKills}}</td>
                <td>{{block.totalDeaths}}</td>
                <td>{{block.totalAssists}}</td>
                <td>
                    {{(block.totalKills / (block.totalDeaths || 1)).toFixed(2)}}
                </td>
                <td>
                    {{((block.totalKills + block.totalAssists) / (block.totalDeaths || 1)).toFixed(2)}}
                </td>
            </tr>
        </tbody>
    </table>
</template>

<script lang="ts">
    import Vue from "vue";
    import EventBus from "EventBus";
    import { Loading } from "Loading";

    import FactionColors from "FactionColors";
    import { PopperModalData } from "popper/PopperModalData";

    import { KillStatApi, CharacterWeaponKillEntry } from "api/KillStatApi";
    import { Session, SessionApi } from "api/SessionApi";

    import InfoHover from "components/InfoHover.vue";
    import TimeUtils from "util/Time";

    export const KillData = Vue.extend({
        props: {
            block: { required: true },
            title: { type: String, required: false, default: "Player" },

            UseShort: { type: Boolean, required: true }
        },

        data: function () {
            return {

            }
        },

        methods: {
            getFactionColor: function(factionID: number): string {
                return FactionColors.getFactionColor(factionID) + "!important";
            },

            openCharacterSessions: async function(event: any, charID: string): Promise<void> {
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Session";
                modalData.columnFields = [ "sessionID", "start", "end"];
                modalData.columnNames = [ "ID", "Start", "End" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                const intervalStart: Date = new Date((new Date()).getTime() - (((this.UseShort == true) ? 120 : 60) * 60 * 1000));

                const api: Loading<Session[]> = await SessionApi.getByCharacterIDAndPeriod(charID, intervalStart, new Date());
                if (api.state != "loaded") {
                    console.warn(`cannot load character session info: ${api.state} is not 'loaded'`);
                    return;
                }

                let sessions: Session[] = api.data.sort((a, b) => b.start.getTime() - a.start.getTime());
                modalData.data = sessions.map((iter: Session) => {
                    const start: string = TimeUtils.format(iter.start);
                    const end: string = iter.end == null ? "current" : TimeUtils.format(iter.end);
                    return {
                        sessionID: iter.id,
                        start: start,
                        end: end
                    }
                });

                modalData.renderers.set("sessionID", (data: any): string => {
                    return `<a href="/s/${data.sessionID}">${data.sessionID}</a>`;
                });

                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            },

            openCharacterWeaponKills: async function(event: any, charID: string): Promise<void> {
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Weapon usage";
                modalData.columnFields = [ "weaponName", "kills", "headshotRatio", "percent" ];
                modalData.columnNames = [ "Weapon", "Kills", "Headshots", "Usage" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                const api: Loading<CharacterWeaponKillEntry[]> = await KillStatApi.getWeaponEntries(charID, this.UseShort);
                if (api.state != "loaded") {
                    console.warn(`Got ${api.state} not 'loaded'`);
                    return;
                }
                let kills = api.data;
                const totalKills: number = kills.reduce((acc, iter) => acc + iter.kills, 0);

                modalData.data = kills.map((iter: CharacterWeaponKillEntry) => {
                    return {
                        ...iter,
                        weaponID: iter.weaponID,
                        headshotRatio: `${(iter.headshotKills / iter.kills * 100).toFixed(2)}%`,
                        percent: `${(iter.kills / totalKills * 100).toFixed(2)}%`
                    }
                });

                modalData.renderers.set("weaponName", (data: any): string => {
                    return `<a href="/i/${data.weaponID}">${data.weaponName}</a>`;
                });

                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            }
        },

        components: {
            InfoHover
        }
    });
    export default KillData;
</script>