<template>
    <div>
        <div class="d-flex align-items-center mb-2">
            <h1 class="d-inline-block flex-grow-1">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />

                <a href="/" title="Return to home page">Honu</a>

                <span>/ PSB named</span>
            </h1>
        </div>

        <div class="mb-3">
            <h4>Filters</h4>

            <div>
                <button type="button" class="btn" :class="[ filter.missingCharacter ? 'btn-primary' : 'btn-secondary' ]" @click="filter.missingCharacters = !filter.missingCharacters">
                    <span v-if="filter.missingCharacter">Y</span>
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

        </div>

        <a-table
            :entries="filtered"
            :show-filters="true"
            default-sort-field="tag" default-sort-order="asc"
            display-type="table">

            <a-col>
                <a-header></a-header>

                <a-body v-slot="entry">
                    <div @click="viewAccount(entry.id)">
                        View
                    </div>
                </a-body>
            </a-col>

            <a-col sort-field="tag">
                <a-header>
                    <b>Tag</b>
                </a-header>

                <a-filter method="input" type="string" field="tag"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.tag}}
                </a-body>
            </a-col>

            <a-col sort-field="name">
                <a-header>
                    <b>Name</b>
                </a-header>

                <a-filter method="input" type="string" field="name"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.name}}
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    Status
                </a-header>

                <a-filter method="dropdown" type="string" field="status"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <span v-if="entry.status == 'Missing'" class="text-danger">
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

            <a-col sort-field="lastUsed">
                <a-header>
                    <b>Last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.lastUsed != null" :title="entry.lastUsed | moment">
                        {{entry.lastUsed | timeAgo}}
                    </span>
                    <span v-else class="text-danger">
                        --
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    VS
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.vsID == null" class="text-danger">
                        &lt;missing VS character&gt;
                    </span>

                    <span v-else-if="entry.vsCharacter == null">
                        <a :href="'/c/' + entry.vsID">
                            &lt;missing {{entry.vsID}}&gt;
                        </a>
                    </span>

                    <span v-if="entry.vsCharacter != null">
                        <a v-if="entry.vsOutfitID != null" :href="'/o/' + entry.vsOutfitID" :title="entry.vsOutfitName">
                            [{{entry.vsOutfitTag}}]
                        </a>

                        <a :href="'/c/' + entry.vsID">
                            {{entry.vsName}}
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    VS last used
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.vsID == null"></span>

                    <span v-else-if="entry.vsLastLogin == null">
                        &lt;never signed in&gt;
                    </span>

                    <span v-else-if="entry.vsLastLogin != null" :title="entry.vsLastLogin | moment">
                        {{entry.vsLastLogin | timeAgo}}
                    </span>
                    <span v-else class="text-danger">
                        --
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    NC
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.ncID == null" class="text-danger">
                        &lt;missing NC character&gt;
                    </span>

                    <span v-else-if="entry.ncCharacter == null">
                        <a :href="'/c/' + entry.ncID">
                            &lt;missing ID {{entry.ncID}}&gt;
                        </a>
                    </span>

                    <span v-if="entry.ncCharacter != null">
                        <a v-if="entry.ncOutfitID != null" :href="'/o/' + entry.ncOutfitID" :title="entry.ncOutfitName">
                            [{{entry.ncOutfitTag}}]
                        </a>

                        <a :href="'/c/' + entry.ncID">
                            {{entry.ncName}}
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    NC last used
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.ncID == null"></span>

                    <span v-else-if="entry.ncLastLogin == null">
                        &lt;never signed in&gt;
                    </span>

                    <span v-else-if="entry.ncLastLogin != null" :title="entry.ncLastLogin | moment">
                        {{entry.ncLastLogin | timeAgo}}
                    </span>
                    <span v-else class="text-danger">
                        --
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    TR
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.trID == null" class="text-danger">
                        &lt;missing TR character&gt;
                    </span>

                    <span v-else-if="entry.trCharacter == null">
                        <a :href="'/c/' + entry.trID">
                            &lt;missing ID {{entry.trID}}&gt;
                        </a>
                    </span>

                    <span v-if="entry.trCharacter != null">
                        <a v-if="entry.trOutfitID != null" :href="'/o/' + entry.trOutfitID" :title="entry.trOutfitName">
                            [{{entry.trOutfitTag}}]
                        </a>

                        <a :href="'/c/' + entry.trID">
                            {{entry.trName}}
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    TR last used
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.trID == null"></span>

                    <span v-else-if="entry.trLastLogin == null">
                        &lt;never signed in&gt;
                    </span>

                    <span v-else-if="entry.trLastLogin != null" :title="entry.trLastLogin | moment">
                        {{entry.trLastLogin | timeAgo}}
                    </span>
                    <span v-else class="text-danger">
                        --
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    NS
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.nsID == null" class="text-danger">
                        &lt;missing NS character&gt;
                    </span>

                    <span v-else-if="entry.nsCharacter == null">
                        <a :href="'/c/' + entry.nsID">
                            &lt;missing {{entry.nsID}}&gt;
                        </a>
                    </span>

                    <span v-if="entry.nsCharacter != null">
                        <a v-if="entry.nsOutfitID != null" :href="'/o/' + entry.nsOutfitID" :title="entry.nsOutfitName">
                            [{{entry.nsOutfitTag}}]
                        </a>

                        <a :href="'/c/' + entry.nsID">
                            {{entry.nsName}}
                        </a>
                    </span>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    NS last used
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.nsID == null"></span>

                    <span v-else-if="entry.nsLastLogin == null">
                        &lt;never signed in&gt;
                    </span>

                    <span v-else-if="entry.nsLastLogin != null" :title="entry.nsLastLogin | moment">
                        {{entry.nsLastLogin | timeAgo}}
                    </span>
                    <span v-else class="text-danger">
                        --
                    </span>
                </a-body>
            </a-col>
        </a-table>

        <div class="modal" id="psb-account-modal">
            <psb-named-account-modal v-if="view.opened == true" :account="view.account">
            </psb-named-account-modal>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import PsbNamedAccountModal from "./components/PsbNamedAccountModal.vue";

    import "MomentFilter";
    import "filters/CharacterName";
    import "filters/TimeAgoFilter";

    import { FlatPsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";

    export const PsbNamed = Vue.extend({
        props: {

        },

        data: function() {
            return {
                accounts: Loadable.idle() as Loading<FlatPsbNamedAccount[]>,
                wrapped: Loadable.idle() as Loading<FlatPsbNamedAccount[]>,

                filter: {
                    missingCharacters: false as boolean,
                    mismatchFactions: false as boolean,
                    wrongWorlds: false as boolean
                },

                view: {
                    opened: false as boolean,
                    account: null as FlatPsbNamedAccount | null
                }
            }
        },

        created: function(): void {
            document.title = `Honu / PSB named`;
        },

        mounted: function(): void {
            this.loadAll();

            this.$nextTick(() => {
                $("#psb-account-modal").on("hide.bs.modal", () => {
                    this.view.opened = false;
                    this.view.account = null;
                });
            });
        },

        methods: {
            viewAccount: function(accID: number): void {
                if (this.accounts.state != "loaded") {
                    return console.warn(`accounts.state is not loaded, cannot load ${accID}`);
                }

                const acc: FlatPsbNamedAccount | undefined = this.accounts.data.find(iter => iter.id == accID);
                if (acc == undefined) {
                    return console.warn(`failed to find ${accID}`);
                }

                this.view.account = acc;
                this.view.opened = true;

                this.$nextTick(() => {
                    this.openModal();
                });
            },

            openModal: function(): void {
                $("#psb-account-modal").modal("show");
            },

            loadAll: async function(): Promise<void> {
                this.accounts = Loadable.loading();
                this.accounts = await PsbNamedAccountApi.getAll();

                if (this.accounts.state == "loaded") {
                    this.updateFilters();
                }
            },

            updateFilters: function(): void {
                console.log(`updating filters`);

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

                this.wrapped = Loadable.loaded(data);
            },
        },

        watch: {
            "filter.missingCharacters": function(): void {
                console.log(`filter.missingCharacters changed`);
                this.updateFilters();
            },

            "filter.mismatchFactions": function(): void {
                console.log(`filter.mismatchFactions changed`);
                this.updateFilters();
            },

            "filter.wrongWorlds": function(): void {
                console.log(`filter.wrongWorlds changed`);
                this.updateFilters();
            }
        },

        computed: {
            filtered: function(): Loading<FlatPsbNamedAccount[]> {
                if (this.filter.missingCharacters == true || this.filter.mismatchFactions || this.filter.wrongWorlds) {
                    return this.wrapped;
                }

                return this.accounts;
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            PsbNamedAccountModal
        }

    });
    export default PsbNamed;
</script>