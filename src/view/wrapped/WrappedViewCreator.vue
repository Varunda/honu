<template>
    <div>
        <div class="input-group">
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

        <hr class="border" />

        <h2>Input characters</h2>
        <h6>These characters will be included in the wrap up</h6>
        <div class="list-group mb-2">
            <div class="list-group-item" v-for="entry in inputCharacters">
                {{entry.name}}
            </div>
        </div>

        <hr class="border" />

        <div v-if="inputCharacters.length > 0">
            <button class="btn btn-success w-100" @click="createWrapped">
                Create
            </button>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import { Loadable, Loading } from "Loading";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";
    import { WrappedApi, WrappedEntry } from "api/WrappedApi";

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
                characterInput: "" as string,
                characters: Loadable.idle() as Loading<PsCharacter[]>,

                inputCharacters: [] as PsCharacter[]
            }
        },

        methods: {
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

            createWrapped: async function(): Promise<void> {
                const id: Loading<string> = await WrappedApi.insert(this.inputCharacters.map(iter => iter.id));

                if (id.state == "loaded") {
                    window.history.pushState({ path: `/wrapped/${id.data}` }, '', `/wrapped/${id.data}`);
                    this.$emit("update-wrapped-id", id.data);
                } else {
                    console.log(`unchecked state of id after insert: ${id.state}`);
                }
            }
        },

        computed: {
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
            ATable, ACol, ABody, AFilter, AHeader,
        }

    });

    export default WrappedViewCreator;
</script>