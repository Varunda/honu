<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/psb">PSB</a>
            </li>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                Practice
            </li>
        </honu-menu>

        <div class="mb-2 d-flex">
            <div class="border-right px-2">
                <h4>Create a block</h4>

                <div class="input-group">
                    <input type="text" v-model="create.tag" placeholder="Tag" />
                    <input type="number" v-model.number="create.count" placeholder="Amount" />

                    <div class="input-group-addon">
                        <toggle-button v-model="create.leadingZeroes">
                            Leading zeroes
                            <info-hover text="If the character have leading zeroes, such as D1RExPratice01, instead of D1RExPractice1"></info-hover>
                        </toggle-button>

                        <toggle-button v-model="create.lowercasePractice">
                            Lowercase practice
                        </toggle-button>
                    </div>

                    <div class="input-group-addon">
                        <button type="button" class="btn btn-primary" @click="createBlockWrapper">
                            Create 
                        </button>
                    </div>
                </div>
            </div>

            <div class="border-right px-2">
                <h4>Recheck all</h4>

                <button @click="recheckAll" type="button" class="btn btn-primary">Recheck all</button>
            </div>

            <div class="border-right px-2">
                <h4>Show deleted</h4>

                <toggle-button v-model="search.deleted">Show deleted</toggle-button>
            </div>
        </div>

        <div v-if="recheckProgress.show == true" class="mb-2">
            <h2>Recheck progress</h2>

            <progress-bar :progress="recheckProgress.progress" :total="recheckProgress.total"></progress-bar>
        </div>

        <div v-if="groupedAccounts.state == 'loaded'">
            <h1 class="wt-header mb-1">
                Practice blocks
            </h1>

            <div class="mb-2">
                <input v-model="search.tag" type="text" placeholder="Filter tag" class="form-control d-inline-block" style="max-width: 12ch;" />

                <toggle-button v-model="search.problems">
                    Show only blocks with problems
                </toggle-button>
            </div>

            <template v-for="group in groupedAccounts.data">
                <collapsible header-text="" :show="false">
                    <template v-slot:header>
                        <div class="d-flex w-100">
                            <div class="flex-grow-0 d-inline-block">
                                <span style="width: 5ch;" class="text-center d-inline-block">
                                    {{group.tag}}
                                </span>
                                <span style="width: 10ch;" class="d-inline-block">
                                    {{group.count}} accounts
                                </span>

                                <span v-if="group.problems.length > 1">
                                    <info-hover :allow-html="true" icon="exclamation-circle" class="text-danger"
                                        :text="'There are ' + group.problems.length + ' problems: <br>' + group.problems.join('<br>')">

                                    </info-hover>
                                </span>
                            </div>

                            <div class="ml-3 flex-grow-1">
                                <div class="progress h-100 h2 mb-0">
                                    <div v-if="group.countOk > 0" class="progress-bar bg-success h3 px-1 mw-fit border-right mb-0" :style="{ width: group.countOk / group.count * 100 + '%' }">{{group.countOk}} OK</div>
                                    <div v-if="group.countUnused > 0" class="progress-bar bg-warning h3 px-1 mw-fit border-right mb-0" :style="{ width: group.countUnused / group.count * 100 + '%' }">{{group.countUnused}} unused</div>
                                    <div v-if="group.countMissing > 0" class="progress-bar bg-danger h3 px-1 mw-fit border-right mb-0" :style="{ width: group.countMissing / group.count * 100 + '%' }">{{group.countMissing}} missing</div>
                                    <div v-if="group.countDeleted > 0" class="progress-bar bg-danger h3 px-1 mw-fit mb-0" :style="{ width: group.countDeleted / group.count * 100 + '%' }">{{group.countDeleted}} deleted</div>
                                </div>
                            </div>
                        </div>
                    </template>

                    <slot>
                        <practice-account-list :accounts="group.accounts"></practice-account-list>

                        <div class="mb-2">
                            <h4>Manage</h4>
                            <button @click="deleteBlockWrapper(group.tag)" type="button" class="btn btn-danger">Delete block</button>
                            <button @click="recheckBlock(group.tag)" type="button" class="btn btn-info">Recheck</button>
                        </div>
                    </slot>
                </collapsible>
            </template>
        </div>

        <div v-else-if="groupedAccounts.state == 'loading'">
            <busy class="honu-busy-lg"></busy>
            Loading...
        </div>

        <div v-else-if="groupedAccounts.state == 'error'">
            Error loading accounts: {{groupedAccounts.message}}
        </div>

        <div v-else>
            Unchecked state of groupedAccounts: {{groupedAccounts.state}}
        </div>

        <div class="modal" id="psb-account-modal">
            <psb-account-modal v-if="view.opened == true" :account="view.account">
            </psb-account-modal>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";

    import { Loadable, Loading } from "Loading";
    import Toaster from "Toaster";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import Collapsible from "components/Collapsible.vue";
    import ToggleButton from "components/ToggleButton";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";
    import ProgressBar from "components/ProgressBar.vue";

    import PracticeAccountList from "./components/PracticeAccountList.vue";
    import PsbAccountModal from "components/psb/PsbAccountModal.vue";

    import { FlatPsbNamedAccount, PsbAccountStatus, PsbAccountType, PsbCharacterStatus, PsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";
    import { PsCharacter } from "api/CharacterApi";

    import EventBus from "EventBus";

    type GroupedAccount = {
        accounts: Loading<FlatPsbNamedAccount[]>;
        tag: string;
        count: number;
        countOk: number;
        countUnused: number;
        countMissing: number;
        countDeleted: number;
        problems: string[];
    };

    export const PsbPractice = Vue.extend({
        props: {

        },

        data: function() {
            return {
                accounts: Loadable.idle() as Loading<FlatPsbNamedAccount[]>,

                search: {
                    tag: "" as string,
                    deleted: false as boolean,
                    problems: false as boolean
                },

                create: {
                    tag: "" as string,
                    count: 0 as number,
                    leadingZeroes: false as boolean,
                    lowercasePractice: false as boolean,
                    ovo: false as boolean
                },

                recheckProgress: {
                    show: false as boolean,
                    total: 0 as number,
                    progress: 0 as number
                },

                view: {
                    opened: false as boolean,
                    account: null as FlatPsbNamedAccount | null
                },
            }
        },

        created: function(): void {
            document.title = `Honu / PSB / Practice`;

            this.bindData();

            EventBus.$on("show-account", this.showAccount);
        },

        mounted: function(): void {
            this.$nextTick(() => {
                $("#psb-account-modal").on("hide.bs.modal", () => {
                    this.view.opened = false;
                    this.view.account = null;
                });

                EventBus.$on("rebind-accounts", async () => {
                    console.log(`Rebinding accounts`);
                    await this.bindData();

                    if (this.view.account != null) {
                        this.updateAccount(this.view.account.id);
                    }
                });
            });
        },

        methods: {
            bindData: async function(): Promise<void> {
                this.accounts = Loadable.loading();
                this.accounts = await PsbNamedAccountApi.getByTypeID(PsbAccountType.PRACTICE);
            },

            showAccount: function(accountID: number): void {
                this.updateAccount(accountID);
                this.view.opened = true;
                this.$nextTick(() => {
                    this.openViewModal();
                });
            },

            openViewModal: function(): void {
                $("#psb-account-modal").modal("show");
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

            createBlockWrapper: async function(): Promise<void> {
                if (this.create.tag.trim().length <= 0) {
                    return;
                }
                if (this.create.count <= 0) {
                    return;
                }

                await this.createBlock(this.create.tag, this.create.count, this.create.leadingZeroes);
            },

            createBlock: async function(tag: string, count: number, leadingZeroes: boolean): Promise<void> {
                Toaster.add(`Creating block`, `Creating ${count} practice accounts for ${tag}`, "info");

                let successCount: number = 0;
                let errorCount: number = 0;
                for (let i = 0; i < count; ++i) {
                    // for ovo, need to use 0001 instead of 01, so get the power of ten, which is how many zeroes to show
                    const magnitude: number = Math.ceil(Math.log10(count));

                    let number: string = `${i + 1}`;
                    if (leadingZeroes == true) {
                        number = (i + 1).toString().padStart(magnitude, "0");
                    }

                    let name: string = `${this.create.lowercasePractice == false ? "Practice" : "practice"}${number}`;
                    if (this.create.ovo == true) {
                        name = `${number}`;
                    }

                    const newAccount: Loading<PsbNamedAccount> = await PsbNamedAccountApi.create(tag, name, PsbAccountType.PRACTICE, true);

                    if (newAccount.state == "loaded") {
                        ++successCount;
                    } else if (newAccount.state == "error") {
                        Toaster.add(`Error creating ${tag}x${name}`, `Error: ${newAccount.message}`, "warning");
                        ++errorCount;
                    }
                }

                Toaster.add(`Done!`, `Created ${successCount}/${count} accounts, ${errorCount} errored `, "success");

                await this.bindData();
            },

            deleteBlockWrapper: async function(tag: string): Promise<void> {
                const conf: boolean = confirm(`Are you sure you want to delete ALL accounts in the block ${tag}?`);

                if (conf != true) {
                    return;
                }

                await this.deleteBlock(tag);
            },

            deleteBlock: async function(tag: string): Promise<void> {
                if (this.accounts.state != "loaded") {
                    return console.warn(`cannot delete block ${tag}: accounts is not 'loaded'`);
                }

                for (const account of this.accounts.data) {
                    if (account.tag != tag) {
                        continue;
                    }
                    if (account.account.deletedAt != null) {
                        continue;
                    }

                    await PsbNamedAccountApi.deleteByID(account.id);
                    Toaster.add(`Deleted ${account.id}`, `Successfully deleted ${account.tag}x${account.name}`, "success");
                }

                await this.bindData();
            },

            recheckBlock: async function(tag: string): Promise<void> {
                if (this.accounts.state != "loaded") {
                    return console.warn(`cannot recheck block ${tag}: accounts is not 'loaded'`);
                }

                Toaster.add(`Rechecking block`, `Rechecking accounts for ${tag}`, "info");

                this.recheckProgress.show = true;
                this.recheckProgress.total = this.accounts.data.filter(iter => iter.tag == tag && iter.account.deletedAt == null).length;
                this.recheckProgress.progress = 0;

                for (const account of this.accounts.data) {
                    if (account.account.deletedAt != null) {
                        continue;
                    }

                    if (account.tag != tag) {
                        continue;
                    }

                    await PsbNamedAccountApi.recheckByID(account.id);
                    ++this.recheckProgress.progress;
                }

                Toaster.add(`Rechecked accounts for ${tag}`, `Recheck accounts`, "success");

                await this.bindData();

                this.recheckProgress.show = false;
            },

            recheckAll: async function(): Promise<void> {
                if (this.accounts.state != "loaded") {
                    return console.warn(`cannot recheck all: accounts is not 'loaded'`);
                }

                Toaster.add(`Rechecking all`, `Rechecking ${this.accounts.data.length} accounts`, "info");

                this.recheckProgress.show = true;
                this.recheckProgress.total = this.accounts.data.filter(iter => iter.account.deletedAt == null).length;
                this.recheckProgress.progress = 0;

                for (const account of this.accounts.data) {
                    if (account.account.deletedAt != null) {
                        continue;
                    }

                    try {
                        await PsbNamedAccountApi.recheckByID(account.id);
                    } catch (err) {
                        console.error(`error updating ${account.id}`, err);
                    }
                    ++this.recheckProgress.progress;
                }

                await this.bindData();

                this.recheckProgress.show = false;
            },

        },

        computed: {

            groupedAccounts: function(): Loading<GroupedAccount[]> {
                if (this.accounts.state != "loaded") {
                    return Loadable.rewrap(this.accounts);
                }

                const accounts: FlatPsbNamedAccount[] = this.accounts.data;
                const map: Map<string, GroupedAccount> = new Map();

                const addProblems = function(acc: FlatPsbNamedAccount, arr: GroupedAccount, faction: string, charStatus: number, character: PsCharacter | null): void {
                    const charName: string = character?.name ?? `${acc.tag}x${acc.name}${faction}`;

                    switch (charStatus) {
                        case PsbCharacterStatus.doesNotExist: arr.problems.push(`Character ${charName} does not exist`); break;
                        case PsbCharacterStatus.deleted: arr.problems.push(`Character ${charName} is deleted`); break;
                        case PsbCharacterStatus.remade: arr.problems.push(`Character ${charName} was remade`); break;
                    }
                    if (character != null && character?.worldID != 19) {
                        arr.problems.push(`Character ${charName}${faction} is not on Jaeger`);
                    }
                };

                for (const acc of accounts) {
                    // don't include empty tags
                    const tag: string | null = acc.tag; 
                    if (tag == null) {
                        continue;
                    }

                    if (acc.account.deletedAt != null && this.search.deleted == false) {
                        continue;
                    }

                    let arr: GroupedAccount | undefined = map.get(tag);
                    if (arr == undefined) {
                        arr = {
                            accounts: Loadable.loaded([]),
                            tag: tag,
                            count: 0,
                            countOk: 0,
                            countUnused: 0,
                            countMissing: 0,
                            countDeleted: 0,
                            problems: []
                        };
                    }

                    if (arr.accounts.state != "loaded") {
                        throw `expected arr.accounts.state for tag ${tag} to be 'loaded', was ${arr.accounts.state}`;
                    }

                    arr.accounts.data.push(acc);
                    ++arr.count;

                    addProblems(acc, arr, "VS", acc.account.vsStatus, acc.vsCharacter);
                    addProblems(acc, arr, "NC", acc.account.ncStatus, acc.ncCharacter);
                    addProblems(acc, arr, "TR", acc.account.trStatus, acc.trCharacter);
                    addProblems(acc, arr, "NS", acc.account.nsStatus, acc.nsCharacter);

                    if (acc.status == PsbAccountStatus.OK) {
                        ++arr.countOk;
                    } else if (acc.status == PsbAccountStatus.MISSING) {
                        ++arr.countMissing;
                    } else if (acc.status == PsbAccountStatus.UNUSED) {
                        ++arr.countUnused;
                    } else if (acc.status == PsbAccountStatus.DELETED) {
                        ++arr.countDeleted;
                    }

                    map.set(tag, arr);
                }

                let blocks: GroupedAccount[] = Array.from(map.values());

                if (this.search.tag.trim().length > 0) {
                    blocks = blocks.filter(iter => iter.tag.indexOf(this.search.tag) > -1);
                }

                if (this.search.problems == true) {
                    blocks = blocks.filter(iter => iter.problems.length > 0);
                }

                blocks = blocks.sort((a, b) => a.tag.localeCompare(b.tag));

                return Loadable.loaded(blocks);
            }

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            ToggleButton, InfoHover, Collapsible, Busy, ProgressBar,
            PracticeAccountList, PsbAccountModal
        }
    });
    export default PsbPractice;
</script>