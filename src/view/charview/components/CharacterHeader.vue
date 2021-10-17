<template>
    <div>
        <h1 class="d-inline">
            <span v-if="character.outfitID != null" title="character.outfitName">
                [{{character.outfitTag}}]
            </span>
            {{character.name}}
        </h1>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsCharacter } from "api/CharacterApi";

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