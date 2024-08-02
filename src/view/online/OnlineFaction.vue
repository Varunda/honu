<template>
    <div>
        <a-table :entries="online"
            :show-filters="true"
            default-sort-field="name" default-sort-order="asc"
            :page-sizes="[50, 100, 200, 500]" :default-page-size="200">

            <a-col sort-field="name">
                <a-header>
                    <b>Name</b>
                </a-header>

                <a-filter method="input" field="display" type="string"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <a :href="'/c/' + entry.characterID">
                        <span v-if="entry.name == null">
                            &lt;missing {{entry.characterID}}&gt
                        </span>
                        <span v-else>
                            {{entry.display}}
                        </span>
                    </a>
                </a-body>
            </a-col>

            <a-col sort-field="battleRankValue">
                <a-header>
                    <b>Battle rank</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.battleRank}}~{{entry.prestige}}
                </a-body>
            </a-col>

            <a-col sort-field="sessionDuration">
                <a-header>
                    Session duration
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.lastLogin == null">
                        --
                    </span>

                    <span v-else>
                        {{entry.sessionDuration | duration}}
                    </span>
                </a-body>
            </a-col>

        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import "filters/FactionNameFilter";
    import "filters/DurationFilter";
    import { CharacterApi, FlatOnlinePlayer, OnlinePlayer } from "api/CharacterApi";

    export const OnlineFaction = Vue.extend({
        props: {
            data: { type: Array as PropType<FlatOnlinePlayer[]>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            online: function(): Loading<FlatOnlinePlayer[]> {
                return Loadable.loaded(this.data);
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter
        }
    });
    export default OnlineFaction;
</script>