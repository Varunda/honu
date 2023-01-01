<template>
        <a-table
            :entries="accounts"
            :show-filters="true"
            :paginate="false"
            default-sort-field="name" default-sort-order="asc"
            display-type="table">

            <a-col>
                <a-header>
                    <b>View</b>
                </a-header>

                <a-body v-slot="entry">
                    <!--<a href="#" @click="viewAccount(entry.id)" :class="{ 'text-danger': entry.account.deletedByID != null }">-->
                    <a href="#" :class="{ 'text-danger': entry.account.deletedByID != null }">
                        View {{entry.id}}
                    </a>
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
                    <psb-account-character-cell :id="entry.vsID" :status="entry.account.vsStatus"
                        :character="entry.vsCharacter" faction-id="1">
                    </psb-account-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="vsLastLogin">
                <a-header>
                    <b>VS last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-login :character="entry.vsCharacter"></psb-account-character-login>
                </a-body>
            </a-col>

            <a-col sort-field="ncName">
                <a-header>
                    <b>NC</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-cell :id="entry.ncID" :status="entry.account.ncStatus"
                        :character="entry.ncCharacter" faction-id="2">
                    </psb-account-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="ncLastLogin">
                <a-header>
                    <b>NC last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-login :character="entry.ncCharacter"></psb-account-character-login>
                </a-body>
            </a-col>

            <a-col sort-field="trName">
                <a-header>
                    <b>TR</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-cell :id="entry.trID" :status="entry.account.trStatus"
                        :character="entry.trCharacter" faction-id="3">
                    </psb-account-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="trLastLogin">
                <a-header>
                    <b>TR last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-login :character="entry.trCharacter"></psb-account-character-login>
                </a-body>
            </a-col>

            <a-col sort-field="nsName">
                <a-header>
                    <b>NS</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-cell :id="entry.nsID" :status="entry.account.nsStatus"
                        :character="entry.nsCharacter" faction-id="4">
                    </psb-account-character-cell>
                </a-body>
            </a-col>

            <a-col sort-field="nsLastLogin">
                <a-header>
                    <b>NS last used</b>
                </a-header>

                <a-body v-slot="entry">
                    <psb-account-character-login :character="entry.nsCharacter"></psb-account-character-login>
                </a-body>
            </a-col>
        </a-table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";

    import "MomentFilter";
    import "filters/CharacterName";
    import "filters/TimeAgoFilter";
    import "filters/FactionNameFilter";

    import { Loading, Loadable } from "Loading";
    import { FlatPsbNamedAccount, PsbNamedAccountApi } from "api/PsbNamedAccountApi";
    import { PsCharacter } from "api/CharacterApi";

    const PsbAccountCharacterCell = Vue.extend({
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

    const PsbAccountCharacterLogin = Vue.extend({
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

    export const PracticeAccountList = Vue.extend({
        props: {
            accounts: { type: Object as PropType<Loading<FlatPsbNamedAccount[]>>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
            InfoHover,
            PsbAccountCharacterCell, PsbAccountCharacterLogin
        }
    });
    export default PracticeAccountList;
</script>