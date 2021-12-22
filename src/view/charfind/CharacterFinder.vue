<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                 / Character Viewer
            </h1>
        </div>

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
                <tr v-for="(c, index) of characters.data" :key="c.id" :class="[ scrollIndex == index ? 'table-info' : '' ]">
                    <td>
                        <span v-if="c.outfitID != null" :title="'\'' + c.outfitName + '\''">
                            [{{c.outfitTag}}]
                        </span>
                        {{c.name}}
                    </td>

                    <td>
                        {{c.prestige}}~{{c.battleRank}}
                    </td>

                    <td>
                        <span v-if="c.outfitID == null">
                            &lt;no outfit&gt;
                        </span>

                        <span v-else>
                            <a :href="'/o/' + c.outfitID">
                                [{{c.outfitTag}}] {{c.outfitName}}
                            </a>
                        </span>
                    </td>

                    <td>
                        {{c.factionID | faction}}

                    <td>
                        {{c.worldID | world}}
                    </td>

                    <td>
                        <span v-if="c.dateLastLogin.getTime() == defaultTime || c.dateLastLogin.getTime() == 0">
                            {{c.lastUpdated | moment}}
                            ({{c.lastUpdated | timeAgo}})
                        </span>
                        <span v-else>
                            {{c.dateLastLogin | moment}}
                            ({{c.dateLastLogin | timeAgo}})
                        </span>
                    </td>

                    <td>
                        <a :href="'/c/' + c.id" class="btn btn-success">View in this tab</a>
                        <a :href="'/c/' + c.id" target="_blank" class="btn btn-primary">View in new tab</a>
                    </td>

                    <td>
                        <button type="button" @click="copyToClipboard(c.id)" class="btn btn-primary">
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

    import "MomentFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/TimeAgoFilter";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";

    export const CharacterFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                characters: Loadable.idle() as Loading<PsCharacter[]>,

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

                CharacterApi.searchByName(this.lastSearch).then((data: Loading<PsCharacter[]>) => {
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
            }
        },

        components: {
            InfoHover
        }
    });
    export default CharacterFinder;

    (window as any).CharacterApi = CharacterApi;
</script>