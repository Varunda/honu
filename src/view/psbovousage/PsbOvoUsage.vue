<template>
    <div style="display: grid; grid-template-rows: min-content 1fr; gap: 0.5rem; max-height: 100vh; max-width: 100vw;">
        <honu-menu>
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/psb">PSB</a>
            </li>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/psb/ovousage">OvO usage</a>
            </li>
        </honu-menu>

        <div style="display: grid; grid-template-columns: 280px 280px 1fr; gap: 1rem; overflow: hidden;">
            <div class="psb-contacts d-flex flex-column overflow-hidden border-right pr-3" style="gap: 0.5rem;">
                <div class="flex-grow-0">
                    <h4>Groups</h4>

                    <div>
                        <input type="text" class="form-control" v-model="filter.contacts" />
                    </div>
                </div>

                <div class="flex-grow-1 overflow-y-auto">
                    <div v-if="contacts.state == 'idle'"></div>
                    <div v-else-if="contacts.state == 'loading'">
                        <busy class="honu-busy honu-busy-sm"></busy>
                        loading...
                    </div>

                    <template v-else-if="contacts.state == 'loaded'">
                        <div class="list-group list-group-sm">
                            <div v-for="group in groups" class="list-group-item border" @click="selectGroup(group)" :class="{ 'list-group-item-primary': selected.group == group }">
                                {{group}}
                            </div>

                        </div>

                        <div v-if="filter.contacts != ''" @click="filter.contacts = ''" class="text-muted">
                            showing {{groups.length}} groups of {{contacts.data.length}}
                        </div>
                    </template>

                    <div v-else-if="contacts.state == 'error'">
                        <api-error :error="contacts.problem"></api-error>
                    </div>

                    <div v-else>
                        unchecked state of contacts: {{contacts.state}}
                    </div>
                </div>
            </div>

            <div class="psb-reservations d-flex flex-column overflow-hidden border-right pr-3" style="gap: 0.5rem;">
                <div class="flex-grow-0">
                    <div class="h4">
                        Reservations made
                    </div>

                    <span class="text-muted">
                        contacts for
                    </span>

                    <strong>
                        {{selected.group}}:
                    </strong>

                    <span v-if="selected.group != ''" class="flex-grow-0">
                        <span v-for="contact in selectedGroupContacts" class="badge badge-primary badge-pill mr-1">
                            {{contact.name}}
                        </span>
                    </span>
                </div>

                <div v-if="selected.group != ''" class="flex-grow-1 overflow-y-auto border-top pt-2">
                    <div v-if="selected.sheets.state == 'idle'"></div>

                    <div v-else-if="selected.sheets.state == 'loading'">
                        <busy class="honu-busy honu-busy-lg"></busy>
                        loading...

                        <div class="text-muted">
                            this data can take 30+ seconds to load if data is not already cached
                        </div>
                    </div>

                    <div v-else-if="selected.sheets.state == 'loaded'" class="list-group list-group-sm">
                        <div v-for="sheet in selectedGroupSheets" @click="selectSheet(sheet.id)" class="list-group-item border"
                             :class="{ 'list-group-item-primary': selected.fileId == sheet.id }">

                            {{sheet.name}}
                        </div>
                    </div>

                    <div v-else-if="selected.sheets.state == 'error'">
                        <api-error :error="selected.sheets.problem"></api-error>
                    </div>

                    <div v-else>
                        unchecked state of selected.sheets: {{selected.sheets.status}}
                    </div>
                </div>
            </div>

            <div class="psb-usage overflow-y-auto">
                <div class="border-bottom pb-2 mb-2">
                    <span v-if="selected.fileId != ''" class="h2">
                        <a :href="'https://docs.google.com/spreadsheets/d/' + selected.fileId" class="d-inline">
                            <span v-if="selected.file != null">
                                {{selected.file.name}}
                            </span>
                            <span v-else class="text-danger">
                                failed to find file {{selected.fileId}} from loaded files
                            </span>
                        </a>
                    </span>

                    <div v-if="selected.sheet.state == 'loaded'" class="d-inline">
                        <span class="h4">
                            at {{selected.sheet.data.when | moment}}

                            <span v-if="selected.sheet.data.when > now" class="text-info">
                                (this reservation has not occured yet!)
                            </span>
                        </span>

                        <div class="h5">
                            sent to:
                            <span v-for="contact in selectedSheetContacts" class="badge badge-primary badge-pill">
                                {{contact.name}}
                            </span>
                        </div>

                        <div v-if="selected.usage.state == 'loading'" class="alert alert-info text-center">
                            <busy class="honu-busy honu-busy-sm"></busy>
                            usage data is currently loading... (this can take a while)
                        </div>
                    </div>
                </div>

                <div v-if="selected.sheet.state == 'idle'"></div>

                <div v-else-if="selected.sheet.state == 'loading'">
                    <busy class="honu-busy honu-busy-sm"></busy>
                    loading...
                </div>

                <div v-else-if="selected.sheet.state == 'loaded'">

                    <div class="">
                        <table class="table">
                            <thead class="table-secondary">
                                <tr>
                                    <th>account</th>
                                    <th>username</th>
                                    <th>player</th>
                                    <th>sessions</th>
                                    <th>errors</th>
                                </tr>
                            </thead>

                            <tbody>
                                <tr v-for="account in selectedSheetUsage">
                                    <td>
                                        PSBx{{account.sheetUsage.accountNumber.padStart(4, '0')}}
                                    </td>

                                    <td>
                                        {{account.sheetUsage.username}}
                                    </td>

                                    <td>
                                        <span v-if="account.sheetUsage.player == null" class="text-muted">
                                            not used
                                        </span>
                                        
                                        <span v-else>
                                            {{account.sheetUsage.player}}
                                        </span>
                                    </td>

                                    <td>
                                        <span v-if="selected.usage.state == 'loading'">
                                            <busy class="honu-busy honu-busy-sm"></busy>
                                        </span>

                                        <a v-for="session in account.sessions" :href="'/session/' + session.id">{{session.id}}</a>

                                        <span v-if="account.sessions.length == 0" class="text-muted">
                                            --
                                        </span>
                                    </td>

                                    <td>
                                        <span v-if="selected.usage.state == 'loading'" class="text-muted">
                                            --
                                        </span>
                                        
                                        <span v-else>
                                            <span v-if="account.sheetUsage.player == null && account.sessions.length > 0" class="text-danger">
                                                account has sessions, but no user provided
                                            </span>

                                            <span v-else class="text-muted">
                                                --
                                            </span>
                                        </span>

                                    </td>
                                </tr>
                            </tbody>
                        </table>

                    </div>
                </div>
                
                <div v-else-if="selected.sheet.state == 'error'">
                    <api-error :error="selected.sheet.problem"></api-error>
                </div>

                <div v-else>
                    unchecked state of selected.sheet: {{selected.sheet.status}}
                </div>

            </div>

        </div>

    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { Loadable, Loading } from "Loading";
    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuHomepage, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ApiError from "components/ApiError";
    import Busy from "components/Busy.vue";

    import { PsbOvOContact, PsbContactApi } from "api/psb/PsbContactApi";
    import { PsbDriveFile } from "api/psb/PsbDriveFileApi";
    import { PsbOvOSheet, PsbOvOSheetApi, PsbSheetAccountUsage } from "api/psb/PsbSheetApi";

    import "MomentFilter";

    export const PsbOvoUsage = Vue.extend({
        props: {

        },

        data: function() {
            return {
                now: new Date() as Date,

                contacts: Loadable.idle() as Loading<PsbOvOContact[]>,

                selected: {
                    group: "" as string,
                    sheets: Loadable.idle() as Loading<PsbDriveFile[]>,

                    fileId: "" as string,
                    file: null as PsbDriveFile | null,
                    sheet: Loadable.idle() as Loading<PsbOvOSheet>,
                    usage: Loadable.idle() as Loading<PsbSheetAccountUsage[]>
                },

                filter: {
                    contacts: "" as string
                }
            }
        },

        mounted: function(): void {
            this.loadContacts();
        },

        methods: {

            selectGroup: function(group: string): void {
                if (this.selected.group == group) {
                    this.selected.group = "";
                    this.selected.sheets = Loadable.idle();
                } else {
                    this.selected.group = group;
                    this.loadSheets(this.selected.group);
                }
            },

            selectSheet: async function(fileID: string): Promise<void> {
                if (this.selected.fileId == fileID) {
                    this.selected.fileId = "";
                    this.selected.file = null;
                    this.selected.sheet = Loadable.idle();
                    this.selected.usage = Loadable.idle();
                    return;
                }

                this.selected.fileId = fileID;

                if (this.selected.sheets.state == "loaded") {
                    this.selected.file = this.selected.sheets.data.find(iter => iter.id == fileID) ?? null;
                } else {
                    console.warn(`sheets are not loaded (how?), cannot select file`);
                }

                this.selected.sheet = Loadable.loading();

                const a = PsbOvOSheetApi.getSheet(fileID);
                a.then((data: Loading<PsbOvOSheet>) => {
                    console.log(`loaded sheet from file ${fileID}`);
                    this.selected.sheet = data;
                });

                this.selected.usage = Loadable.loading();
                const b = PsbOvOSheetApi.getSheetUsage(fileID);
                b.then((data: Loading<PsbSheetAccountUsage[]>) => {
                    console.log(`loaded sheet usage from file ${fileID}`);
                    this.selected.usage = data;
                });

                await Promise.all([a, b]);
                console.log(`finished loading both`);
            },

            loadContacts: async function(): Promise<void> {
                this.contacts = Loadable.loading();
                this.contacts = await PsbContactApi.getOvOContacts();
            },

            loadSheets: async function(name: string): Promise<void> {
                this.selected.sheet = Loadable.idle();
                this.selected.usage = Loadable.idle();

                this.selected.sheets = Loadable.loading();
                this.selected.sheets = await PsbOvOSheetApi.getSheets(name);
            }
        },

        computed: {

            groups: function(): string[] {
                if (this.contacts.state != "loaded") {
                    return [];
                }

                const set: Set<string> = new Set();

                for (const contact of this.contacts.data) {
                    for (const group of contact.groups) {
                        set.add(group.trim());
                    }
                }

                return Array.from(set).filter((iter: string) => {
                    if (this.filter.contacts != "") {
                        return iter.indexOf(this.filter.contacts) > -1;
                    }
                    return true;
                }).sort((a: string, b: string) => {
                    return a.localeCompare(b);
                });
            },

            selectedGroupContacts: function(): PsbOvOContact[] {
                if (this.contacts.state != "loaded") {
                    return [];
                }

                if (this.selected.group == "") {
                    return [];
                }

                return this.contacts.data.filter(iter => iter.groups.indexOf(this.selected.group) > -1);
            },

            selectedGroupSheets: function(): PsbDriveFile[] {
                if (this.selected.sheets.state != "loaded") {
                    return [];
                }

                return this.selected.sheets.data.sort((a, b) => {
                    return b.name.localeCompare(a.name);
                });
            },

            selectedSheetContacts: function(): PsbOvOContact[] {
                if (this.selected.sheet.state != "loaded") {
                    return [];
                }

                if (this.contacts.state != "loaded") {
                    console.warn(`cannot get contacts in selectedSheetContacts: contacts.state is ${this.contacts.state}, not "loaded"`);
                    return [];
                }

                const emails: string[] = this.selected.sheet.data.emails;
                return this.contacts.data.filter((iter: PsbOvOContact) => {
                    return emails.indexOf(iter.email) > -1;
                });
            },

            selectedSheetUsage: function(): PsbSheetAccountUsage[] {
                if (this.selected.usage.state == "loaded") {
                    return this.selected.usage.data;
                }

                if (this.selected.sheet.state == "loading") {
                    return [];
                }

                if (this.selected.sheet.state == "loaded" && this.selected.usage.state == "loading") {
                    return this.selected.sheet.data.accounts.map(iter => {
                        return {
                            sheetUsage: iter,
                            sessions: []
                        };
                    });
                }

                console.log(`DEBUG> sheet.state ${this.selected.sheet.state}, usage.state ${this.selected.usage.state}`);

                return [];
            }

        },

        components: {
            ApiError, Busy,
            HonuMenu, MenuSep, MenuHomepage, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage
        }

    });

    export default PsbOvoUsage;
</script>