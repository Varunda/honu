<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/outfitwars">Outfits Wars</a>
            </li>
        </honu-menu>

        <hr class="border" />

        <ul class="nav nav-tabs border-bottom mb-2">
            <li class="nav-item" @click="selectTab('matches')">
                <a class="nav-link border" :class="{ 'text-success': selectedTab != 'matches', 'bg-info': selectedTab == 'matches' }">
                    Matches
                </a>
            </li>
            <li class="nav-item" @click="selectTab('outfits')">
                <a class="nav-link border" :class="{ 'text-success': selectedTab != 'outfits', 'bg-info': selectedTab == 'outfits' }">
                    Outfits
                </a>
            </li>
        </ul>

        <keep-alive>
            <component :is="selectedComponent"></component>
        </keep-alive>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { FlatOutfitWarsOutfit, OutfitWarsOutfitApi } from "api/OutfitWarsApi";

    import OutfitWarsOutfits from "./components/OutfitWarsOutfits.vue";
    import OutfitWarsMatches from "./components/OutfitWarsMatches.vue";

    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";

    import WorldUtils from "util/World";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import Collapsible from "components/Collapsible.vue";
    import Busy from "components/Busy.vue";
    import ApiError from "components/ApiError";

    export const OutfitWars = Vue.extend({
        props: {

        },

        data: function() {
            return {
                selectedTab: "matches" as string,
                selectedComponent: "OutfitWarsMatches" as string,
            }
        },

        mounted: function(): void {
            document.title = "Honu / Outfit Wars";

            const parts: string[] = location.pathname.split("/");
            console.log(parts);
            if (parts.length < 2) {
                throw `Invalid URL passed '${location.pathname}': Expected at least 2 parts after split on '/'`;
            }

            // a trailing / can cause 3 parts
            if (parts.length == 2) {
                this.selectTab("matches");
            } else {
                this.selectTab(parts[2] == "" ? "matches" : parts[2]);
            }
        },

        methods: {

            selectTab: function(tab: string): void {
                console.log(`OutfitWars> showing tab ${tab}`);
                this.selectedTab = tab.toLowerCase();

                const lower: string = this.selectedTab.toLowerCase();

                if (lower == "outfits") {
                    this.selectedComponent = "OutfitWarsOutfits";
                } else if (lower == "matches") {
                    this.selectedComponent = "OutfitWarsMatches";
                } else {
                    throw `Unhandled tab selected '${lower}'`;
                }

                const url = new URL(location.href);
                history.pushState({ path: url.href }, "", `/outfitwars/${lower}`);
            },

        },

        computed: {

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Collapsible,
            Busy,
            ApiError,
            OutfitWarsOutfits, OutfitWarsMatches
        }

    });

    export default OutfitWars;

</script>