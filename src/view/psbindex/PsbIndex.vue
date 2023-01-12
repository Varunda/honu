<template>
    <div>
        <honu-menu class="flex-grow-1">
            <menu-dropdown></menu-dropdown>

            <menu-sep></menu-sep>

            <li class="nav-item h1 p-0">
                <a href="/psb">PSB</a>
            </li>
        </honu-menu>

        <h2>Planetside Battles Index</h2>

        <h3>Logged in</h3>
        <div>
            <div v-if="currentAccount.state == 'idle'"></div>

            <div v-else-if="currentAccount.state == 'loading'">
                Loading...
            </div>

            <div v-else-if="currentAccount.state == 'nocontent'">
                Not logged in
            </div>

            <div v-else-if="currentAccount.state == 'loaded'">
                Current user: {{currentAccount.data.name}}
            </div>
        </div>

        <h3>Links</h3>

        <div>
            <a href="/psb/named">Named accounts</a>
        </div>

        <div>
            <a href="/psb/practice">Practice accounts</a>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Loadable, Loading } from "Loading";

    import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";

    import { HonuAccount, HonuAccountApi } from "api/HonuAccountApi";

    export const PsbIndex = Vue.extend({
        props: {

        },

        data: function() {
            return {
                currentAccount: Loadable.idle() as Loading<HonuAccount>
            }
        },

        created: function(): void {
            this.bindCurrentUser();
        },

        methods: {
            bindCurrentUser: async function(): Promise<void> {
                this.currentAccount = Loadable.loading();
                this.currentAccount = await HonuAccountApi.getMe();
            }
        },

        components: {
            HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
        }
    });
    export default PsbIndex;
</script>
