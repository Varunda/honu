<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/online">Online</a>
            </li>
        </honu-menu>

        <div class="mb-3 border-bottom pb-3">
            <h2>
                Server
            </h2>

            <select v-model="worldID" class="form-control">
                <option :value="null">All</option>
                <option :value="13">Cobalt</option>
                <option :value="1">Connery</option>
                <option :value="17">Emerald</option>
                <option :value="19">Jaeger</option>
                <option :value="10">Miller</option>
                <option :value="40">SolTech</option>
            </select>
        </div>

        <div v-if="online.state == 'idle'">

        </div>
        
        <div v-else-if="online.state == 'loading'">
            <busy class="honu-busy"></busy>
            loading...
        </div>

        <div v-else-if="online.state == 'loaded'" class="d-flex" style="gap: 1rem;">
            <div class="flex-grow-1">
                <h2 class="text-center rounded" style="background-color: var(--color-bg-vs)">
                    <faction :faction-id="1" style="height: 3rem;"></faction>
                    VS
                </h2>

                <online-faction :data="onlineVs"></online-faction>
            </div>
            <div class="flex-grow-1">
                <h2 class="text-center rounded" style="background-color: var(--color-bg-nc)">
                    <faction :faction-id="2" style="height: 3rem;"></faction>
                    NC
                </h2>
                <online-faction :data="onlineNc"></online-faction>
            </div>
            <div class="flex-grow-1">
                <h2 class="text-center rounded" style="background-color: var(--color-bg-tr)">
                    <faction :faction-id="3" style="height: 3rem;"></faction>
                    TR
                </h2>
                <online-faction :data="onlineTr"></online-faction>
            </div>
        </div>

        <div v-else-if="online.state == 'error'">
            <api-error :error="online.problem"></api-error>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loading, Loadable } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import Busy from "components/Busy.vue";
    import ApiError from "components/ApiError";
    import Faction from "components/FactionImage";

    import "filters/FactionNameFilter";

    import FactionUtils from "util/Faction";

    import { CharacterApi, FlatOnlinePlayer, OnlinePlayer } from "api/CharacterApi";

    import OnlineFaction from "./OnlineFaction.vue";

    export const OutfitFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                online: Loadable.idle() as Loading<FlatOnlinePlayer[]>,

                worldID: null as number | null
            }
        },

        mounted: function() {
            document.title = "Honu / Online";

            const url = new URL(location.href);
            const urlWorldID: string | null = url.searchParams.get("worldID");
            if (urlWorldID != null) {
                this.worldID = Number.parseInt(urlWorldID);
                console.log(`OnlineViewer> parsed ${urlWorldID} to ${this.worldID}`);
            }

            this.loadOnline();
        },

        methods: {
            loadOnline: async function(): Promise<void> {
                this.online = Loadable.loading();
                this.online = await CharacterApi.getOnline();
            }
        },

        computed: {

            worldMatch: function(): FlatOnlinePlayer[] {
                if (this.online.state != "loaded") {
                    return [];
                }

                if (this.worldID == null) {
                    return this.online.data;
                }

                return this.online.data.filter(iter => iter.worldID == this.worldID);
            },

            onlineVs: function(): FlatOnlinePlayer[] {
                return this.worldMatch.filter(iter => iter.teamID == FactionUtils.VS);
            },
            onlineNc: function(): FlatOnlinePlayer[] {
                return this.worldMatch.filter(iter => iter.teamID == FactionUtils.NC);
            },
            onlineTr: function(): FlatOnlinePlayer[] {
                return this.worldMatch.filter(iter => iter.teamID == FactionUtils.TR);
            }
        },

        watch: {
            worldID: function(): void {
                const url = new URL(location.href);
                if (this.worldID != null) {
                    url.searchParams.set("worldID", `${this.worldID}`);
                }

                history.pushState({ path: url.href }, "", `/online?${url.searchParams.toString()}`);
            }
        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ATable, ACol, AHeader, ABody, AFilter, Busy, ApiError,
            OnlineFaction, Faction
        }

    });
    export default OutfitFinder;
</script>
