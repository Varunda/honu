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
                    &lt;loading...&gt;
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

        <div class="d-flex border rounded">
            <div style="width: 256px;" class="align-content-center m-2">
                <census-image v-if="item.state == 'loaded'" :image-id="item.data.imageID" style="max-width: 256px; max-height: 256px;"></census-image>
            </div>

            <div class="flex-grow-1 align-content-center">
                <div v-if="item.state == 'idle'"></div>
                
                <div v-else-if="item.state == 'loading'">
                    Loading...
                </div>

                <div v-else-if="item.state == 'nocontent'">
                    Item {{itemID}} does not exist! (404 returned from API)
                </div>

                <div v-else-if="item.state == 'loaded'" class="d-inline-block">
                    <table class="table table-sm">
                        <tbody>
                            <tr>
                                <td><b>Name</b></td>
                                <td>{{item.data.name}}</td>
                            </tr>
                            <tr>
                                <td><b>Description</b></td>
                                <td>{{item.data.description}}</td>
                            </tr>
                            <tr>
                                <td><b>Faction</b></td>
                                <td>
                                    <span v-if="item.data.factionID == 0 || item.data.factionID == -1">
                                        All ({{item.data.factionID}})
                                    </span>
                                    <span v-else>
                                        {{item.data.factionID | faction}}

                                        <span v-if="item.data.factionID == 4" class="text-muted">
                                            (this can mean all factions too)
                                        </span>
                                    </span>
                                </td>
                            </tr>
                        </tbody>
                    </table>

                </div>

                <api-error v-else-if="item.state == 'error'" :error="item.problem"></api-error>
            </div>
        </div>

        <toggle-button v-model="showSnapshot" class="m-2">
            Show snapshot data
        </toggle-button>

        <div v-show="showSnapshot" class="w-100 mw-100">
            <collapsible header-text="Snapshots">
                <item-snapshot :item-id="itemID"></item-snapshot>
            </collapsible>
        </div>

        <div class="w-100 mw-100">
            <collapsible header-text="Percentile stats">
                <item-percentile-viewer :item-id="itemID"></item-percentile-viewer>
            </collapsible>
        </div>

        <div>
            <collapsible header-text="Top">
                <item-top-viewer :item-id="itemID"></item-top-viewer>
            </collapsible>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import Collapsible from "components/Collapsible.vue";
    import CensusImage from "components/CensusImage";
    import ApiError from "components/ApiError";
    import ToggleButton from "components/ToggleButton";

    import { PsItem, ItemApi } from "api/ItemApi";
    import { WeaponStatTopApi } from "api/WeaponStatTopApi";
    import { HonuHealth, HonuHealthApi } from "api/HonuHealthApi";

    import ItemPercentileViewer from "./components/ItemPercentileViewer.vue";
    import ItemTopViewer from "./components/ItemTopViewer.vue";
    import ItemSnapshot from "./components/ItemSnapshot.vue";

    import "MomentFilter";
    import "filters/FactionNameFilter";

    export const ItemStatViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                itemID: "" as string,

                queue: Loadable.idle() as Loading<number[]>,
                health: Loadable.idle() as Loading<HonuHealth>,

                item: Loadable.idle() as Loading<PsItem>,

                showSnapshot: false as boolean,
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

                    const url = new URL(location.href);
                    url.searchParams.set("name", this.item.data.name);

                    history.pushState({ path: url.href }, "", `/i/${this.itemID}?${url.searchParams.toString()}`);

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
            ItemPercentileViewer, ItemTopViewer, ItemSnapshot,
            InfoHover, Collapsible, CensusImage, ApiError, ToggleButton,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });
    export default ItemStatViewer;
</script>