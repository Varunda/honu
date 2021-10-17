<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/</span>

                <a href="/character" title="Return to character search">Character Viewer</a>

                <span>/</span>

                <span v-if="character.state == 'loading'">
                    &lt;Loading...&gt;
                </span>

                <span v-else-if="character.state == 'loaded'">
                    {{character.data.name}}
                </span>
            </h1>
        </div>

        <hr />

        <div v-if="character.state == 'idle'">

        </div>

        <div v-else-if="character.state == 'loading'">
            Loading...
        </div>

        <div v-else-if="character.state == 'nocontent'">
            <span class="text-danger">
                No character with ID {{charID}} exists
            </span>
        </div>

        <div v-else-if="character.state == 'loaded'">
            <hr />

            <character-header :character="character.data"></character-header>

            <ul class="nav nav-tabs">
                <li class="nav-item" @click="selectTab('overview')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab == 'overview' }">
                        Overview
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('weapons')">
                    <a class="nav-link" :class="{ 'text-success': selectedTab == 'weapons' }">
                        Weapon stats
                    </a>
                </li>
                <li class="nav-item" disable="disabled">
                    <a class="nav-link" :class="{ 'text-success': selectedTab == 'sessions' }">
                        Sessions
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
    import { Loadable, Loading } from "Loading";

    import CharacterHeader from "./components/CharacterHeader.vue";
    import CharacterOverview from "./components/CharacterOverview.vue";
    import CharacterWeaponStats from "./components/CharacterWeaponStats.vue";

    export const CharacterViewer = Vue.extend({
        beforeMount: function(): void {
            this.loadCharacterID();
        },

        data: function() {
            return {
                charID: "" as string,
                character: Loadable.idle() as Loading<PsCharacter>,

                selectedTab: "Overview" as string,
                selectedComponent: "CharacterOverview" as string
            }
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
                } else {
                    throw `Unhandled tab selected '${lower}'`;
                }
            },

            loadCharacterID: function(): void {
                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid URL passed '${location.pathname}': Expected 3 parts after split on '/'`;
                }

                if (parts[1] != "c") {
                    throw `Expected 'c' in parts[1]`;
                }

                this.charID = parts[2];

                this.character = Loadable.loading();

                CharacterApi.getByID(this.charID).then((data: PsCharacter | null) => {
                    if (data == null) {
                        this.character = Loadable.nocontent();
                    } else {
                        this.character = Loadable.loaded(data);
                        console.log(`Loaded character: ${this.character}`);
                    }
                }).catch((err: any) => {
                    this.character = Loadable.error(err);
                });

                if (parts.length >= 4) {
                    console.log(`viewing tab: '${parts[3]}'`);
                    this.selectTab(parts[3]);
                }
            }

        },

        components: {
            CharacterHeader,
            CharacterOverview,
            CharacterWeaponStats
        }
    });
    export default CharacterViewer;

</script>