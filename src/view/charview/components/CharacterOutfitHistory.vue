<template>
    <div>
        <div v-if="outfitHistory.state == 'idle'"></div>

        <div v-else-if="outfitHistory.state == 'loading'">
            <busy class="honu-busy honu-busy-lg"></busy>
        </div>

        <div v-else-if="outfitHistory.state == 'loaded'">
            <a-table 
                :entries="entries"
                :striped="true"
                default-sort-field="start" default-sort-order="desc"
                display-type="table">

                <a-col>
                    <a-header>
                        <b>Outfit</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.outfitID == '0' || entry.outfitID == ''">
                            &lt;no outfit&gt;
                        </span>

                        <a v-else :href="'/o/' + entry.outfitID">
                            <span v-if="entry.outfit != null">
                                [{{entry.outfit.tag}}] {{entry.outfit.name}}
                            </span>
                            <span v-else>
                                &lt;missing {{entry.outfitID}}&gt;
                            </span>
                        </a>
                    </a-body>
                </a-col>

                <a-col sort-field="start">
                    <a-header>
                        <b>Joined</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.start | moment}}
                    </a-body>
                </a-col>

                <a-col sort-field="end">
                    <a-header>
                        <b>Left</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.current == true">
                            <b>current outfit</b>
                        </span>
                        <span v-else>
                            {{entry.end | moment}}
                        </span>
                    </a-body>
                </a-col>

                <a-col>
                    <a-header>
                        <b>Duration</b>
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.duration | tduration}}
                    </a-body>
                </a-col>
            </a-table>
        </div>

        <hr class="border" />

        <div class="d-flex mb-2">
            <div class="flex-grow-1"></div>

            <div class="text-center">
                DISCLAIMER: this is not 100% accurate, and does contain inaccuracies. 
                <br />
                If a character switches outfits quickly (multiple times a week), or multiple times per session, honu will not be track that change.
                <br />
                This data was only collected after 2021-07-23, and does not track changes before that
            </div>

            <div class="flex-grow-1"></div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";

    import "MomentFilter";
    import "filters/LocaleFilter";
    import "filters/FixedFilter";

    import { PsCharacter } from "api/CharacterApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { OutfitHistoryBlock, OutfitHistoryApi, OutfitHistoryEntry } from "api/OutfitHistoryApi";
    import { PsOutfit } from "api/OutfitApi";

    type ExpandedOutfitHistoryEntry = OutfitHistoryEntry & {
        current: boolean;
        duration: number;
        outfit: PsOutfit | null;
    }

    export const CharacterOutfitHistory = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                outfitHistory: Loadable.idle() as Loading<OutfitHistoryBlock>
            }
        },

        created: function(): void {
            this.bindHistory();
        },

        methods: {
            bindHistory: async function(): Promise<void> {
                this.outfitHistory = Loadable.loading();
                this.outfitHistory = await OutfitHistoryApi.getByCharacterID(this.character.id);
            }
        },

        computed: {

            entries: function(): Loading<ExpandedOutfitHistoryEntry[]> {
                if (this.outfitHistory.state != "loaded") {
                    return Loadable.rewrap(this.outfitHistory);
                }

                return Loadable.loaded(this.outfitHistory.data.entries.map((iter: OutfitHistoryEntry, index: number) => {
                    if (this.outfitHistory.state != "loaded") { throw `1234sd`; }

                    const outfit: PsOutfit | null = this.outfitHistory.data.outfits.find(i => i.id == iter.outfitID) ?? null;

                    return {
                        ...iter,
                        current: index == this.outfitHistory.data.entries.length - 1, // last entry = most recent
                        duration: (iter.end.getTime() - iter.start.getTime()) / 1000,
                        outfit: outfit
                    };
                }));
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            Busy
        }
    });
    export default CharacterOutfitHistory;
</script>