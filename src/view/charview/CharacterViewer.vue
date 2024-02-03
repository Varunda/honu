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
                This character has likely been deleted (or you are viewing a PS4 character, which Honu does not support)
            </h4>
            <small class="text-muted text-center d-block" :title="'Have found in DB ' + metadata.data.notFoundCount + ' times, but not in Census'">
                This character exists in Honu's database, but not in the Planetside 2 API. Missed {{metadata.data.notFoundCount > 50 ? `50+` : `${metadata.data.notFoundCount}`}} times
            </small>
        </div>

        <div v-if="queue.data.state == 'loaded'">
            <div v-if="queue.index > -1">
                <h4 class="text-info text-center">
                    This character is
                    <span v-if="queue.index == 0">
                        currently being
                    </span>
                    <span v-else>
                        in queue to be
                    </span>
                    updated
                    <button title="Refresh" class="btn btn-link p-0" @click="loadQueuePosition" :class=" {'spin': queue.data.state != 'loaded'}">
                        <span>&#x21bb;</span>
                    </button>
                </h4>
                <div class="text-center d-block">
                    This character is in position {{queue.index + 1}} of {{queue.data.data.length}}.
                    <span v-if="queue.processingTime != null">
                        Estimated time: {{(queue.processingTime * (queue.index + 1)) / 1000 | mduration}}
                    </span>
                </div>
            </div>

            <div v-else-if="queue.wasInQueue == true">
                <h4 class="text-success text-center btn-link" @click="loadCharacterID">
                    This character was updated. Click here to reload character data
                </h4>
            </div>
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
            <character-header :character="character.data" :honu-data="honuData"></character-header>

            <ul class="nav nav-tabs border-bottom mb-2">
                <li class="nav-item" @click="selectTab('overview')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'overview', 'bg-info': selectedTab == 'overview' }">
                        Overview
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('weapons')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'weapons', 'bg-info': selectedTab == 'weapons' }">
                        Weapon stats
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('vehicle')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'vehicle', 'bg-info': selectedTab == 'vehicle' }">
                        Vehicle stats
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('sessions')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'sessions', 'bg-info': selectedTab == 'sessions' }">
                        Sessions
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('items')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'items', 'bg-info': selectedTab == 'items' }">
                        Unlocks
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('friends')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'friends', 'bg-info': selectedTab == 'friends' }">
                        Friends
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('directives')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'directives', 'bg-info': selectedTab == 'directives' }">
                        Directives
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('outfitHistory')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'outfithistory', 'bg-info': selectedTab == 'outfithistory' }">
                        Outfit history
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('killboard')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'killboard', 'bg-info': selectedTab == 'killboard' }">
                        Killboard
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('alerts')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'alerts', 'bg-info': selectedTab == 'alerts' }">
                        Alerts
                    </a>
                </li>
                <li class="nav-item" @click="selectTab('extra')">
                    <a class="nav-link border" :class="{ 'text-success': selectedTab != 'extra', 'bg-info': selectedTab == 'extra' }">
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

    import { PsCharacter, CharacterApi, HonuCharacterData } from "api/CharacterApi";
    import { CharacterMetadata, CharacterMetadataApi } from "api/CharacterMetadataApi";
    import { HonuHealth, HonuHealthApi, ServiceQueueCount } from "api/HonuHealthApi";
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
    import CharacterVehicleStats from "./components/CharacterVehicleStats.vue";
    import CharacterOutfitHistory from "./components/CharacterOutfitHistory.vue";
    import CharacterKillboard from "./components/CharacterKillboard.vue";
    import CharacterAlerts from "./components/CharacterAlerts.vue";

    export const CharacterViewer = Vue.extend({
        data: function() {
            return {
                charID: "" as string,
                character: Loadable.idle() as Loading<PsCharacter>,
                metadata: Loadable.idle() as Loading<CharacterMetadata>,
                honuData: Loadable.idle() as Loading<HonuCharacterData>,

                queue: {
                    data: Loadable.idle() as Loading<string[]>,
                    index: 0 as number,
                    processingTime: null as number | null,
                    wasInQueue: false as boolean,
                    intervalId: -1 as number
                },

                selectedTab: "overview" as string,
                selectedComponent: "CharacterOverview" as string
            }
        },

        created: function(): void {
            document.title = `Honu / Char / <loading...>`;
        },

        beforeMount: function(): void {
            this.loadCharacterID();

            this.$nextTick(async () => {
                console.log(`doing first queue check`);
                await this.loadQueuePosition();
                console.log(`loaded queue position, at ${this.queue.index}`);

                if (this.queue.wasInQueue == true) {
                    this.queue.intervalId = setInterval(async () => {
                        if (this.queue.wasInQueue == true) {
                            console.log(`character is in update queue, updating queue position`);
                            await this.loadQueuePosition();
                            console.log(`character is in position ${this.queue.index} (exiting on -1)`);

                            if (this.queue.index == -1) {
                                console.log(`no longer in queue, stopping background queue check`);
                                clearInterval(this.queue.intervalId);
                            }
                        }
                    }, 6000) as unknown as number;
                }
            });
        },

        methods: {
            selectTab: function(tab: string): void {
                this.selectedTab = tab.toLowerCase();
                if (this.selectedTab == "vehicles") {
                    this.selectedTab = "vehicle";
                }

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
                } else if (lower == "vehicle" || lower == "vehicles") {
                    this.selectedComponent = "CharacterVehicleStats";
                } else if (lower == "outfithistory") {
                    this.selectedComponent = "CharacterOutfitHistory";
                } else if (lower == "killboard") {
                    this.selectedComponent = "CharacterKillboard";
                } else if (lower == "alerts") {
                    this.selectedComponent = "CharacterAlerts";
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
                this.queue.wasInQueue = false;

                const parts: string[] = location.pathname.split("/");
                if (parts.length < 3) {
                    throw `Invalid URL passed '${location.pathname}': Expected at least 3 parts after split on '/'`;
                }

                if (parts[1] != "c") {
                    throw `Expected 'c' in parts[1], got '${parts[1]}' instead`;
                }

                this.charID = parts[2];

                // Run in background so it doesn't hang up character loading
                this.metadata = Loadable.loading();
                CharacterMetadataApi.getByID(this.charID).then((data: Loading<CharacterMetadata>) => {
                    this.metadata = data;
                });

                this.honuData = Loadable.loading();
                CharacterApi.getHonuData(this.charID).then((data: Loading<HonuCharacterData>) => {
                    this.honuData = data;
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
            },

            loadQueuePosition: async function(): Promise<void> {
                this.queue.data = await CharacterMetadataApi.getQueue();
                if (this.queue.data.state != "loaded") {
                    return;
                }

                this.queue.index = this.queue.data.data.findIndex((iter: string) => iter == this.charID);
                if (this.queue.index == -1) {
                    this.queue.index = -1;
                    this.queue.processingTime = null;
                    return;
                }

                this.queue.wasInQueue = true;

                const health: Loading<HonuHealth> = await HonuHealthApi.getHealth();
                if (health.state == "loaded") {
                    const queue: ServiceQueueCount | undefined = health.data.queues.find((iter) => iter.queueName == "character_prio_queue");
                    if (queue == undefined) {
                        this.queue.processingTime = null;
                        return;
                    }

                    this.queue.processingTime = queue.median;
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
            CharacterVehicleStats,
            CharacterOutfitHistory,
            CharacterKillboard,
            CharacterAlerts,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });
    export default CharacterViewer;

</script>