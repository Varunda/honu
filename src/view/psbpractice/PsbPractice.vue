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

        <div class="mb-2">
            <h4>Create a block</h4>

            <div class="input-group">
                <input type="text" v-model="create.tag" placeholder="Tag" />
                <input type="number" v-model.number="create.count" placeholder="Amount" />

                <div class="input-group-addon">
                    <button type="button" class="btn btn-primary" @click="createBlockWrapper">
                        Create 
                    </button>
                </div>
            </div>
        </div>

        <div v-if="groupedAccounts.state == 'loaded'">
            <h1 class="wt-header">
                Practice blocks
                <input v-model="search.tag" type="text" placeholder="Filter tag" />
            </h1>

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
                            </div>

                            <div class="ml-3 flex-grow-1">
                                <div class="progress h-100 h2 mb-0">
                                    <div class="progress-bar bg-success h3 px-1 mw-fit border-right mb-0" :style="{ width: group.countOk / group.count * 100 + '%' }">{{group.countOk}} OK</div>
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
                            <button type="button" class="btn btn-warning">Resize</button>
                        </div>
                    </slot>
                </collapsible>
            </template>
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

    import PracticeAccountList from "./components/PracticeAccountList.vue";

    import { FlatPsbNamedAccount, PsbAccountCharacterStatus, PsbAccountType, PsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";
    import { PsCharacter } from "api/CharacterApi";

    type GroupedAccount = {
        accounts: Loading<FlatPsbNamedAccount[]>;
        tag: string;
        headerText: string;
        count: number;
        countOk: number;
        countUnused: number;
        countMissing: number;
        countDeleted: number;
    };

    export const PsbPractice = Vue.extend({
        props: {

        },

        data: function() {
            return {
                accounts: Loadable.idle() as Loading<FlatPsbNamedAccount[]>,

                search: {
                    tag: "" as string,
                    deleted: false as boolean
                },

                create: {
                    tag: "" as string,
                    count: 0 as number
                }
            }
        },

        created: function(): void {
            document.title = `Honu / PSB / Practice`;

            this.bindData();
        },

        methods: {

            bindData: async function(): Promise<void> {
                this.accounts = Loadable.loading();
                this.accounts = await PsbNamedAccountApi.getByTypeID(PsbAccountType.PRACTICE);
            },

            createBlockWrapper: async function(): Promise<void> {
                if (this.create.tag.trim().length <= 0) {
                    return;
                }
                if (this.create.count <= 0) {
                    return;
                }

                await this.createBlock(this.create.tag, this.create.count);
            },

            createBlock: async function(tag: string, count: number): Promise<void> {
                Toaster.add(`Creating block`, `Creating ${count} practice accounts for ${tag}`, "info");

                let successCount: number = 0;
                let errorCount: number = 0;
                for (let i = 0; i < count; ++i) {
                    const newAccount: Loading<PsbNamedAccount> = await PsbNamedAccountApi.create(tag, `Practice${i + 1}`, PsbAccountType.PRACTICE, true);

                    if (newAccount.state == "loaded") {
                        ++successCount;
                    } else if (newAccount.state == "error") {
                        Toaster.add(`Error creating ${tag}xPractice${i + 1}`, `Error: ${newAccount.message}`, "warning");
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
                    if (account.tag == tag) {
                        await PsbNamedAccountApi.deleteByID(account.id);
                        Toaster.add(`Deleted ${account.id}`, `Successfully deleted ${account.tag}x${account.name}`, "success");
                    }
                }

                await this.bindData();
            },

        },

        computed: {

            groupedAccounts: function(): Loading<GroupedAccount[]> {
                if (this.accounts.state != "loaded") {
                    return Loadable.rewrap(this.accounts);
                }

                const accounts: FlatPsbNamedAccount[] = this.accounts.data;
                const map: Map<string, GroupedAccount> = new Map();

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
                            headerText: "",
                            count: 0,
                            countOk: 0,
                            countUnused: 0,
                            countMissing: 0,
                            countDeleted: 0
                        };
                    }

                    if (arr.accounts.state != "loaded") {
                        throw `expected arr.accounts.state for tag ${tag} to be 'loaded', was ${arr.accounts.state}`;
                    }

                    arr.accounts.data.push(acc);
                    ++arr.count;

                    if (acc.status == PsbAccountCharacterStatus.OK) {
                        ++arr.countOk;
                    } else if (acc.status == PsbAccountCharacterStatus.MISSING) {
                        ++arr.countMissing;
                    } else if (acc.status == PsbAccountCharacterStatus.UNUSED) {
                        ++arr.countUnused;
                    } else if (acc.status == PsbAccountCharacterStatus.DELETED) {
                        ++arr.countDeleted;
                    }

                    map.set(tag, arr);
                }

                map.forEach((group: GroupedAccount, _) => {
                    if (group.accounts.state != "loaded") {
                        throw ``;
                    }
                    group.headerText = `${group.tag} block: ${group.countOk + group.countUnused}/${group.accounts.data.length}: `;

                    if (group.countOk > 0) { group.headerText += `${group.countOk} OK; `; }
                    if (group.countUnused > 0) { group.headerText += `${group.countUnused} Unused; `; }
                    if (group.countMissing > 0) { group.headerText += `${group.countMissing} missing; `; }
                    if (group.countDeleted > 0) { group.headerText += `${group.countDeleted} deleted; `; }
                });

                let blocks: GroupedAccount[] = Array.from(map.values());

                if (this.search.tag.trim().length > 0) {
                    blocks = blocks.filter(iter => iter.tag.indexOf(this.search.tag) > -1);
                }

                blocks = blocks.sort((a, b) => a.tag.localeCompare(b.tag));

                return Loadable.loaded(blocks);
            }

        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
            Collapsible,
            PracticeAccountList
        }
    });
    export default PsbPractice;
</script>