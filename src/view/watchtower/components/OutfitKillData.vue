<template>
    <table class="wt-block table table-sm">
        <thead>
            <tr class="table-secondary">
                <th style="width: 30ch">Outfit</th>
                <th title="Per Players (Total)">Kills</th>
                <th title="Revives remove a death, like in game">Deaths</th>
                <th title="Kills / Deaths">K/D</th>
                <th>Characters</th>
            </tr>
        </thead>

        <tbody>
            <tr v-for="entry in block.entries">
                <td :title="entry.name">
                    <a :href="'/o/' + entry.id" class="text-white">
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
                <td>{{entry.members}}</td>
            </tr>
        </tbody>
    </table>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading } from "Loading";

    import EventBus from "EventBus";

    import { PopperModalData } from "popper/PopperModalData";
    import { KillStatApi, OutfitKillerEntry } from "api/KillStatApi";

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
                modalData.loading = false;

                EventBus.$emit("set-modal-data", modalData);
            }
        },
    });
    export default OutfitKillData;
</script>