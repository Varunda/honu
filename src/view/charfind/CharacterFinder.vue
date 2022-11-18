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
                <input v-model="charName" class="form-control" @keyup.enter="openEnter" @keyup="scrollOptions" />

                <div class="input-group-append">
                    <button type="button" @click="loadPartial" class="btn btn-primary" :disabled="validCharName == false">
                        Search
                    </button>
                    <button type="button" @click="loadExact" class="btn btn-primary" :disabled="validCharName == false">
                        Load exact
                    </button>
                    <button type="button" @click="loadAll" class="btn btn-secondary" title="Show data from all characters, not just 20">
                        Search all
                    </button>
                </div>
            </div>
        </div>

        <hr />

        <table class="table">
            <thead class="table-secondary">
                <tr class="font-weight-bold">
                    <td>Character</td>
                    <td>Battle rank</td>
                    <td>Outfit</td>
                    <td>Faction</td>
                    <td>Server</td>
                    <td>Last login</td>
                    <td>
                        Permalink
                        <info-hover text="This link will always function"></info-hover>
                    </td>
                    <td>Copy link</td>
                </tr>
            </thead>

            <tbody v-if="characters.state == 'idle' || characters.state == 'nocontent'"></tbody>

            <tbody v-else-if="characters.state == 'loading'">
                <tr>
                    <td colspan="7">
                        Loading...
                    </td>
                </tr>
            </tbody>

            <tbody v-else-if="characters.state == 'loaded' && characters.data.length <= 0">
                <tr class="table-warning">
                    <td colspan="7">
                        No characters by the name of '{{lastSearch}}' found
                    </td>
                </tr>
            </tbody>

            <tbody v-else-if="characters.state == 'loaded' && characters.data.length > 0">
                <tr v-for="(c, index) of entries" :key="c.character.id" :class="[ scrollIndex == index ? 'table-info' : '' ]">
                    <td :class="{ 'text-danger': c.metadata != null && c.metadata.notFoundCount > 0 }">
                        <span v-if="c.character.outfitID != null" :title="'\'' + c.character.outfitName + '\''">
                            [{{c.character.outfitTag}}]
                        </span>
                        {{c.character.name}}

                        <info-hover v-if="c.metadata != null && c.metadata.notFoundCount > 0" text="This character may have been deleted">
                        </info-hover>
                    </td>

                    <td>
                        {{c.character.prestige}}~{{c.character.battleRank}}
                    </td>

                    <td>
                        <span v-if="c.outfitID == null">
                            &lt;no outfit&gt;
                        </span>

                        <span v-else>
                            <a :href="'/o/' + c.character.outfitID">
                                [{{c.character.outfitTag}}] {{c.character.outfitName}}
                            </a>
                        </span>
                    </td>

                    <td>
                        {{c.character.factionID | faction}}

                    <td>
                        {{c.character.worldID | world}}
                    </td>

                    <td>
                        <span v-if="c.character.dateLastLogin.getTime() == defaultTime || c.character.dateLastLogin.getTime() == 0">
                            {{c.character.lastUpdated | moment}}
                            ({{c.character.lastUpdated | timeAgo}})
                        </span>
                        <span v-else>
                            {{c.character.dateLastLogin | moment}}
                            ({{c.character.dateLastLogin | timeAgo}})
                        </span>
                    </td>

                    <td>
                        <a :href="'/c/' + c.character.id" class="btn btn-success">View in this tab</a>
                        <a :href="'/c/' + c.character.id" target="_blank" class="btn btn-primary">View in new tab</a>
                    </td>

                    <td>
                        <button type="button" @click="copyToClipboard(c.character.id)" class="btn btn-primary">
                            Copy
                        </button>
                    </td>
                </tr>
            </tbody>

            <tbody v-else-if="characters.state == 'error'">
                <tr class="table-danger">
                    <td colspan="7">
                        {{characters.message}}
                    </td>
                </tr>
            </tbody>

            <tbody v-else>
                <tr class="table-danger">
                    <td colspan="7">
                        Unchecked state of characters: '{{characters.state}}'
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import InfoHover from "components/InfoHover.vue";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import "MomentFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/TimeAgoFilter";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { CharacterMetadata, CharacterMetadataApi } from "api/CharacterMetadataApi";

    type CharacterEntry = {
        character: PsCharacter,
        metadata: CharacterMetadata | null
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
                    && this.charName.length < 33;
            },

            entries: function(): CharacterEntry[] {
                if (this.characters.state != "loaded") {
                    return [];
                }

                return this.characters.data.map(iter => {
                    let metadata: CharacterMetadata | null = null;
                    if (this.metadatas.state == "loaded") {
                        metadata = this.metadatas.data.find(i => i.id == iter.id) ?? null;
                    }

                    return {
                        character: iter,
                        metadata: metadata
                    };
                });
            }
        },

        components: {
            InfoHover,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }
    });
    export default CharacterFinder;

    (window as any).CharacterApi = CharacterApi;
</script>