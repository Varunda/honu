<template>
    <table class="wt-block table table-sm">
        <thead>
            <tr class="table-secondary th-border-top-0">
                <th style="width: 30ch">Outfit</th>
                <th title="Per Players (Total)">Kills</th>
                <th title="Revives remove a death, like in game">Deaths</th>
                <th title="Kills / Deaths">K/D</th>
                <th title="Characters who have been online (characters currently online)">
                    Characters
                    <info-hover text="How many characters have gotten kills in the period, and how many are currently online. The currently online can be higher as it includes people who have not gotten kills"></info-hover>
                </th>
            </tr>
        </thead>

        <tbody>
            <tr v-for="entry in block.entries">
                <td :title="entry.name">
                    <!--
                        <a style="flex-grow: 1; overflow: hidden; text-overflow: ellipsis;" :style="{ color: getFactionColor(entry.factionID) }" :href="'/c/' + entry.id">
                    -->
                    <a :href="'/o/' + entry.id" class="text-white" :style="{ color: getFactionColor(entry) }">
                        [{{entry.tag}}] {{entry.name}}
                    </a>
                </td>
                <td>
                    {{(entry.kills / (entry.members || 1)).toFixed(2)}}
                    <a @click="openOutfitKillers($event, entry.id)">
                        ({{entry.kills}})
                    </a>
                </td>
                <td>
                    {{(entry.deaths / (entry.members || 1)).toFixed(2)}}
                    ({{entry.deaths}})
                </td>
                <td>
                    {{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
                </td>
                <td>
                    {{entry.members}}
                    <a href="javascript:void(0);" @click="openMembersOnline($event, entry.id)" title="Members currently online">
                        ({{entry.membersOnline}})
                    </a>
                </td>
            </tr>
        </tbody>
    </table>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading } from "Loading";

    import EventBus from "EventBus";
    import { PopperModalData } from "popper/PopperModalData";

    import InfoHover from "components/InfoHover.vue";

    import { KillStatApi, OutfitKillerEntry } from "api/KillStatApi";
    import { PsCharacter } from "api/CharacterApi";
    import { OutfitApi } from "api/OutfitApi";
    import CharacterUtils from "util/Character";

    export const OutfitKillData = Vue.extend({
        props: {
            block: { required: true },
            title: { type: String, required: false, default: "Player" },
        },

        data: function () {
            return {

            }
        },

        methods: {
            getFactionColor: function(factionID: number): string {
                return "";
                //return FactionColors.getFactionColor(factionID) + "!important";
            },

            openMembersOnline: async function(event: any, outfitID: string): Promise<void> {
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Online members";
                modalData.columnFields = ["characterName"];
                modalData.columnNames = ["Character"];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                let online: Loading<PsCharacter[]> = await OutfitApi.getOnlineByOutfitID(outfitID);
                if (online.state != "loaded") {
                    console.warn(`failed to load online characters, ${online.state} not 'loaded'`);
                    return;
                }

                modalData.data = online.data.sort((a: PsCharacter, b: PsCharacter) => {
                    return a.name.localeCompare(b.name);
                }).map(iter => {
                    return {
                        characterName: CharacterUtils.getDisplay(iter),
                        characterID: iter.id
                    };
                });

                modalData.renderers.set("characterName", (data: any): string => {
                    if (data.characterID != "") {
                        return `<a href="/c/${data.characterID}">${data.characterName}</a>`;
                    }
                    return `${data.characterName}`;
                });
                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            },

            openOutfitKillers: async function(event: any, outfitID: string): Promise<void> {
                const modalData: PopperModalData = new PopperModalData();
                modalData.root = event.target;
                modalData.title = "Outfit top killers";
                modalData.columnFields = [ "characterName", "kills", "percent" ];
                modalData.columnNames = [ "Character", "Kills", "Percent" ];
                modalData.loading = true;

                EventBus.$emit("set-modal-data", modalData);

                let api: Loading<OutfitKillerEntry[]> = await KillStatApi.getOutfitKillers(outfitID);
                if (api.state != "loaded") {
                    console.warn(`Got ${api.state} not 'loaded'`);
                    return;
                }
                let kills: OutfitKillerEntry[] = api.data;
                const totalKills: number = kills.reduce((acc, iter) => acc + iter.kills, 0);

                // Trim to only show the top 6 killers
                if (kills.length > 7) {
                    const hiddenKillers: OutfitKillerEntry[] = kills.slice(6); // 6 to end
                    kills = kills.slice(0, 6); // 0 up to 6, or 0 - 5

                    kills.push({
                        characterID: "",
                        characterName:  `${hiddenKillers.length} others`,
                        kills: hiddenKillers.reduce((acc, iter) => acc + iter.kills, 0)
                    });
                }

                modalData.data = kills.map((iter: OutfitKillerEntry) => {
                    return {
                        ...iter,
                        percent: `${(iter.kills / totalKills * 100).toFixed(2)}%`
                    }
                });

                modalData.renderers.set("characterName", (data: any): string => {
                    if (data.characterID != "") {
                        return `<a href="/c/${data.characterID}">${data.characterName}</a>`;
                    }
                    return `${data.characterName}`;
                });
                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            }
        },

        components: {
            InfoHover
        }
    });
    export default OutfitKillData;
</script>