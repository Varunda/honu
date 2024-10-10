<template>
    <div>
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Account Management
            </li>
        </honu-menu>

        <div class="row">
            <div class="col-2">
                <h2 class="wt-header text-center">
                    Accounts

                    <toggle-button v-model="showDeactivated">
                        Show deactivated
                    </toggle-button>
                </h2>

                <div v-if="accounts.state == 'idle'"></div>
                <div v-else-if="accounts.state == 'loading'">
                    <busy class="honu-busy-sm"></busy>
                    Loading...
                </div>

                <div v-else-if="accounts.state == 'loaded'">
                    <div class="list-group">
                        <div v-for="account in displayedAccounts" @click="viewAccount(account.id)" class="list-group-item"
                                :class="{ 'list-group-item-primary': selected.account != null && selected.account.id == account.id }">

                            {{account.id}} / {{account.name}}
                            <span v-if="account.deletedOn != null">
                                (deactivated)
                            </span>
                        </div>
                    </div>
                </div>

                <button class="btn btn-success w-100 mt-1" @click="showCreateModal">
                    Create account
                </button>
            </div>

            <div class="col-10">
                <h2 class="wt-header">
                    <span v-if="selected.account == null" class="text-muted">
                        No account deleted
                    </span>
                    <span v-else>
                        <span class="flex-grow-1">
                            {{selected.account.name}}
                            <span v-if="selected.account.deletedOn != null" class="text-danger">
                                (deactivated)
                            </span>
                        </span>
                        <span class="flex-grow-0">
                            <button v-if="selected.account.deletedOn == null" class="btn btn-danger" @click="deactivateAccount(selected.account.id)">
                                Deactiviate
                            </button>
                        </span>
                    </span>
                </h2>

                <div v-if="selected.account != null">
                    <div>
                        Created on: {{selected.account.timestamp | moment}}
                    </div>

                    <collapsible header-text="Permissions">
                        <template v-slot:header>
                            <div class="flex-grow-1"></div>

                            <div class="flex-grow-0 mr-2">
                                <button @click.stop="showPermissionModal" type="button" class="btn btn-primary">
                                    Add
                                </button>
                            </div>
                        </template>

                        <a-table v-slot:default :entries="selected.permissions" display-type="table" :show-filters="true" 
                            row-padding="normal"
                            default-sort-order="asc" default-sort-field="permission">

                            <a-col>
                                <a-header>
                                    <b>ID</b>
                                </a-header>

                                <a-body v-slot="entry">
                                    {{entry.id}}
                                </a-body>
                            </a-col>

                            <a-col>
                                <a-header>
                                    <b>Permission</b>
                                </a-header>

                                <a-filter field="permission" type="string" method="input"
                                    :conditions="[ 'contains' ]">
                                </a-filter>

                                <a-body v-slot="entry">
                                    {{entry.permission}}
                                </a-body>
                            </a-col>

                            <a-col>
                                <a-header>
                                    <b>Granted on</b>
                                </a-header>

                                <a-body v-slot="entry">
                                    {{entry.timestamp | moment}}
                                </a-body>
                            </a-col>

                            <a-col>
                                <a-header>
                                    <b>Granted by</b>
                                </a-header>

                                <a-body v-slot="entry">
                                    {{entry.grantedByID}}
                                </a-body>
                            </a-col>

                            <a-col>
                                <a-header>
                                    <b>Revoke</b>
                                </a-header>

                                <a-body v-slot="entry">
                                    <button type="button" class="btn btn-sm btn-danger" @click="removePermission(entry.id)">
                                        <span class="ph-bold ph-x"></span>
                                    </button>
                                </a-body>
                            </a-col>
                        </a-table>

                    </collapsible>
                </div>
            </div>
        </div>

        <div class="modal" id="account-create-modal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Create account</h5>
                        <button type="button" class="close">
                            &times;
                        </button>
                    </div>

                    <div class="modal-body">
                        <div>Name:</div>
                        <input v-model="create.name" class="form-control" placeholder="Account display name" />

                        <div>Email:</div>
                        <input v-model="create.email" class="form-control" placeholder="Account email" />

                        <div>Discord name:</div>
                        <input v-model="create.discord" class="form-control" placeholder="Discord name" />

                        <div>Discord ID:</div>
                        <input v-model="create.discordID" class="form-control" placeholder="Discord ID" />

                        <button type="button" class="btn btn-primary" @click="createAccount">
                            Create account
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div class="modal" id="add-permission-modal">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Add permission</h5>
                        <button type="button" class="close">
                            &times;
                        </button>
                    </div>

                    <div v-if="selected.account != null" class="modal-body">
                        <h4>
                            Adding permssion to {{selected.account.name}}
                        </h4>

                        <div v-if="permissions.state == 'idle'"></div>
                        <div v-else-if="permissions.state == 'loading'">
                            <busy class="honu-busy-sm"></busy>
                            Loading...
                        </div>

                        <div v-else-if="permissions.state == 'loaded'">
                            <input v-model="permissionSearch" id="add-permission-search" @keyup.enter="addPermissionEnterWrapper"
                                type="text" class="form-control" placeholder="Filter permissions" />

                            <div class="list-group list-group-flush" style="height: 320px; max-height: 320px; overflow-y: auto;">
                                <div v-for="perm in filteredPermissions" class="list-group-item px-0 d-flex">
                                    <span class="flex-grow-1">
                                        {{perm.id}}
                                    </span>
                                    <span class="flex-grow-0">
                                        <button class="btn btn-sm btn-primary" @click="permissionSearch = perm.id">
                                            +
                                        </button>
                                    </span>
                                </div>
                            </div>

                            <button class="btn btn-success w-100" @click="addPermission(permissionSearch)" :disabled="!validPermission">
                                Add permission
                            </button>
                        </div>
                    </div>

                    <div v-else class="modal-body">
                        no account selected
                    </div>
                </div>
            </div>

        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import Collapsible from "components/Collapsible.vue";
    import ToggleButton from "components/ToggleButton";

    import "MomentFilter";
    import Toaster from "Toaster";

    import { HonuAccount, HonuAccountApi } from "api/HonuAccountApi";
    import { HonuAccountPermission, HonuAccountPermissionApi } from "api/HonuAccountPermissionApi";
    import { HonuPermission, HonuPermissionApi } from "api/HonuPermissionApi";

    export const AccountManagement = Vue.extend({
        props: {

        },

        data: function() {
            return {
                accounts: Loadable.idle() as Loading<HonuAccount[]>,

                showDeactivated: false as boolean,

                permissions: Loadable.idle() as Loading<HonuPermission[]>,
                permissionSearch: "" as string,

                selected: {
                    account: null as HonuAccount | null,
                    permissions: Loadable.idle() as Loading<HonuAccountPermission[]>
                },

                create: new HonuAccount() as HonuAccount
            }
        },

        created: function(): void {
            document.title = `Honu / Accounts`;
        },

        mounted: function(): void {
            this.getAccounts();
            this.getPermissions();
        },

        methods: {
            /**
             * Load all accounts that Honu knowns of
             */
            getAccounts: async function(): Promise<void> {
                this.accounts = Loadable.loading();
                this.accounts = await HonuAccountApi.getAll();
            },

            /**
             * Load the Honu permissions that are available
             */
            getPermissions: async function(): Promise<void> {
                this.permissions = Loadable.loading();
                this.permissions = await HonuPermissionApi.getAll();
            },

            /**
             * View an account, loading the permissions and setting the selected user
             * @param accountID ID of the honu account to view
             */
            viewAccount: function(accountID: number): void {
                console.log(`viewing account ${accountID}`);
                if (this.accounts.state != "loaded") {
                    return;
                }

                if (this.selected.account != null && this.selected.account.id == accountID) {
                    this.selected.account = null;
                    this.selected.permissions = Loadable.idle();
                } else {
                    this.selected.account = this.accounts.data.find(iter => iter.id == accountID) || null;
                    this.getAccountPermission(accountID);
                }
            },

            /**
             * Get the account permissions for a user
             * @param accountID Account ID to load the permissions of
             */
            getAccountPermission: async function(accountID: number): Promise<void> {
                this.selected.permissions = Loadable.loading();
                this.selected.permissions = await HonuAccountPermissionApi.getByAccountID(accountID);
            },

            /**
             * Remove a permssion from the currently selected user
             * 
             * @param permID Permission to remove
             */
            removePermission: async function(permID: number): Promise<void> {
                if (this.selected.account == null) {
                    return;
                }

                const accountID: number = this.selected.account.id;

                this.selected.permissions = Loadable.loading();

                const response: Loading<void> = await HonuAccountPermissionApi.delete(permID);
                if (response.state == "loaded") {
                    Toaster.add("Permission removed", "Removed permission", "success");
                } else if (response.state == "error" || response.state == "notfound") {
                    Toaster.add("Failed to remove permission", `${response.state}: ${response.message}`, "danger");
                }

                this.getAccountPermission(accountID);
            },

            /**
             * Show the modal to create an account
             */
            showCreateModal: function(): void {
                $("#account-create-modal").modal("show");
            },

            /**
             * Actually create an accoutn
             */
            createAccount: async function(): Promise<void> {
                const id: Loading<number> = await HonuAccountApi.create(this.create);
                if (id.state == "loaded") {
                    Toaster.add("Account created", `Successfully created an account for ${this.create.name}/${this.create.email}`, "success");
                } else if (id.state == "error") {
                    Toaster.add("Failed to create account", `Failed to create account<br>${id.state}: ${id.message}`, "danger");
                }
                this.getAccounts();
            },

            /**
             * Show the modal to add a permission to an account
             */
            showPermissionModal: async function(): Promise<void> {
                $("#add-permission-modal").modal("show");
                this.$nextTick(() => {
                    document.getElementById("add-permission-search")?.focus();
                });
            },

            /**
             * Add a permission to the currently selected account
             * 
             * @param perm Permission to add to the currently selected account
             */
            addPermission: async function(perm: string): Promise<void> {
                if (this.permissions.state == "loaded") {
                    if (this.permissions.data.find(iter => iter.id.toLowerCase() == perm.toLowerCase()) == undefined) {
                        console.warn(`AccountManagement> cannot add permission '${perm}', does not exist`);
                        return;
                    }
                } else {
                    console.warn(`Cannot validated if '${perm}' is a valid permission, permissions.state is ${this.permissions.state}, needed 'loaded'`);
                }

                if (perm == null || perm.length == 0) {
                    return;
                }

                if (this.selected.account == null) {
                    return;
                }

                const id: Loading<number> = await HonuAccountPermissionApi.insert(this.selected.account.id, perm);

                if (id.state == "loaded") {
                    Toaster.add("Permission added", `Permission ${perm} added`, "success");
                } else if (id.state == "error" || id.state == "notfound") {
                    Toaster.add("Failed to add permission", `${id.state}: ${id.message}`, "danger");
                }

                if (this.selected.account != null) {
                    this.getAccountPermission(this.selected.account.id);
                }
            },

            addPermissionEnterWrapper: function(): void {
                if (this.permissions.state != "loaded") {
                    return;
                }

                const perm: string = this.permissionSearch.toLowerCase();

                if (this.permissions.data.find(iter => iter.id.toLowerCase() == perm) != undefined) {
                    this.addPermission(this.permissionSearch);
                    return;
                }

                if (this.filteredPermissions.length == 1) {
                    this.addPermission(this.filteredPermissions[0].id);
                }
            },

            /**
             * Deactivate a Honu account
             * @param accountID ID of the account to deactivate
             */
            deactivateAccount: async function(accountID: number): Promise<void> {
                const conf: boolean = confirm(`Are you sure you want to deactivate this account?`);
                if (conf != true) {
                    return;
                }

                const response: Loading<void> = await HonuAccountApi.deactivate(accountID);
                if (response.state == "loaded") {
                    Toaster.add("Account deactiviated", `Successfully deactivated account ${accountID}`, "success");
                    this.selected.account = null;
                    this.getAccounts();
                } else {
                    Toaster.add("Error", `Failed to deactivatet account: <br>${response.state}`, "danger");
                }
            }
        },

        computed: {
            displayedAccounts: function(): HonuAccount[] {
                if (this.accounts.state != "loaded") {
                    return [];
                }

                if (this.showDeactivated == true) {
                    return this.accounts.data;
                }

                return this.accounts.data.filter(iter => iter.deletedOn == null);
            },

            filteredPermissions: function(): HonuPermission[] {
                if (this.permissions.state != "loaded") {
                    return [];
                }

                const search: string = this.permissionSearch.toLowerCase();

                return this.permissions.data.filter(iter => {
                    return iter.id.toLowerCase().indexOf(search) > -1
                        || iter.description.toLowerCase().indexOf(search) > -1;
                });
            },

            validPermission: function(): boolean {
                if (this.permissions.state != "loaded") {
                    return false;
                }

                return this.permissions.data.find(iter => iter.id.toLowerCase() == this.permissionSearch.toLowerCase()) != undefined;
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            InfoHover, Busy, Collapsible, ToggleButton
        }
    });
    export default AccountManagement;
</script>