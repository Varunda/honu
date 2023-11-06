<template>
    <div class="modal-dialog modal-xl">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Create a new account</h5>

                <button type="button" class="close" data-dismiss-modal>
                    &times;
                </button>
            </div>

            <div class="modal-body">
                <div class="mb-2">
                    <h3 class="wt-header">Input</h3>

                    <b>
                        Case sensitive
                    </b>

                    <div class="input-grid-col2" style="grid-template-columns: min-content 1fr;">
                        <div class="input-cell input-group-text input-group-prepend">
                            Tag
                            <info-hover class="ml-2" text="Tag the player requested. Leave blank for no tag"></info-hover>
                        </div>

                        <div class="input-cell">
                            <input v-model="tag" type="text" class="form-control" />
                        </div>

                        <div class="input-cell input-group-text input-group-prepend">
                            Character name
                            <info-hover class="ml-2" text="Name of the player's characters"></info-hover>
                        </div>

                        <div class="input-cell">
                            <input v-model="characterName" type="text" class="form-control" @keyup.enter="checkNames" placeholder="Press enter here to check" />
                        </div>
                    </div>
                </div>

                <div>
                    <button class="btn btn-success" @click="checkNames">
                        Check characters
                    </button>
                </div>

                <hr class="border" />

                <div v-if="characterSet.state == 'idle'" class="my-2">
                    No check started
                </div>

                <div v-else-if="characterSet.state == 'loaded'" class="my-2 d-flex">
                    <div class="flex-grow-1 border-right mr-2">
                        <character-column faction="VS" :character="characterSet.data.vs"></character-column>
                    </div>

                    <div class="flex-grow-1 border-right mr-2">
                        <character-column faction="NC" :character="characterSet.data.nc"></character-column>
                    </div>

                    <div class="flex-grow-1 border-right mr-2">
                        <character-column faction="TR" :character="characterSet.data.tr"></character-column>
                    </div>

                    <div class="flex-grow-1">
                        <character-column faction="NS" :character="characterSet.data.ns"></character-column>
                    </div>
                </div>

                <div>
                    <hr class="border" />

                    <div v-if="exists != null" class="text-danger text-center">
                        Account {{exists.id}} already has the name {{exists.tag}}x{{exists.name}}
                    </div>

                    <button @click="create" type="button" class="btn btn-primary w-100" :disabled="canCreate == false">
                        Create
                    </button>

                    <div v-if="showSuccess == true" class="text-success">
                        Successfully created new named account
                    </div>
                </div>

            </div>
        </div>
    </div>

</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loadable, Loading } from "Loading";
    import Toaster from "Toaster";

    import InfoHover from "components/InfoHover.vue";
    import { FlatPsbNamedAccount, PsbAccountType, PsbCharacterSet, PsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";
    import { PsCharacter } from "api/CharacterApi";

    import "filters/WorldNameFilter";

    const CharacterColumn = Vue.extend({
        props: {
            faction: { type: String, required: true },
            character: { type: Object as PropType<PsCharacter | null>, required: false }
        },

        template: `
            <div>
                <h2 class="text-center">{{faction}}</h2>

                <div v-if="character == null" class="text-danger">
                    No character exists
                </div>

                <table v-else class="table">
                    <tr>
                        <th>Name</th>
                        <td>
                            <a :href="'/c/' + character.id">
                                {{character.name}}
                            </a>
                        </td>
                    </tr>

                    <tr>
                        <th>Faction</th>
                        <td>{{character.factionID | faction}}</td>
                    </tr>

                    <tr>
                        <th>Server</th>
                        <td>{{character.worldID | world}}</td>
                    </tr>
                </table>
            </div>
        `
    });

    export const PsbNamedAccountCreateModal = Vue.extend({
        props: {
            entries: { type: Array as PropType<FlatPsbNamedAccount[]>, required: true },
            TypeId: { type: Number, required: true }
        },

        data: function() {
            return {
                tag: "" as string,
                characterName: "" as string,
                playerName: "" as string,

                exists: null as FlatPsbNamedAccount | null,

                showSuccess: false as boolean,

                characterSet: Loadable.idle() as Loading<PsbCharacterSet>,
                createResponse: Loadable.idle() as Loading<PsbNamedAccount>,
            }
        },

        methods: {
            checkNames: async function(): Promise<void> {
                this.characterSet = Loadable.loading();
                this.characterSet = await PsbNamedAccountApi.getCharacterSet(this.tag, this.characterName);

                this.exists = this.entries.find(iter => iter.tag == this.tag && iter.name == this.characterName) || null;
            },

            create: async function(): Promise<void> {
                this.createResponse = Loadable.loading();
                this.createResponse = await PsbNamedAccountApi.create(this.tag, this.characterName, PsbAccountType.NAMED);

                if (this.createResponse.state == "loaded") {
                    Toaster.add("Named account created", `Successfully created a named account for ${this.tag}x${this.characterName}`, "success");
                    this.showSuccess = true;
                    this.tag = "";
                    this.characterName = ""
                    this.exists = null;
                    this.characterSet = Loadable.idle();

                    setTimeout(() => {
                        this.showSuccess = false;
                    }, 8000);
                } else {
                    Toaster.add("Failed!", `Failed to create named account:\n${(this.createResponse as any).message}`, "danger");
                }
            }
        },

        computed: {
            canCreate: function(): boolean {
                return (this.characterSet.state == "loaded")
                    && this.exists == null
                    && this.characterSet.data.vs != null
                    && this.characterSet.data.nc != null
                    && this.characterSet.data.tr != null
                    && this.characterSet.data.ns != null;
            }

        },

        components: {
            CharacterColumn,
            InfoHover
        }

    });
    export default PsbNamedAccountCreateModal;
</script>