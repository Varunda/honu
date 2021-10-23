﻿<template>
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
                <input v-model="charName" class="form-control" @keyup.enter="getByNameWrapper" />

                <div class="input-group-append">
                    <button type="button" @click="getByNameWrapper" class="btn btn-primary" :disabled="validCharName == false">
                        Load
                    </button>
                </div>
            </div>
        </div>

        <hr />

        <table class="table">
            <thead>
                <tr class="font-weight-bold">
                    <td>Character</td>
                    <td>Battle rank</td>
                    <td>Outfit</td>
                    <td>
                        Permalink
                        <info-hover text="This link will always function"></info-hover>
                    </td>
                    <td>Copy link</td>
                </tr>
            </thead>

            <tbody v-if="characters.state == 'idle'">

            </tbody>

            <tbody v-else-if="characters.state == 'loading'">
                <tr>
                    <td colspan="5">
                        Loading...
                    </td>
                </tr>
            </tbody>

            <tbody v-else-if="characters.state == 'loaded' && characters.data.length <= 0">
                <tr class="table-warning">
                    <td colspan="5">
                        No characters by the name of '{{lastSearch}}' found
                    </td>
                </tr>
            </tbody>

            <tbody v-else-if="characters.state == 'loaded' && characters.data.length > 0">
                <tr v-for="c of characters.data" :key="c.id">
                    <td>
                        <span v-if="c.outfitID != null" :title="'\'' + c.outfitName + '\''">
                            [{{c.outfitTag}}]
                        </span>
                        {{c.name}}
                    </td>

                    <td>
                        <span v-if="c.prestige == true">1~</span>{{c.battleRank}}
                    </td>

                    <td>
                        <span v-if="c.outfitID == null">
                            &lt;No outfit&gt;
                        </span>

                        <span v-else>
                            [{{c.outfitTag}}] {{c.outfitName}}
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
                    <td colspan="5">
                        {{characters.message}}
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import InfoHover from "components/InfoHover.vue";

    import { Loading, Loadable } from "Loading";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";

    export const CharacterFinder = Vue.extend({
        props: {

        },

        data: function() {
            return {
                characters: Loadable.idle() as Loading<PsCharacter[]>,
                charName: "" as string,
                lastSearch: "" as string
            }
        },

        methods: {
            getByNameWrapper: function() {
                this.lastSearch = this.charName;
                this.getByName(this.charName);
            },

            getByName: async function(name: string): Promise<void> {
                this.characters = Loadable.loading();
                try {
                    this.characters = Loadable.loaded(await CharacterApi.getByName(name));
                } catch (err: any) {
                    this.characters = Loadable.error(err);
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