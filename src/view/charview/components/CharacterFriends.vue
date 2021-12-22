<template>
    <div>
        <hr class="border" />

        <div class="d-flex">
            <a-table
                :entries="friends"
                :show-filters="true"
                display-type="table">

                <a-col sort-field="friendName">
                    <a-header>
                        <b>Character</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <a :href="'/c/' + entry.friendID">
                            <span v-if="entry.friendName == null">
                                &lt;missing {{entry.friendID}}&gt;

                                <info-hover text="This character has yet to be found by Honu. You might be able to refresh">
                                </info-hover>
                            </span>
                            <span v-else>
                                <span v-if="entry.friendOutfitID != null && entry.friendOutfitName != null">
                                    [{{entry.friendOutfitTag}}]
                                </span>
                                {{entry.friendName}}
                            </span>
                        </a>
                    </a-body>
                </a-col>

                <a-col sort-field="friendFactionID">
                    <a-header>
                        <b>Faction</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.friendFactionID == null">
                            --
                        </span>
                        <span v-else>
                            {{entry.friendFactionID | faction}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="friendWorldID">
                    <a-header>
                        <b>Server</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.friendWorldID == null">
                            --
                        </span>
                        <span v-else>
                            {{entry.friendWorldID | world}}
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="friendDateLastLogin">
                    <a-header>
                        <b>Last login</b>
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.friendDateLastLogin == null">
                            --
                        </span>
                        <span v-else>
                            {{entry.friendDateLastLogin | moment}}
                            ({{entry.friendDateLastLogin | timeAgo}})
                        </span>
                    </a-body>
                </a-col>
            </a-table>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "filters/LocaleFilter";
    import "filters/FixedFilter";
    import "filters/FactionNameFilter";
    import "filters/WorldNameFilter";
    import "filters/TimeAgoFilter";
    import "MomentFilter";

    import InfoHover from "components/InfoHover.vue";
    import { ATable, ACol, AHeader, AFilter, ABody } from "components/ATable";

    import { PsCharacter } from "api/CharacterApi";
    import { FlatExpandedCharacterFriend, CharacterFriendApi } from "api/CharacterFriendApi";

    import Busy from "components/Busy.vue";

    const NotFilled = Vue.extend({
        template: `
            <span>
                !!
                <info-hover text="This character has yet to be found by Honu. You might be able to refresh">
                </info-hover>
            </span>
        `,

        components: {
            InfoHover
        }
    });

    export const CharacterOverview = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                friends: Loadable.idle() as Loading<FlatExpandedCharacterFriend[]>,
            }
        },

        mounted: function(): void {
            this.loadFriends();
        },

        methods: {
            loadFriends: async function(): Promise<void> {
                this.friends = Loadable.loading();
                this.friends = await CharacterFriendApi.getByCharacterID(this.character.id);
            }
        },

        components: {
            Busy,
            InfoHover,
            ATable,
            ACol,
            AHeader,
            AFilter,
            ABody,
            NotFilled
        }

    });
    export default CharacterOverview;
</script>
