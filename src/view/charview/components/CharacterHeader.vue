<template>
    <div>
        <h1 class="d-inline">
            <b>
                <span v-if="character.outfitID != null" title="character.outfitName">
                    [{{character.outfitTag}}]
                </span>
                {{character.name}}
            </b>
        </h1>

        <h3 class="d-inline">
            of

            {{character.worldID | world}}'s

            {{character.factionID | factionLong}}
        </h3>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsCharacter } from "api/CharacterApi";

    import "filters/WorldNameFilter";
    import "filters/FactionNameFilter";

    export const CharacterHeader = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        }

    });
    export default CharacterHeader;
</script>