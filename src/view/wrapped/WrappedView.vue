<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <honu-menu class="flex-grow-1">
                <menu-dropdown></menu-dropdown>

                <menu-sep></menu-sep>

                <li class="nav-item h1 p-0">
                    <a href="/wrapped">Wrapped</a>
                </li>
            </honu-menu>
        </div>

        <div v-if="wrappedID == ''" >
            <wrapped-view-creator @update-wrapped-id="onUpdateWrappedID"></wrapped-view-creator>
        </div>

        <div v-else>
            <wrapped-view-entry :wrapped-id="wrappedID"></wrapped-view-entry>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import WrappedViewCreator from "./WrappedViewCreator.vue";
    import WrappedViewEntry from "./WrappedViewEntry.vue";

    export const WrappedView = Vue.extend({
        props: {

        },

        data: function() {
            return {
                wrappedID: "" as string
            }
        },

        created: function(): void {
            this.getWrappedIdFromUrl();
            document.title = "Honu / Wrapped";
        },

        methods: {
            getWrappedIdFromUrl: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length == 2) {
                    this.wrappedID = "";
                } else if (parts.length < 3) {
                    throw `Invalid pathname passed: '${location.pathname}. Expected 3 splits after '/', got ${parts}'`;
                }

                this.wrappedID = parts[2] ?? "";
                console.log(`loading wrappedID ${this.wrappedID}`);
            },

            onUpdateWrappedID: function(wrappedID: string): void {
                this.wrappedID = wrappedID;
                console.log(`wrappedID updated: ${this.wrappedID}`);
            }
        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            WrappedViewCreator, WrappedViewEntry
        }
    });

    export default WrappedView;
</script>