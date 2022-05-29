<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/character">Characters</a>
            </li>

            <li class="nav-item h1 p-0 mx-2">
                /
                <span v-if="character.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <span v-else-if="character.state == 'loaded'">
                    {{character.data.name}}
                </span>
            </li>
        </honu-menu>

        <hr class="border" />

        <div v-if="metadata.state == 'loaded' && metadata.data.notFoundCount > 2">
            <h4 class="text-warning text-center">
                This character has likely been deleted
            </h4>
            <small class="text-muted text-center d-block" :title="'Have found in DB ' + metadata.data.notFoundCount + ' times, but not in Census'">
                This character exists in Honu's database, but not in the Planetside 2 API. Missed {{metadata.data.notFoundCount > 50 ? `50+` : `${metadata.data.notFoundCount}`}} times
            </small>
        </div>

        <div v-if="character.state == 'idle'"></div>

        <div v-else-if="character.state == 'loading'">
            Loading...
            <busy class="honu-busy-lg"></busy>
        </div>

        <div v-else-if="character.state == 'nocontent'">
            <span class="text-danger">
                No character with ID {{charID}} exists
            </span>
        </div>

        <div v-else-if="character.state == 'loaded'">
            <character-header :character="character.data"></character-header>

            <ul class="nav nav-tabs mb-2">
                <li class="nav-item" @click="selectTab('overview')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'overview' }">
                        Overview
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('weapons')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'weapons' }">
                        Weapon stats
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('sessions')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'sessions' }">
                        Sessions
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('items')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'items' }">
                        Unlocks
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('friends')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'friends' }">
                        Friends
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('directives')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'directives' }">
                        Directives
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('extra')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab != 'extra' }">
                        Fun stats
                    </a>
                </li>
            </ul>

            <keep-alive>
                <component :is="selectedComponent" :character="character.data"></component>
            </keep-alive>
        </div>

        <div v-else-if="character.state == 'error'">
            Failed to load character: {{character.message}}
        </div>

        <div v-else>
            Unchecked state of character: {{character.state}}
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { CharacterMetadata, CharacterMetadataApi } from "api/CharacterMetadataApi";
    import { Loadable, Loading } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import Busy from "components/Busy.vue";
    import CharacterHeader from "./components/CharacterHeader.vue";
    import CharacterOverview from "./components/CharacterOverview.vue";
    import CharacterWeaponStats from "./components/CharacterWeaponStats.vue";
    import CharacterSessions from "./components/CharacterSessions.vue";
    import CharacterItems from "./components/CharacterItems.vue";
    import CharacterFriends from "./components/CharacterFriends.vue";
    import CharacterDirectives from "./components/CharacterDirectives.vue";
    import CharacterExtraStats from "./components/CharacterExtraStats.vue";

    export const CharacterViewer = Vue.extend({
        beforeMount: function(): void {
            this.loadCharacterID();
        },

        data: function() {
            return {
                charID: "" as string,
                character: Loadable.idle() as Loading<PsCharacter>,
                metadata: Loadable.idle() as Loading<CharacterMetadata>,

                selectedTab: "overview" as string,
                selectedComponent: "CharacterWeaponStats" as string
            }
        },

        created: function(): void {
            document.title = `Honu / Char / <loading...>`;
        },

        methods: {
            selectTab: function(tab: string): void {
                this.selectedTab = tab.toLowerCase();

                const lower: string = this.selectedTab.toLowerCase();

                if (lower == "overview") {
                    this.selectedComponent = "CharacterOverview";
                } else if (lower == "weapons") {
                    this.selectedComponent = "CharacterWeaponStats";
                } else if (lower == "sessions") {
                    this.selectedComponent = "CharacterSessions";
                } else if (lower == "items") {
                    this.selectedComponent = "CharacterItems";
                } else if (lower == "friends") {
                    this.selectedComponent = "CharacterFriends";
                } else if (lower == "directives") {
                    this.selectedComponent = "CharacterDirectives";
                } else if (lower == "extra") {
                    this.selectedComponent = "CharacterExtraStats";
                } else {
                    throw `Unhandled tab selected '${lower}'`;
                }

                const url = new URL(location.href);
                if (this.character.state == "loaded") {
                    url.searchParams.set("name", this.character.data.name);
                }

                history.pushState({ path: url.href }, "", `/c/${this.charID}/${lower}?${url.searchParams.toString()}`);
            },

            loadCharacterID: async function(): Promise<void> {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid URL passed '${location.pathname}': Expected at least 3 parts after split on '/'`;
                }

                if (parts[1] != "c") {
                    throw `Expected 'c' in parts[1]`;
                }

                this.charID = parts[2];

                // Run in background so it doesn't hang up character loading
                this.metadata = Loadable.loading();
                CharacterMetadataApi.getByID(this.charID).then((data: Loading<CharacterMetadata>) => {
                    this.metadata = data;
                });

                this.character = Loadable.loading();
                this.character = await CharacterApi.getByID(this.charID);

                if (this.character.state == "nocontent") {
                    document.title = `Honu / Char / <not found>`;
                } else if (this.character.state == "loaded") {
                    document.title = `Honu / Char / ${this.character.data.name}`;

                    const url = new URL(location.href);
                    url.searchParams.set("name", this.character.data.name);
                    history.replaceState({ path: url.href }, "", url.href);
                }

                if (parts.length >= 4) {
                    console.log(`viewing tab: '${parts[3]}'`);
                    this.selectTab(parts[3]);
                }
            }

        },

        components: {
            Busy,
            CharacterHeader,
            CharacterOverview,
            CharacterWeaponStats,
            CharacterSessions,
            CharacterItems,
            CharacterFriends,
            CharacterDirectives,
            CharacterExtraStats,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });
    export default CharacterViewer;

</script>