<template>
    <div>
        <div class="input-group mb-3">
            <input class="form-control" v-model="characterInput" placeholder="Character name" @keyup.enter="findCharactersWrapped" />

            <button class="input-group-append btn btn-primary" @click="findCharactersWrapped">
                Search
            </button>
        </div>

        <div>
            <div v-if="characters.state == 'idle'"></div>

            <div v-else-if="characters.state == 'loading'">
                Loading...
            </div>

            <div v-else-if="characters.state == 'loaded' && characters.data.length > 0">
                <h2>Multiple names match!</h2>
                <h6>Multiple characters have this name. Please select which one you want</h6>

                <a-table
                    :entries="characters"
                    :show-filters="true"
                    :paginate="false"
                    default-sort-field="dateLastLogin" default-sort-order="desc"
                    display-type="table">

                    <a-col sort-field="name">
                        <a-header>
                            <b>Character</b>
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry | characterName}}
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
                            {{entry.dateLastLogin | moment}}
                            ({{entry.dateLastLogin | timeAgo}})
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            <b>View</b>
                        </a-header>

                        <a-body v-slot="entry">
                            <a :href="'/c/' + entry.id" class="btn btn-primary">View</a>
                        </a-body>
                    </a-col>

                    <a-col>
                        <a-header>
                            <b>Select</b>
                        </a-header>

                        <a-body v-slot="entry">
                            <button class="btn btn-success" @click="addCharacter(entry.id)">
                                Select
                            </button>
                        </a-body>
                    </a-col>
                </a-table>
            </div>
        </div>

        <div>
            <label class="mb-1">
                Year to make the wrapped for
            </label>

            <select class="form-control" v-model="year" placeholder="Select a year">
                <option v-for="y in years" :value="y">{{y}}</option>
            </select>

            <div class="my-1">
                This Wrapped will contain events between {{rangeStart | momentNoTz("YYYY-MM-DD")}} and {{rangeEnd | momentNoTz("YYYY-MM-DD")}}
            </div>
        </div>

        <hr class="border" />

        <div class="p-3 border rounded" v-if="responseId.state == 'error'">
            <div class="alert alert-danger mx-n3 mt-n3">
                <h2>Failed to create wrapped!</h2>
            </div>

            <api-error :error="responseId.problem"></api-error>
        </div>

        <div class="p-3 border rounded" v-if="enabled.state == 'loaded' && enabled.data == false">
            <div class="alert alert-danger mx-n3 mt-n3">
                <h2>
                    Wrapped generation has been disabled!
                </h2>
            </div>

            This is likely because the database has not yet been setup to provide data for 2023.
            <br />
            Please check back later.
        </div>

        <h2>Input characters</h2>
        <h6>These characters will be included in the wrap up</h6>

        <table class="table table-sm mb-2">
            <thead>
                <tr class="table-secondary th-border-top-0">
                    <th>
                        Character
                    </th>

                    <th>
                        Faction
                    </th>

                    <th>
                        Server
                    </th>

                    <th>
                        Remove
                    </th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="entry in inputCharacters">
                    <td>
                        <a :href="'/c/' + entry.id">
                            {{entry | characterName}}
                        </a>
                    </td>

                    <td>
                        {{entry.factionID | faction}}
                    </td>

                    <td>
                        {{entry.worldID | world}}
                    </td>

                    <td>
                        <span @click="removeCharacter(entry.id)">
                            &times;
                        </span>
                    </td>
                </tr>
            </tbody>
        </table>

        <div v-if="inputCharacters.length > 0">
            <hr class="border" />

            <button class="btn btn-success w-100 mb-2" @click="createWrapped" :disabled="!canSubmit">
                Create
            </button>

            <div v-if="inputCharacters.length >= 16" class="alert alert-warning">
                A Wrapped can have at most 16 characters
            </div>

            <div v-if="enabled.state == 'loaded' && enabled.data == false" class="alert alert-danger">
                Wrapped creation has been disabled on a server-wide level
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import Toaster from "Toaster";
    import { Loadable, Loading } from "Loading";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { WrappedApi, WrappedEntry } from "api/WrappedApi";
    import { ApiError } from "components/ApiError";

    import "MomentFilter";
    import "filters/WorldNameFilter";
    import "filters/ZoneNameFilter";
    import "filters/CharacterName";
    import "filters/TimeAgoFilter";
    import "filters/FactionNameFilter";
    import WorldUtils from "util/World";

    export const WrappedViewCreator = Vue.extend({
        props: {

        },

        data: function() {
            return {
                enabled: Loadable.idle() as Loading<boolean>,
                year: 2024 as number,
                years: [] as number[],

                characterInput: "" as string,
                characters: Loadable.idle() as Loading<PsCharacter[]>,

                responseId: Loadable.idle() as Loading<string>,

                inputCharacters: [] as PsCharacter[]
            }
        },

        mounted: function(): void {
            this.loadEnabled();

            // -1, as wrapped is only valid for the previous year
            // so in 2024, only the full year of 2023 is completed
            this.year = (new Date()).getFullYear() - 1;

            for (let i = 2022; i <= this.year; ++i) {
                this.years.push(i);
            }

        },

        methods: {
            loadEnabled: async function(): Promise<void> {
                this.enabled = Loadable.loading();
                this.enabled = await WrappedApi.isEnabled();
            },

            findCharactersWrapped: async function(): Promise<void> {
                if (this.characterInput.length < 3) {
                    console.log(`not searching, character input is ${this.characterInput.length}`);
                    return;
                }

                await this.findCharacters(this.characterInput);
            },

            findCharacters: async function(name: string): Promise<void> {
                this.characters = Loadable.loading();
                this.characters = await CharacterApi.getByName(name);

                if (this.characters.state == "loaded") {
                    // if there's only 1 character, the user wanted that one
                    if (this.characters.data.length == 1) {
                        this.addCharacter(this.characters.data[0].id);
                        this.characters = Loadable.loaded([]);
                    }
                }
            },

            addCharacter: function(id: string): void {
                if (this.inputCharacters.find(iter => iter.id == id) != undefined) {
                    Toaster.add(`Duplicate`, "duplicate character skipped", "warning");
                    console.warn(`not adding duplicate character ${id}`);
                    return;
                }

                if (this.characters.state != "loaded") {
                    console.log(`characters.state is not 'loaded', is actually ${this.characters.state}`);
                    return;
                }

                const c: PsCharacter | undefined = this.characters.data.find(iter => iter.id == id);
                if (c == undefined) {
                    console.log(`failed to find character id ${id} from input characters: [${this.characters.data.map(iter => iter.id).join(", ")}`);
                    return;
                }

                this.inputCharacters.push(c);

                this.characters = Loadable.loaded([]);
            },

            removeCharacter: function(id: string): void {
                console.log(`removing ${id} from the wrapped`);
                this.inputCharacters = this.inputCharacters.filter(iter => iter.id != id);
            },

            createWrapped: async function(): Promise<void> {
                this.responseId = Loadable.loading();
                this.responseId = await WrappedApi.insert(this.inputCharacters.map(iter => iter.id), this.year);

                if (this.responseId.state == "loaded") {
                    window.history.pushState({ path: `/wrapped/${this.responseId.data}` }, '', `/wrapped/${this.responseId.data}`);
                    this.$emit("update-wrapped-id", this.responseId.data);
                } else if (this.responseId.state == "error") {
                    console.error(`failed to create wrapped: ${this.responseId.problem.detail}`);
                } else {
                    console.log(`failed to create Wrapped! ${this.responseId.state}`);
                }
            }
        },

        computed: {
            canSubmit: function(): boolean {
                if (this.inputCharacters.length == 0) {
                    return false;
                }

                if (this.inputCharacters.length >= 16) {
                    return false;
                }

                if (this.enabled.state == "loaded" && this.enabled.data == false) {
                    return false;
                }

                return true;
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
            },

            rangeStart: function(): Date {
                return new Date(this.year, 0, 1);
            },

            rangeEnd: function(): Date {
                return new Date(this.year, 11, 31);
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            ApiError
        }

    });

    export default WrappedViewCreator;
</script>