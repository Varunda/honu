<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                PSB {{typeName}}
            </li>
        </honu-menu>

        <div class="mb-3 d-flex">
            <div class="flex-grow-1">
                <h4>Filters</h4>
                <div>
                    <button type="button" class="btn" :class="[ filter.missingCharacters ? 'btn-primary' : 'btn-secondary' ]" @click="filter.missingCharacters = !filter.missingCharacters">
                        <span v-if="filter.missingCharacters">Y</span>
                        <span v-else>N</span>
                    </button>

                    <span>
                        Only show accounts with missing characters
                    </span>
                </div>

                <div>
                    <button type="button" class="btn" :class="[ filter.mismatchFactions ? 'btn-primary' : 'btn-secondary' ]" @click="filter.mismatchFactions = !filter.mismatchFactions">
                        <span v-if="filter.mismatchFactions">Y</span>
                        <span v-else>N</span>
                    </button>

                    <span>
                        Only show accounts that have characters on the wrong faction (VS on NC for example) (excludes NS)
                    </span>
                </div>

                <div>
                    <button type="button" class="btn" :class="[ filter.wrongWorlds ? 'btn-primary' : 'btn-secondary' ]" @click="filter.wrongWorlds = !filter.wrongWorlds">
                        <span v-if="filter.wrongWorlds">Y</span>
                        <span v-else>N</span>
                    </button>

                    <span>
                        Only show accounts not on Jaeger (includes NSO)
                    </span>
                </div>

                <div>
                    <button type="button" class="btn" :class="[ filter.deleted ? 'btn-primary' : 'btn-secondary' ]" @click="filter.deleted = !filter.deleted">
                        <span v-if="filter.deleted">Y</span>
                        <span v-else>N</span>
                    </button>

                    <span>
                        Show deleted accounts
                    </span>
                </div>

                <div>
                    <button type="button" class="btn" :class="[ filter.warnings ? 'btn-primary' : 'btn-secondary' ]" @click="filter.warnings = !filter.warnings">
                        <span v-if="filter.warnings">Y</span>
                        <span v-else>N</span>
                    </button>

                    <span>
                        Only show accounts with problems
                    </span>
                </div>
            </div>

            <div class="flex-grow-1">
                <h4>Settings</h4>

                <div class="mb-2">
                    Padding
                    <select class="form-control" v-model="padding">
                        <option>compact</option>
                        <option>normal</option>
                        <option>expanded</option>
                    </select>
                </div>

                <div>
                    <button class="btn btn-danger" @click="recheckAll" :disabled="recheck.going == true">
                        Recheck all
                    </button>
                    <span v-show="recheck.going == true">
                        Cannot recheck while a recheck is going. Refresh to cancel
                    </span>
                </div>
            </div>
        </div>

        <div v-if="recheck.opened == true" class="mb-2">
            <h2>
                Recheck progress
                <busy v-if="recheck.going == true" class="honu-busy honu-busy-sm"></busy>
            </h2>

            <progress-bar :progress="recheck.progress" :total="recheck.total" :show-percent="true"></progress-bar>

            <hr class="border" />
        </div>

        <a-table
            :entries="filtered"
            :show-filters="true"
            default-sort-field="tag" default-sort-order="asc"
            display-type="table" :row-padding="padding">

            <a-col>
                <a-header>
                    <button @click="openCreateModal" type="button" class="btn btn-success">
                        <span class="fas fa-plus"></span>
                    </button>
                </a-header>

                <a-body v-slot="entry">
                    <a href="#" @click="viewAccount(entry.id)" :class="{ 'text-danger': entry.account.deletedByID != null }">
                        View {{entry.id}}
                    </a>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Player name</b>
                </a-header>

                <a-filter method="input" type="string" field="playerName" max-width="10ch"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span :class="{ 'text-danger': entry.account.deletedBy != null }">
                        {{entry.playerName}}
                        <info-hover v-if="entry.account.deletedBy != null" text="This account has been deleted"></info-hover>
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="tag">
                <a-header>
                    <b>Tag</b>
                </a-header>

                <a-filter method="input" type="string" field="tag" max-width="10ch"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span :class="{ 'text-danger': entry.account.deletedBy != null }">
                        {{entry.tag}}
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="name">
                <a-header>
                    <b>Name</b>
                </a-header>

                <a-filter method="input" type="string" field="name" max-width="24ch"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span :class="{ 'text-danger': entry.account.deletedBy != null }">
                        {{entry.name}}
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Status</b>
                </a-header>

                <a-filter method="dropdown" type="string" field="status"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span v-if="entry.status == 'Deleted'" class="text-muted">
                        Deleted
                    </span>

                    <span v-else-if="entry.status == 'Missing'" class="text-danger">
                        Missing
                    </span>

                    <span v-else-if="entry.status == 'Unused'" class="text-warning">
                        Unused
                        <info-hover text="Last login >90 days">
                        </info-hover>
                    </span>

                    <span v-else-if="entry.status == 'Ok'" class="text-success">
                        Ok
                    </span>

                    <span v-else>
                        Unchecked status {{entry.status}}
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="secondsUsage">
                <a-header>
                    <b>Time used</b>
                    <info-hover text="How much time has been spent on the VS + NC + TR characters in the last 90 days">
                    </info-hover>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.secondsUsage | mduration}}
                </a-body>
            </a-col>

            <a-col sort-field="timestamp">
                <a-header>
                    <b>Created</b>
                    <info-hover text="When this named account was created">
                    </info-hover>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.timestamp | timeAgo}}
                </a-body>
            </a-col>

            <a-col sort-field="lastUsed">
                <a-header>
                    <b>Last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <div class="border-right my-n2 py-2">
                        <span v-if="entry.lastUsed != null" :title="entry.lastUsed | moment">
                            {{entry.lastUsed | timeAgo}}
                        </span>
                        <span v-else class="text-danger">
                            --
                        </span>
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="vsName">
                <a-header>
                    <b>VS</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-cell :id="entry.vsID" :status="entry.account.vsStatus"
                        :character="entry.vsCharacter" faction-id="1">
                    </psb-named-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="vsLastLogin">
                <a-header>
                    <b>VS last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-login :character="entry.vsCharacter"></psb-named-character-login>
                </a-body>
            </a-col>

            <a-col sort-field="ncName">
                <a-header>
                    <b>NC</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-cell :id="entry.ncID" :status="entry.account.ncStatus"
                        :character="entry.ncCharacter" faction-id="2">
                    </psb-named-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="ncLastLogin">
                <a-header>
                    <b>NC last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-login :character="entry.ncCharacter"></psb-named-character-login>
                </a-body>
            </a-col>

            <a-col sort-field="trName">
                <a-header>
                    <b>TR</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-cell :id="entry.trID" :status="entry.account.trStatus"
                        :character="entry.trCharacter" faction-id="3">
                    </psb-named-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="trLastLogin">
                <a-header>
                    <b>TR last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-login :character="entry.trCharacter"></psb-named-character-login>
                </a-body>
            </a-col>

            <a-col sort-field="nsName">
                <a-header>
                    <b>NS</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-cell :id="entry.nsID" :status="entry.account.nsStatus"
                        :character="entry.nsCharacter" faction-id="4">
                    </psb-named-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="nsLastLogin">
                <a-header>
                    <b>NS last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-named-character-login :character="entry.nsCharacter"></psb-named-character-login>
                </a-body>
            </a-col>
        </a-table>

        <div class="modal" id="psb-account-modal">
            <psb-account-modal v-if="view.opened == true" :account="view.account">
            </psb-account-modal>
        </div>

        <div class="modal" id="account-create-modal">
            <psb-named-account-create-modal v-if="create.opened == true" :entries="filtered.data" :type-id="TypeId">
            </psb-named-account-create-modal>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loadable, Loading } from "Loading";
    import EventBus from "EventBus";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import PsbAccountModal from "components/psb/PsbAccountModal.vue";
    import ProgressBar from "components/ProgressBar.vue";
    import Busy from "components/Busy.vue";

    import PsbNamedAccountCreateModal from "./components/PsbNamedAccountCreateModal.vue";

    import Toaster from "Toaster";
    import "MomentFilter";
    import "filters/CharacterName";
    import "filters/TimeAgoFilter";
    import "filters/FactionNameFilter";

    import { FlatPsbNamedAccount, PsbAccountType, PsbNamedAccountApi } from "api/PsbNamedAccountApi";
    import { PsCharacter } from "api/CharacterApi";

    const PsbNamedCharacterCell = Vue.extend({
        props: {
            id: { type: String, required: false },
            character: { type: Object as PropType<PsCharacter | null>, required: false },
            status: { type: Number, required: true },
            FactionId: { type: String, required: true }
        },

        template: `
            <span v-if="id == null" class="text-danger">
                <info-hover icon="exclamation-circle" class="text-danger"
                    text="This character is missing">
                </info-hover>
            </span>

            <span v-else>
                <a v-if="character == null" :href="'/c/' + id">
                    &lt;missing {{id}}&gt;
                </a>
                    
                <span v-else>
                    <info-hover v-if="character.worldID != 19" icon="exclamation" 
                        class="text-warning" text="This character is on the wrong server">
                    </info-hover>

                    <info-hover v-if="status == 2" icon="exclamation"
                        class="text-warning" text="This character does not exist">
                    </info-hover>

                    <info-hover v-else-if="status == 3" icon="exclamation"
                        class="text-warning" text="This character has been deleted">
                    </info-hover>

                    <info-hover v-else-if="status == 4" icon="exclamation"
                        class="text-warning" text="This character has been recreated">
                    </info-hover>

                    <a :href="'/c/' + id">
                        View
                    </a>
                </span>
            </span>
        `,

        components: {
            InfoHover
        }
    });

    const PsbNamedCharacterLogin = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter | null>, required: false }
        },

        template: `
            <span v-if="character == null">

            </span>

            <span v-else>
                <span v-if="character.dateLastLogin == null || character.dateLastLogin.getTime() == 0">
                    &lt;never signed in&gt;
                </span>

                <span v-else :title="character.dateLastLogin | moment">
                    {{character.dateLastLogin | timeAgo}}
                </span>
            </span>
        `
    });

    export const PsbNamed = Vue.extend({
        props: {
            TypeId: { type: Number, required: false, default: 1 }
        },

        data: function() {
            return {
                accounts: Loadable.idle() as Loading<FlatPsbNamedAccount[]>,
                wrapped: Loadable.idle() as Loading<FlatPsbNamedAccount[]>,

                filter: {
                    missingCharacters: false as boolean,
                    mismatchFactions: false as boolean,
                    wrongWorlds: false as boolean,
                    deleted: false as boolean,
                    warnings: false as boolean
                },

                typeName: "" as string,

                create: {
                    opened: false as boolean,
                },

                recheck: {
                    going: false as boolean,
                    opened: false as boolean,
                    progress: 0 as number,
                    total: 0 as number,
                    current: "" as string
                },

                view: {
                    opened: false as boolean,
                    account: null as FlatPsbNamedAccount | null
                },

                padding: "normal" as "normal" | "expanded" | "compact"
            }
        },

        created: function(): void {
            document.title = `Honu / PSB ${PsbAccountType.getName(this.TypeId)}`;
        },

        mounted: function(): void {
            this.loadAll();

            this.$nextTick(() => {
                $("#psb-account-modal").on("hide.bs.modal", () => {
                    this.view.opened = false;
                    this.view.account = null;
                });

                EventBus.$on("rebind-accounts", async () => {
                    console.log(`Rebinding accounts`);
                    await this.loadAll();

                    if (this.view.account != null) {
                        this.updateAccount(this.view.account.id);
                    }
                });
            });

            this.typeName = PsbAccountType.getName(this.TypeId);
        },

        methods: {
            viewAccount: function(accID: number): void {
                this.updateAccount(accID);
                this.view.opened = true;
                this.$nextTick(() => {
                    this.openViewModal();
                });
            },

            updateAccount: function(accID: number): void {
                if (this.accounts.state != "loaded") {
                    return console.warn(`accounts.state is not loaded, cannot load ${accID}`);
                }

                const acc: FlatPsbNamedAccount | undefined = this.accounts.data.find(iter => iter.id == accID);
                if (acc == undefined) {
                    return console.warn(`failed to find ${accID}`);
                }

                this.view.account = acc;
            },

            openViewModal: function(): void {
                $("#account-create-modal").modal("hide");
                $("#psb-account-modal").modal("show");
            },

            openCreateModal: function(): void {
                this.create.opened = true;
                $("#psb-account-modal").modal("hide");
                $("#account-create-modal").modal("show");
            },

            loadAll: async function(): Promise<void> {
                this.accounts = Loadable.loading();
                this.accounts = await PsbNamedAccountApi.getByTypeID(this.TypeId); 
                if (this.accounts.state == "loaded") {
                    this.updateFilters();
                }
            },

            recheckAll: async function(): Promise<void> {
                if (this.accounts.state != "loaded") {
                    Toaster.add("Error", "Cannot recheck all accounts: accounts is not loaded", "danger");
                    return;
                }

                const accounts: FlatPsbNamedAccount[] = this.accounts.data.filter(iter => iter.account.deletedAt == null);

                this.recheck.going = true;
                this.recheck.opened = true;
                this.recheck.progress = 0;
                this.recheck.total = accounts.length;

                for (const account of accounts) {
                    this.recheck.current = `${account.tag}x${account.name}`;
                    console.log(`rechecking ${account.id}/${account.tag}x${account.name}`);
                    try {
                        await PsbNamedAccountApi.recheckByID(account.id);
                    } catch (err) {
                        console.error(`error updating account ${account.id}: ${err}`);
                        Toaster.add(`Error updating ${account.id}`, `${err}`, "danger");
                    }
                    ++this.recheck.progress;
                }

                Toaster.add("Done!", `Updated ${accounts.length} accounts`, "success");

                this.loadAll();

                this.recheck.going = false;
                setTimeout(() => {
                    this.recheck.opened = false;
                }, 2000);
            },

            updateFilters: function(): void {
                console.log(`PsbNamed> updating filters`);

                if (this.accounts.state != "loaded") {
                    return console.warn(`accounts.state is not loaded, cannot filter`);
                }

                let data: FlatPsbNamedAccount[] = this.accounts.data;

                if (this.filter.missingCharacters) {
                    data = data.filter(iter => iter.missingCharacter == true);
                }

                if (this.filter.mismatchFactions) {
                    data = data.filter(iter => {
                        if ((iter.vsCharacter?.factionID ?? 1) != 1) {
                            return true;
                        }
                        if ((iter.ncCharacter?.factionID ?? 2) != 2) {
                            return true;
                        }
                        if ((iter.trCharacter?.factionID ?? 3) != 3) {
                            return true;
                        }

                        return false;
                    });
                }

                if (this.filter.wrongWorlds == true) {
                    data = data.filter(iter => {
                        if (iter.vsCharacter != null && iter.vsCharacter?.worldID != 19) {
                            return true;
                        }
                        if (iter.ncCharacter != null && iter.ncCharacter?.worldID != 19) {
                            return true;
                        }
                        if (iter.trCharacter != null && iter.trCharacter?.worldID != 19) {
                            return true;
                        }
                        if (iter.nsCharacter != null && iter.nsCharacter?.worldID != 19) {
                            return true;
                        }

                        return false;
                    });
                }

                if (this.filter.deleted == false) {
                    data = data.filter(iter => {
                        return iter.account.deletedAt == null;
                    });
                }

                if (this.filter.warnings == true) {
                    data = data.filter(iter => {
                        return iter.account.vsStatus != 1
                            || iter.account.ncStatus != 1
                            || iter.account.trStatus != 1
                            || iter.account.nsStatus != 1;
                    });
                }

                this.wrapped = Loadable.loaded(data);
            },
        },

        watch: {
            "filter.missingCharacters": function(): void {
                console.log(`PsbNamed> filter.missingCharacters changed`);
                this.updateFilters();
            },

            "filter.mismatchFactions": function(): void {
                console.log(`PsbNamed> filter.mismatchFactions changed`);
                this.updateFilters();
            },

            "filter.wrongWorlds": function(): void {
                console.log(`PsbNamed> filter.wrongWorlds changed`);
                this.updateFilters();
            },

            "filter.deleted": function(): void {
                console.log(`PsbNamed> filter.deleted changed`);
                this.updateFilters();
            },

            "filter.warnings": function(): void {
                console.log(`PsbNamed> filter.warnings changed`);
                this.updateFilters();
            }

        },

        computed: {
            filtered: function(): Loading<FlatPsbNamedAccount[]> {
                if (this.accounts.state != "loaded") {
                    return this.accounts;
                }

                if (this.filter.missingCharacters == true || this.filter.mismatchFactions == true || this.filter.wrongWorlds == true || this.filter.deleted == false || this.filter.warnings == true) {
                    return this.wrapped;
                }

                return this.accounts;
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            PsbAccountModal, PsbNamedAccountCreateModal,
            PsbNamedCharacterCell, PsbNamedCharacterLogin,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ProgressBar,
            Busy
        }

    });
    export default PsbNamed;
</script>