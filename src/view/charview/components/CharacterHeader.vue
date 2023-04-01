<template>
    <div style="display: grid; grid-template-rows: min-content 1fr; grid-template-columns: min-content 1fr;">
        <h1 style="grid-column-start: 1; grid-row-start: 1;">
            <span v-if="HonuData.state != 'loaded'">
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
        </div>

        <div v-if="HonuData.state == 'loaded'" style="grid-row-start: 2; grid-column-start: 2;">
            <h4>
                <span v-if="HonuData.data.online == true">
                    Online: last seen on {{HonuData.data.zoneID | zone}}
                    <span v-if="HonuData.data.factionID == 4">
                        fighting for {{HonuData.data.teamID | faction}}
                    </span>
                    {{HonuData.data.latestEventTimestamp | timeAgo}} ago ({{HonuData.data.latestEventTimestamp | moment}}).
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

            }
        },

        methods: {

 
        },

        components: {
            Collapsible
        }

    });
    export default CharacterHeader;
</script>