﻿<template>
    <div style="display: grid; grid-template-rows: min-content 1fr; grid-template-columns: min-content 1fr;">
        <h1 style="grid-column-start: 1; grid-row-start: 1;">
            <span v-if="HonuData.state == 'nocontent'">
                <span style="color: red;" title="Offline">
                    ●
                </span>
            </span>

            <span v-else-if="HonuData.state != 'loaded'">
                <span style="color: grey;" title="Unknown if online or offline">
                    ●
                </span>
            </span>

            <span v-else-if="HonuData.state == 'loaded' && HonuData.data.online == true">
                <span style="color: green;" title="Online">
                    ●
                </span>
            </span>

            <span v-else-if="HonuData.state == 'loaded' && HonuData.data.online == false">
                <span style="color: red;" title="Offline">
                    ●
                </span>
            </span>
        </h1>

        <div style="grid-column-start: 2; grid-row-start: 1">
            <h1 class="d-inline">
                <b>
                    <span v-if="character.outfitID != null" title="character.outfitName">
                        [{{character.outfitTag}}]
                    </span>
                    {{character.name}}
                </b>
            </h1>

            <h3 class="d-inline" style="align-self: center;">
                of

                {{character.worldID | world}}'s

                {{character.factionID | factionLong}}
            </h3>

            <template v-if="worldChanges.state == 'loaded' && worldChanges.data.length > 0">
                <h5 class="d-inline text-muted">
                    <b>&middot;</b> Fought on
                </h5>
                <h5 v-for="change in worldChanges.data" class="d-inline join-dashes text-muted">
                    {{change.worldID | world}} until {{change.timestamp | momentNoTz("YYYY-MM-DD")}}
                </h5>
            </template>
        </div>

        <div v-if="HonuData.state == 'loaded'" style="grid-row-start: 2; grid-column-start: 2;">
            <h4>
                <span v-if="HonuData.data.online == true">
                    Online: 

                    <span v-if="HonuData.data.zoneID != 0">
                        last seen on {{HonuData.data.zoneID | zone}}
                    </span>
                    <span v-else>
                        not seen on a continent yet
                    </span>
                    
                    <span v-if="HonuData.data.factionID == 4">
                        fighting for {{HonuData.data.teamID | faction}}
                    </span>

                    {{HonuData.data.latestEventTimestamp | timeAgo}} ago ({{HonuData.data.latestEventTimestamp | moment}})
                </span>

                <a v-if="HonuData.data.sessionID != null" :href="'/s/' + HonuData.data.sessionID">
                    Latest session
                </a>
            </h4>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import { PsCharacter, HonuCharacterData } from "api/CharacterApi";
    import { CharacterWorldChange, CharacterWorldChangeApi } from "api/CharacterWorldChangeApi";

    import Collapsible from "components/Collapsible.vue";

    import "MomentFilter";
    import "filters/TimeAgoFilter";
    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";
    import "filters/ZoneNameFilter";

    export const CharacterHeader = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true },
            HonuData: { type: Object as PropType<Loading<HonuCharacterData>>, required: true }
        },

        data: function() {
            return {
                worldChanges: Loadable.idle() as Loading<CharacterWorldChange[]>
            }
        },

        mounted: function(): void {
            this.loadWorldChanges();
        },

        methods: {
            loadWorldChanges: async function(): Promise<void> {
                this.worldChanges = Loadable.loading();
                this.worldChanges = await CharacterWorldChangeApi.getByCharacterID(this.character.id);
            }
        },

        watch: {
            character: function(): void {
                this.loadWorldChanges();
            }
        },

        components: {
            Collapsible
        }

    });
    export default CharacterHeader;
</script>