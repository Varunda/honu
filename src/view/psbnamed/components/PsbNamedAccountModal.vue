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

                    <div class="input-grid-col2" style="grid-template-columns: min-content 1fr;">
                        <div class="input-cell mr-2">
                            <b>ID</b>
                        </div>

                        <div class="input-cell">
                            {{account.id}}
                        </div>

                        <div class="input-cell mr-2">
                            <b>Last used</b>
                        </div>

                        <div class="input-cell">
                            {{account.lastUsed | moment}}
                        </div>

                        <div class="input-cell mr-2">
                            <b>Time used</b>
                        </div>

                        <div class="input-cell">
                            {{account.secondsUsage | mduration}}
                        </div>
                    </div>
                </div>

                <div>
                    <h3 class="wt-header">Characters</h3>
                    <div class="d-flex">
                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>VS</h4>
                            <psb-named-character-column :character="account.vsCharacter"></psb-named-character-column>
                        </div>

                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>NC</h4>
                            <psb-named-character-column :character="account.ncCharacter"></psb-named-character-column>
                        </div>

                        <div class="flex-grow-1 mr-2 pr-2 border-right">
                            <h4>TR</h4>
                            <psb-named-character-column :character="account.trCharacter"></psb-named-character-column>
                        </div>

                        <div class="flex-grow-1">
                            <h4>NS / Do not use</h4>
                            <psb-named-character-column :character="account.nsCharacter"></psb-named-character-column>
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

    import PsbNamedCharacterColumn from "./PsbNamedCharacterColumn.vue";

    import { FlatPsbNamedAccount, PsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";

    import Busy from "components/Busy.vue";

    export const PsbNamedAccountModal = Vue.extend({
        props: {
            account: { type: Object as PropType<FlatPsbNamedAccount>, required: true }
        },

        data: function() {
            return {
                loading: {
                    delete: false as boolean,
                    recheck: false as boolean
                }
            }
        },

        methods: {
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
                    console.error(`Error when deleting account ${this.account.id}: ${status.message}`);
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
                    console.error(`Error when rechecking account ${this.account.id}: ${status.message}`);
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
            PsbNamedCharacterColumn,
            Busy
        }
    });
    export default PsbNamedAccountModal;
</script>