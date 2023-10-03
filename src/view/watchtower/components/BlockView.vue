<template>
    <table class="wt-block table table-sm">
        <thead>
            <tr class="table-secondary th-border-top-0">
                <th>{{title}}</th>
                <th style="width: 12ch">
                    <info-hover text="Click green links to view more info"></info-hover>
                    Amount
                </th>
            </tr>
        </thead>

        <tbody>
            <tr v-for="entry in block.entries">
                <td :title="entry.name" :style="{ color: getFactionColor(entry.factionID) }">
                    <a v-if="link != null && entry.id != '0'" :href="link + entry.id" :style="{ color: getFactionColor(entry.factionID) }">
                        {{entry.name}}
                    </a>
                    <span v-else>
                        {{entry.name}}
                    </span>
                </td>
                <td>
                    <a v-if="source" @click="clickHandler($event, entry.id)" href="javascript:void(0);" class="wt-click">
                        {{entry.value}} 
                    </a>

                    <span v-else>
                        {{entry.value}}
                    </span>

                    / {{(entry.value / block.total * 100).toFixed(2)}}%
                </td>
            </tr>
            <tr class="table-secondary th-border-top-0">
                <th colspan="2">
                    Total: {{block.total}}
                </th>
            </tr>
        </tbody>
    </table>
</template>

<script lang="ts">
    import Vue from "vue";
    import EventBus from "EventBus";
    import { Loading, Loadable } from "Loading";

    import FactionColors from "FactionColors";
    import { PopperModalData } from "popper/PopperModalData";
    import { CharacterExpSupportEntry } from "api/ExpStatApi";
    import InfoHover from "components/InfoHover.vue";

    export const BlockView = Vue.extend({
        props: {
            block: { required: true },
            title: { type: String, required: false, default: "Player" },

            source: { type: Function, required: false, default: null },
            link: { type: String, required: false },

            SourceLimit: { type: Number, required: false, default: 6 },
            SourceWorldId: { type: Number, required: false },
            SourceTeamId: { type: Number, required: false },
            SourceTitle: { type: String, required: false, default: "Supported" },
            SourceUseShort: { type: Boolean, required: true }
        },

        data: function () {
            return {

            }
        },

        methods: {
            getFactionColor: function (factionID: number): string {
                return FactionColors.getFactionColor(factionID) + " !important";
            },

            clickHandler: async function(event: any, charID: string): Promise<void> {
                if (this.source) {
                    const modalData: PopperModalData = new PopperModalData();
                    modalData.root = event.target;
                    modalData.title = this.SourceTitle;
                    modalData.columnFields = [ "characterName", "amount", "percent" ];
                    modalData.columnNames = [ "Character", "Amount", "Percent" ];
                    modalData.loading = true;

                    EventBus.$emit("set-modal-data", modalData);

                    const api: Loading<CharacterExpSupportEntry[]> = await this.source(charID, this.SourceUseShort, this.SourceWorldId, this.SourceTeamId);
                    if (api.state != "loaded") {
                        console.warn(`Got ${api.state} not 'loaded'`);
                        return;
                    }
                    let data: CharacterExpSupportEntry[] = api.data;
                    const total: number = data.reduce((acc, iter) => acc + iter.amount, 0);

                    // Trim to only show the top 6 killers
                    if (data.length > (this.SourceLimit + 1)) {
                        const hidden: CharacterExpSupportEntry[] = data.slice(this.SourceLimit);
                        data = data.slice(0, this.SourceLimit);

                        data.push({
                            characterID: "",
                            characterName:  `${hidden.length} others`,
                            amount: hidden.reduce((acc, iter) => acc + iter.amount, 0)
                        });
                    }

                    modalData.data = data.map((iter: CharacterExpSupportEntry) => {
                        return {
                            characterID: iter.characterID,
                            characterName: iter.characterName,
                            amount: iter.amount,
                            percent: `${(iter.amount / total * 100).toFixed(2)}%`
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
            }

        },

        components: {
            InfoHover
        }
    });

    export default BlockView;
</script>