<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/items">Items</a>
            </li>

            <li class="nav-item h1 p-0 mx-2">
                <span>/</span>

                <span v-if="item.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <span v-else-if="item.state == 'loaded'">
                    {{item.data.name}}
                </span>

                <span v-else-if="item.state == 'nocontent'">
                    &lt;missing {{itemID}}&gt;
                </span>
            </li>
        </honu-menu>

        <div v-if="queueIndex > -1" class="alert alert-info text-center">
            <h4>
                This weapon is currently pending a data refresh
                <info-hover text="A weapon is put into a refresh queue whenever a character gets a kill with it"></info-hover>
            </h4>
            <div>
                Position: {{queueIndex + 1}} / {{queueLength}}
            </div>

            <div v-if="queueIndex == 0">
                This weapon data is currently being refreshed! Come back in a couple of minutes
            </div>

            <div>
                Estimated time until refresh:
                <span v-if="queueAvg > -1">
                    {{(queueAvg * (queueIndex + 1)) / 1000 | mduration}}
                    (average time per weapon: {{queueAvg / 1000 | mduration}})
                </span>
                <span v-else>
                    &lt;missing queue average&gt;
                </span>
            </div>
        </div>

        <div class="w-100 mw-100">
            <h2 class="wt-header">Percentile stats</h2>
            <item-percentile-viewer :item-id="itemID"></item-percentile-viewer>
        </div>

        <div>
            <h2 class="wt-header">Top</h2>
            <item-top-viewer :item-id="itemID"></item-top-viewer>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";

    import { PsItem, ItemApi } from "api/ItemApi";
    import { WeaponStatTopApi } from "api/WeaponStatTopApi";
    import { HonuHealth, HonuHealthApi } from "api/HonuHealthApi";

    import ItemPercentileViewer from "./components/ItemPercentileViewer.vue";
    import ItemTopViewer from "./components/ItemTopViewer.vue";

    import "MomentFilter";

    export const ItemStatViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                itemID: "" as string,

                queue: Loadable.idle() as Loading<number[]>,
                health: Loadable.idle() as Loading<HonuHealth>,

                item: Loadable.idle() as Loading<PsItem>
            }
        },

        created: function(): void {
            document.title = `Honu / Item / <loading...>`;
        },

        beforeMount: function(): void {
            this.getItemIdFromUrl();
            this.bindItem();
            this.bindQueue();
            this.bindHealth();
        },

        methods: {
            getItemIdFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                const itemID: number = Number.parseInt(parts[2]);
                if (Number.isNaN(itemID) == false) {
                    this.itemID = parts[2];
                    console.log(`Item ID is ${this.itemID}`);
                } else {
                    throw `Failed to parse parts[2] '${parts[2]}' into a number, got ${itemID}`;
                }
            },

            bindItem: async function(): Promise<void> {
                this.item = Loadable.loading();
                this.item = await ItemApi.getByID(this.itemID);

                if (this.item.state == "loaded") {
                    document.title = `Honu / Item / ${(this.item as any).data.name}`;
                } else {
                    document.title = `Honu / Item / <${this.itemID}>`;
                }
            },

            bindQueue: async function(): Promise<void> {
                this.queue = Loadable.loading();
                this.queue = await WeaponStatTopApi.getQueueItems();
            },

            bindHealth: async function(): Promise<void> {
                this.health = Loadable.loading();
                this.health = await HonuHealthApi.getHealth();
            }

        },

        computed: {
            queueIndex: function(): number {
                if (this.queue.state != "loaded") {
                    return -1;
                }

                const itemID: number = Number.parseInt(this.itemID);
                if (Number.isNaN(itemID)) {
                    console.warn(`Failed to parse ${this.itemID} to a valid integer`);
                    return -1;
                }

                const index: number = this.queue.data.findIndex(iter => iter == itemID);

                return index;
            },

            queueLength: function(): number {
                if (this.queue.state != "loaded") {
                    return -1;
                }

                return this.queue.data.length;
            },

            queueAvg: function(): number {
                if (this.health.state != "loaded") {
                    return -1;
                }

                const queue = this.health.data.queues.find(iter => iter.queueName == "weapon_update_queue");
                if (queue == undefined) {
                    return -1;
                }

                return queue.average ?? 0;
            }
        },

        components: {
            ItemPercentileViewer,
            ItemTopViewer,
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });
    export default ItemStatViewer;
</script>