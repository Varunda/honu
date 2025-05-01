<template>
    <div>
        <a-table :entries="online"
            :show-filters="true"
            default-sort-field="name" default-sort-order="asc"
            :page-sizes="[50, 100, 200, 500, 1000]" :default-page-size="200">

            <a-col sort-field="name">
                <a-header>
                    <b>Name</b>
                </a-header>

                <a-filter method="input" field="display" type="string"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <a :href="'/c/' + entry.characterID">
                        <faction :faction-id="entry.factionID" style="width: 24px;"></faction>

                        <span v-if="entry.name == null">
                            &lt;missing {{entry.characterID}}&gt
                        </span>
                        <span v-else>
                            {{entry.display}}
                        </span>
                    </a>
                </a-body>
            </a-col>

            <a-col v-if="ShowFaction">
                <a-header>
                    <b>Faction</b>
                </a-header>

                <a-filter field="factionID" type="number" method="dropdown" :source="sources.factions" source-key="key" source-value="value" :sort-values="false"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.factionID | faction}}
                </a-body>
            </a-col>

            <a-col v-if="ShowWorld">
                <a-header>
                    <b>Server</b>
                </a-header>

                <a-filter field="worldID" type="number" method="dropdown" :source="sources.worlds" source-key="key" source-value="value" :sort-values="false"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.worldID | world}}
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

            <a-col>
                <a-header>
                    <b>Continent</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.zoneID == 0">
                        unknown
                        <info-hover text="This character has done no actions that tell Honu where they are"></info-hover>
                    </span>
                    <span v-else>
                        {{entry.zoneID | zone}}
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="sessionDuration">
                <a-header>
                    Session (duration)
                </a-header>

                <a-body v-slot="entry">
                    <a :href="'/s/' + entry.sessionID" target="_blank">
                        view

                        <span v-if="entry.lastLogin == null">
                            <info-hover text="Honu does not know when this character last came online. This can occur when Honu restarts"></info-hover>
                        </span>

                        <span v-else>
                            ({{entry.sessionDuration / 1000 | mduration}})
                        </span>
                    </a>
                </a-body>
            </a-col>

        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";
    import InfoHover from "components/InfoHover.vue";
    import Faction from "components/FactionImage";

    import WorldUtils from "util/World";

    import { CharacterApi, FlatOnlinePlayer, OnlinePlayer } from "api/CharacterApi";

    import "filters/FactionNameFilter";
    import "filters/DurationFilter";
    import "filters/ZoneNameFilter";
    import "filters/WorldNameFilter";
    import "MomentFilter";

    export const OnlineFaction = Vue.extend({
        props: {
            data: { type: Array as PropType<FlatOnlinePlayer[]>, required: true },
            ShowFaction: { type: Boolean, required: false, default: false },
            ShowWorld: { type: Boolean, required: false, default: false }
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
            },

            sources: function() {
                return {
                    factions: [
                        { key: "All", value: null },
                        { key: "VS", value: 1 },
                        { key: "NC", value: 2 },
                        { key: "TR", value: 3 },
                        { key: "NS", value: 4 },
                    ],

                    worlds: [
                        { key: "All", value: null },
                        { key: "Osprey (US)", value: WorldUtils.Osprey },
                        { key: "Jaeger", value: WorldUtils.Jaeger },
                        { key: "Wainwright (EU)", value: WorldUtils.Wainwright },
                        { key: "SolTech", value: WorldUtils.SolTech },
                    ]
                }
            }
        },

        components: {
            ATable, ACol, AHeader, ABody, AFilter,
            InfoHover, Faction
        }
    });
    export default OnlineFaction;
</script>