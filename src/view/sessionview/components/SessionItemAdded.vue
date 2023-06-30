<template>
    <div>
        <a @click="showTable = !showTable" href="javascript:void(0);">(debug) show all item added events</a>

        <table v-show="showTable" class="table">
            <thead class="table-secondary">
                <tr>
                    <th colspan="2">Item</th>
                    <th>Context</th>
                    <th>Continent</th>
                    <th>Timestamp</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="ev in events" :key="ev.event.id">
                    <td style="width: 128px;">
                        <div v-if="ev.item != null">
                            <census-image :image-id="ev.item.imageID" style="height: 64px;"></census-image>
                        </div>
                        <div v-else style="height: 64px;"></div>
                    </td>
                    <td class="align-middle">
                        <div v-if="ev.item != null">
                            {{ev.item.name}}
                            <info-hover :text="ev.item.description"></info-hover>
                        </div>
                        <div v-else style="height: 64px;">
                            &lt;unknown item {{ev.event.itemID}}&gt;
                        </div>
                    </td>

                    <td class="align-middle">
                        {{ev.event.context}}
                    </td>

                    <td class="align-middle">
                        {{ev.event.zoneID | zone}}
                    </td>

                    <td class="align-middle">
                        {{ev.event.timestamp | moment("YYYY-MM-DD hh:mm:ssA")}}
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Loading, Loadable } from "Loading";
    import { AchievementEarned, AchievementEarnedBlock } from "api/AchievementEarnedApi";
    import { Session } from "api/SessionApi";
    import { ItemAddedEventBlock, ItemAddedEventApi, ItemAddedEvent } from "api/ItemAddedEventApi";
    import { PsItem } from "api/ItemApi";

    import InfoHover from "components/InfoHover.vue";

    import CensusImage from "components/CensusImage";

    import "MomentFilter";
    import "filters/ZoneNameFilter";

    type ExpandedItemAddedEvent = {
        event: ItemAddedEvent;
        item: PsItem | null;
    }

    export const SessionItemAdded = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
        },

        data: function() {
            return {
                eventsBlock: Loadable.idle() as Loading<ItemAddedEventBlock>,

                showTable: false as boolean
            }
        },

        created: function(): void {
            this.bindEvents();
        },

        methods: {
            bindEvents: async function(): Promise<void> {
                this.eventsBlock = Loadable.loading();
                this.eventsBlock = await ItemAddedEventApi.getBySessionID(this.session.id);
            }
        },

        computed: {
            events: function(): ExpandedItemAddedEvent[] {
                if (this.eventsBlock.state != "loaded") {
                    return [];
                }

                return this.eventsBlock.data.events.map((iter: ItemAddedEvent): ExpandedItemAddedEvent => {
                    if (this.eventsBlock.state != "loaded") {
                        throw ``;
                    }
                    return {
                        event: iter,
                        item: this.eventsBlock.data.items.find(i => i.id == iter.itemID) ?? null
                    };
                }).sort((a, b) => a.event.timestamp.getTime() - b.event.timestamp.getTime());
            }
        },

        components: {
            CensusImage, InfoHover
        }
    });
    export default SessionItemAdded;

</script>
