<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="/items">Item</a>

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
            </h1>
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

    import { PsItem, ItemApi } from "api/ItemApi";
    import { ExpandedWeaponStatEntry, CharacterWeaponStatApi } from "api/CharacterWeaponStatApi";

    import ItemPercentileViewer from "./components/ItemPercentileViewer.vue";
    import ItemTopViewer from "./components/ItemTopViewer.vue";

    export const ItemStatViewer = Vue.extend({
        props: {

        },

        data: function() {
            return {
                itemID: "" as string,

                item: Loadable.idle() as Loading<PsItem | null>,

                topKd: Loadable.idle() as Loading<ExpandedWeaponStatEntry[] | null>,
                topKpm: Loadable.idle() as Loading<ExpandedWeaponStatEntry[] | null>,
                topAcc: Loadable.idle() as Loading<ExpandedWeaponStatEntry[] | null>,
                topHsr: Loadable.idle() as Loading<ExpandedWeaponStatEntry[] | null>,
                topKills: Loadable.idle() as Loading<ExpandedWeaponStatEntry[] | null>,
            }
        },

        created: function(): void {
            document.title = `Honu / Item / <loading...>`;
        },

        beforeMount: function(): void {
            this.getItemIdFromUrl();
            this.bindItem();
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
                this.item = await Loadable.promise(ItemApi.getByID(this.itemID));

                if (this.item.state == "loaded" || (this.item as any).data != null) {
                    document.title = `Honu / Item / ${(this.item as any).data.name}`;
                } else {
                    document.title = `Honu / Item / <${this.itemID}>`;
                }

                this.topKd = Loadable.loading();
                this.topKd = await Loadable.promise(CharacterWeaponStatApi.getTopKD(this.itemID));
                this.topKpm = await Loadable.promise(CharacterWeaponStatApi.getTopKPM(this.itemID));
                this.topAcc = await Loadable.promise(CharacterWeaponStatApi.getTopAccuracy(this.itemID));
                this.topHsr = await Loadable.promise(CharacterWeaponStatApi.getTopHeadshotRatio(this.itemID));
                this.topKills = await Loadable.promise(CharacterWeaponStatApi.getTopKills(this.itemID));
            }

        },

        components: {
            ItemPercentileViewer,
            ItemTopViewer
        }
    });
    export default ItemStatViewer;
</script>