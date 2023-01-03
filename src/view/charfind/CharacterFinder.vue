<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/character">Characters</a>
            </li>
        </honu-menu>

        <hr />

        <div>
            <h4>Character name</h4>

            <div class="input-group">
                <input v-model="charName" class="form-control" @keyup.enter="openEnter" @keyup="scrollOptions" placeholder="Enter a name here!" />

                <div class="input-group-append">
                    <button type="button" @click="loadPartial" class="btn btn-primary" :disabled="validCharName == false">
                        <info-hover text="Search for characters by name, showing the most recent 20 by date logged in"></info-hover>
                        Search
                    </button>
                    <button type="button" @click="loadExact" class="btn btn-primary" :disabled="validCharName == false">
                        <info-hover text="Load exactly this character by name, showing the most recent 20 by date logged in"></info-hover>
                        Load exact
                    </button>
                    <button type="button" @click="loadAll" class="btn btn-secondary" :disabled="validCharName == false">
                        <info-hover text="Search for characters by name, showing all characters (not just 20)"></info-hover>
                        Search all
                    </button>
                </div>
            </div>
        </div>

        <hr />

        <a-table
            :entries="tableEntries"
            :show-filters="true"
            :paginate="false"
            default-sort-field="dateLastLogin" default-sort-order="desc"
            display-type="table">

            <a-col sort-field="name">
                <a-header>
                    <b>Character</b>
                </a-header>

                <a-filter method="template" v-slot="entry">
                    <toggle-button v-model="filter.showDeleted">
                        Show deleted
                    </toggle-button>
                </a-filter>

                <a-body v-slot="entry">
                    <span :class="[entry.notFoundCount && entry.notFoundCount > 0 ? 'text-danger' : '' ]">
                        {{entry | characterName}}

                        <info-hover v-if="entry.notFoundCount > 0" text="This character may have been deleted">
                        </info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="battleRankOrder">
                <a-header>
                    <b>Battle rank</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.prestige}}~{{entry.battleRank}}
                </a-body>
            </a-col>

            <a-col sort-field="outfitName">
                <a-header>
                    <b>Outfit</b>
                </a-header>

                <a-filter method="input" type="string" field="outfitSearch"
                    :conditions="[ 'contains' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span v-if="entry.outfitID == null">
                        &lt;no outfit&gt;
                    </span>

                    <span v-else>
                        <a :href="'/o/' + entry.outfitID">
                            [{{entry.outfitTag}}] {{entry.outfitName}}
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Faction</b>
                </a-header>

                <a-filter method="dropdown" type="number" field="factionID" :source="filterSources.faction"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.factionID | faction}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Server</b>
                </a-header>

                <a-filter method="dropdown" type="number" field="worldID" :source="filterSources.world"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.worldID | world}}
                </a-body>
            </a-col>

            <a-col sort-field="dateLastLogin">
                <a-header>
                    <b>Last login</b>
                </a-header>

                <a-body v-slot="entry">
                    <!--
                        some characters have 2000-01-01T00:00 as their login time, which is wrong,
                        but we can't fix it cause the character doesn't exist anymore
                    -->
                    <span v-if="entry.dateLastLogin.getTime() == defaultTime || entry.dateLastLogin.getTime() == 0">
                        {{entry.lastUpdated | moment}}
                        ({{entry.lastUpdated | timeAgo}})
                    </span>
                    <span v-else>
                        {{entry.dateLastLogin | moment}}
                        ({{entry.dateLastLogin | timeAgo}})
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Permalink</b>
                    <info-hover text="This link will always function"></info-hover>
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/c/' + entry.id" class="btn btn-success">View in this tab</a>
                    <a :href="'/c/' + entry.id" target="_blank" class="btn btn-primary">View in new tab</a>
                </a-body>
            </a-col>
        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import ToggleButton from "components/ToggleButton";

    import "MomentFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/TimeAgoFilter";
    import "filters/CharacterName";

    import WorldUtils from "util/World";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { CharacterMetadata, CharacterMetadataApi } from "api/CharacterMetadataApi";

    type CharacterEntry = {
        character: PsCharacter,
        metadata: CharacterMetadata | null
    };

    type FlatPsCharacter = PsCharacter & Partial<CharacterMetadata> & {
        battleRankOrder: number;
        outfitSearch: string;
    };

    export const CharacterFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                characters: Loadable.idle() as Loading<PsCharacter[]>,
                metadatas: Loadable.idle() as Loading<CharacterMetadata[]>,

                charName: "" as string,
                lastSearch: "" as string,
                scrollIndex: 0 as number,

                filter: {
                    showDeleted: true as boolean,
                },

                searchTimeoutID: -1 as number,
                pendingSearch: null as Promise<Loading<PsCharacter[]>> | null,

                defaultTime: 978307200000 as number
            }
        },

        created: function(): void {
            document.title = `Honu / Character search`;
        },

        methods: {
            scrollOptions: function(ev: KeyboardEvent): void {
                if (this.characters.state != "loaded") {
                    return;
                }

                if (ev.key == "ArrowUp") {
                    if (this.scrollIndex == 0) {
                        return;
                    }
                    --this.scrollIndex;
                } else if (ev.key == "ArrowDown") {
                    if (this.scrollIndex - 1 == this.characters.data.length) {
                        return;
                    }
                    ++this.scrollIndex;
                } else {
                    return;
                }

                ev.preventDefault();
                this.lastSearch = this.characters.data[this.scrollIndex].name;
            },

            openEnter: function(ev: KeyboardEvent): void {
                if (this.characters.state != "loaded") {
                    return;
                }

                const char: PsCharacter = this.characters.data[this.scrollIndex];

                if (ev.ctrlKey == true) {
                    console.log(`opening ${char.id} in new tab`);
                    window.open(`/c/${char.id}`, "_blank");
                } else {
                    console.log(`opening ${char.id} in this tab`);
                    location.href = `/c/${char.id}`;
                }

                ev.preventDefault();
            },

            loadPartial: function(): void {
                this.lastSearch = this.charName.toLowerCase();

                this.characters = Loadable.loading();
                CharacterApi.searchByName(this.lastSearch).then((data: Loading<PsCharacter[]>) => {
                    if (data.state == "loaded") {
                        this.setCharacters(data.data, 20);
                    } else {
                        this.characters = data;
                    }
                }).catch((err: any) => {
                    console.error(`Failed in loadPartial: ${err}`);
                });
            },

            loadAll: function(): void {
                this.lastSearch = this.charName.toLowerCase();
                this.characters = Loadable.loading();

                if (this.lastSearch.length < 4) {
                    return;
                }

                CharacterApi.searchByName(this.lastSearch, false).then((data: Loading<PsCharacter[]>) => {
                    if (data.state == "loaded") {
                        this.setCharacters(data.data);
                    } else {
                        this.characters = data;
                    }
                }).catch((err: any) => {
                    console.error(`failed in loadAll: ${err}`);
                });
            },

            loadExact: function(): void {
                this.lastSearch = this.charName.toLowerCase();
                this.characters = Loadable.loading();

                CharacterApi.getByName(this.lastSearch).then((data: Loading<PsCharacter[]>) => {
                    if (data.state == "loaded") {
                        this.setCharacters(data.data, 100);
                    } else {
                        this.characters = data;
                    }
                }).catch((err: any) => {
                    console.error(`failed in loadExact: ${err}`);
                });
            },

            setCharacters: function(data: PsCharacter[], count?: number): void {
                this.scrollIndex = 0;

                const arr: PsCharacter[] = data.sort((a, b) => {
                    if (a.name != b.name && a.name.toLowerCase() == this.lastSearch) {
                        return -1;
                    }
                    if (a.name != b.name && b.name.toLowerCase() == this.lastSearch) {
                        return 1;
                    }

                    const valA: number = (a.dateLastLogin.getTime() == this.defaultTime ? a.lastUpdated : a.dateLastLogin).getTime();
                    const valB: number = (b.dateLastLogin.getTime() == this.defaultTime ? b.lastUpdated : b.dateLastLogin).getTime();
                    return valB - valA;
                })

                if (count) {
                    this.characters = Loadable.loaded(arr.slice(0, count));
                } else {
                    this.characters = Loadable.loaded(arr);
                }

                if (this.characters.state != "loaded") {
                    throw `how aren't you loaded?`;
                }

                const charIDs: string[] = this.characters.data.map(iter => iter.id);
                this.metadatas = Loadable.loading();

                CharacterMetadataApi.getByIDs(charIDs).then((data: Loading<CharacterMetadata[]>) => {
                    this.metadatas = data;
                });
            },

            copyToClipboard: function(charID: string): void {
                const url: string = `${location.protocol}//${location.hostname}:${location.port}/c/${charID}`;
                try {
                    navigator.clipboard.writeText(url);
                } catch (err: any) {
                    console.error(err);
                }
            }
        },

        watch: {
            charName: function(): void {
                if (this.pendingSearch != null) {
                    this.pendingSearch = null;
                }

                if (this.charName.length < 3) {
                    this.characters = Loadable.nocontent();
                    return;
                }

                clearTimeout(this.searchTimeoutID);

                const savedName: string = this.charName.toLowerCase();

                this.searchTimeoutID = setTimeout(() => {
                    this.characters = Loadable.loading();
                    this.lastSearch = savedName;

                    CharacterApi.searchByName(savedName).then((data: Loading<PsCharacter[]>) => {
                        if (data.state == "loaded") {
                            this.setCharacters(data.data, 20);
                        }
                    }).catch((err: any) => {
                        console.error(`error in charName watch: ${err}`);
                    });
                }, 600) as unknown as number;
            }
        },

        computed: {
            validCharName: function(): boolean {
                return this.charName.trim().length > 1
                    && this.charName.length < 33; // character limit is 32
            },

            tableEntries: function(): Loading<FlatPsCharacter[]> {
                if (this.characters.state == "nocontent") {
                    return Loadable.idle();
                }

                if (this.characters.state != "loaded") {
                    return Loadable.rewrap(this.characters);
                }

                if (this.filter.showDeleted == false && this.metadatas.state == "loading") {
                    return Loadable.loading();
                }

                const chars: FlatPsCharacter[] = [];

                for (const iter of this.characters.data) {
                    let metadata: CharacterMetadata | null = null;
                    if (this.metadatas.state == "loaded") {
                        metadata = this.metadatas.data.find(i => i.id == iter.id) ?? null;
                    }

                    if (this.filter.showDeleted == false && (metadata?.notFoundCount ?? 0) > 0) {
                        continue;
                    }

                    chars.push({
                        ...iter,
                        ...metadata,
                        battleRankOrder: (iter.prestige * 1000) + iter.battleRank,
                        outfitSearch: (iter.outfitID != null) ? `[${iter.outfitTag}] ${iter.outfitName}` : ""
                    });
                }

                return Loadable.loaded(chars);
            },

            filterSources: function() {
                return {
                    faction: [
                        { value: null, key: "All" },
                        { value: 1, key: "VS" } ,
                        { value: 2, key: "NC" } ,
                        { value: 3, key: "TR" } ,
                        { value: 4, key: "NS" } ,
                    ],

                    world: [
                        { value: null, key: "All" },
                        { value: WorldUtils.Connery, key: "Connery" },
                        { value: WorldUtils.Cobalt, key: "Cobalt" },
                        { value: WorldUtils.Miller, key: "Miller" },
                        { value: WorldUtils.Emerald, key: "Emerald" },
                        { value: WorldUtils.Jaeger, key: "Jaeger" },
                        { value: WorldUtils.SolTech, key: "SolTech" }
                    ]
                }
            }
        },

        components: {
            InfoHover, ToggleButton,
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
        }
    });
    export default CharacterFinder;

    (window as any).CharacterApi = CharacterApi;
</script>