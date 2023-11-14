<template>
    <div class="modal-dialog modal-xl">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Account for {{account.playerName}}</h5>

                <button type="button" class="close" data-dismiss-modal>
                    &times;
                </button>
            </div>

            <div class="modal-body">
                <div class="mb-2">
                    <h3 class="wt-header">Actions</h3>

                    <table class="table table-sm">
                        <tr>
                            <td>
                                <button @click="recheckByID" type="button" class="btn btn-success" :disabled="deleted == true || loading.recheck == true">
                                    Recheck the characters
                                </button>
                                <busy style="height: 1.5rem;" v-if="loading.recheck == true"></busy>
                            </td>

                            <td class="align-middle">
                                <span v-if="deleted == true">
                                    This account has been deleted and cannot be rechecked
                                </span>
                            </td>
                        </tr>

                        <tr>
                            <td>
                                <button @click="deleteByID" type="button" class="btn btn-danger" :disabled="deleted == true || loading.delete == true">
                                    Mark as deleted
                                </button>
                                <busy style="height: 1.5rem;" v-if="loading.delete == true"></busy>
                            </td>

                            <td class="align-middle">
                                <span v-if="deleted == false">
                                    This account can be deleted
                                </span>
                                <span v-else>
                                    This account has already been deleted
                                </span>
                            </td>
                        </tr>

                    </table>
                </div>

                <div class="mb-2">
                    <h3 class="wt-header">Account</h3>

                    <div class="input-grid-col2" style="grid-template-columns: min-content 1fr">
                        <div class="input-cell">
                            <span class="input-group-text input-group-prepend">
                                <b>ID</b>
                            </span>
                        </div>

                        <div class="input-cell">
                            <input :value="account.id" readonly class="form-control" type="text" />
                        </div>

                        <div class="input-cell">
                            <span class="input-group-text input-group-prepend">
                                <b>Last used</b>
                            </span>
                        </div>

                        <div class="input-cell">
                            <input :value="account.lastUsed | moment" readonly class="form-control" type="text" />
                        </div>

                        <div class="input-cell">
                            <span class="input-group-text input-group-prepend">
                                <b>Time used</b>
                            </span>
                        </div>

                        <div class="input-cell">
                            <input :value="account.secondsUsage | mduration" readonly class="form-control" type="text" />
                        </div>

                        <div class="input-cell pt-1 mt-3 border-top">Edit</div>
                        <div class="input-cell pt-1 mt-3 border-top">&nbsp;</div>

                        <div class="input-cell">
                            <span class="input-group-text input-group-prepend">
                                Tag
                            </span>
                        </div>

                        <div class="input-cell">
                            <input v-model="edit.tag" :readonly="edit.editing == false" class="form-control" type="text" />
                        </div>

                        <div class="input-cell">
                            <span class="input-group-text input-group-prepend">
                                Character name
                            </span>
                        </div>

                        <div class="input-cell">
                            <input v-model="edit.name" :readonly="edit.editing == false" class="form-control" type="text" />
                        </div>

                        <div class="input-cell"></div>

                        <div class="input-cell">
                            <div v-if="edit.editing == false">
                                <button @click="startEdit" type="button" class="btn btn-primary">Edit</button>
                            </div>

                            <div v-else>
                                <button @click="submitEdit" type="button" class="btn btn-success" :disabled="edit.response.state == 'loading'">
                                    Save
                                </button>

                                <button @click="cancelEdit" type="button" class="btn btn-secondary">
                                    Cancel
                                </button>

                                <busy v-if="edit.response.state == 'loading'" style="height: 1.5rem;">
                                </busy>
                            </div>
                        </div>
                    </div>
                </div>

                <div>
                    <h3 class="wt-header">Characters</h3>
                    <div class="d-flex">
                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>VS</h4>
                            <psb-character-column :character="account.vsCharacter" :status="account.account.vsStatus"></psb-character-column>
                        </div>

                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>NC</h4>
                            <psb-character-column :character="account.ncCharacter" :status="account.account.ncStatus"></psb-character-column>
                        </div>

                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>TR</h4>
                            <psb-character-column :character="account.trCharacter" :status="account.account.trStatus"></psb-character-column>
                        </div>

                        <div class="flex-grow-1">
                            <h4>NS / Do not use</h4>
                            <psb-character-column :character="account.nsCharacter" :status="account.account.nsStatus"></psb-character-column>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loadable, Loading } from "Loading";
    import EventBus from "EventBus";
    import Toaster from "Toaster";

    import PsbCharacterColumn from "./PsbCharacterColumn.vue";

    import { FlatPsbNamedAccount, PsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";

    import Busy from "components/Busy.vue";

    export const PsbAccountModal = Vue.extend({
        props: {
            account: { type: Object as PropType<FlatPsbNamedAccount>, required: true }
        },

        data: function() {
            return {
                edit: {
                    tag: "" as string,
                    name: "" as string,

                    editing: false as boolean,

                    response: Loadable.idle() as Loading<PsbNamedAccount>
                },

                loading: {
                    delete: false as boolean,
                    recheck: false as boolean
                }
            }
        },

        created: function(): void {
            this.cancelEdit();
        },

        methods: {

            cancelEdit: function(): void {
                this.edit.tag = this.account.tag ?? "";
                this.edit.name = this.account.name;

                this.edit.editing = false;
            },

            startEdit: function(): void {
                this.edit.editing = true;
            },

            submitEdit: async function(): Promise<void> {
                this.edit.response = Loadable.loading();
                this.edit.response = await PsbNamedAccountApi.rename(this.account.id, this.edit.tag, this.edit.name);

                if (this.edit.response.state == "loaded") {
                    EventBus.$emit("rebind-accounts");
                    Toaster.add("Successfully renamed account", `Account successfully renamed to ${this.edit.tag}x${this.edit.name}`, "success");
                    this.cancelEdit();
                } else if (this.edit.response.state == "error") {
                    Toaster.add("Failed to rename account", `Failed to rename account: ${this.edit.response.problem.detail}`, "danger");
                }
            },

            deleteByID: async function(): Promise<void> {
                const conf: boolean = confirm(`Are you sure you want to delete the account for ${this.account.playerName}?`);
                if (conf == false) {
                    return;
                }

                this.loading.delete = true;
                const status: Loading<void> = await PsbNamedAccountApi.deleteByID(this.account.id);
                if (status.state == "loaded") {
                    EventBus.$emit("rebind-accounts");
                    console.log(`Successfully marked account as deleted`);
                } else if (status.state == "error") {
                    console.error(`Error when deleting account ${this.account.id}: ${status.problem.detail}`);
                }
                this.loading.delete = false;
            },

            recheckByID: async function(): Promise<void> {
                this.loading.recheck = true;
                const status: Loading<PsbNamedAccount> = await PsbNamedAccountApi.recheckByID(this.account.id);
                if (status.state == "loaded") {
                    console.log(`Successfully recheck account ${this.account.id}`);
                    EventBus.$emit("rebind-accounts");
                } else if (status.state == "error") {
                    console.error(`Error when rechecking account ${this.account.id}: ${status.problem.detail}`);
                }
                this.loading.recheck = false;
            }
        },

        computed: {
            deleted: function(): boolean {
                return this.account.account.deletedAt != null || this.account.account.deletedBy != null;
            }
        },

        components: {
            PsbCharacterColumn,
            Busy
        }
    });
    export default PsbAccountModal;
</script>