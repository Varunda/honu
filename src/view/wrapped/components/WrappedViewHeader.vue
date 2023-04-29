<template>
    <div>
        <collapsible header-text="Input characters">
            <div>
                This wrapped was generated with {{wrapped.inputCharacterIDs.length}} characters
            </div>

            <template v-for="charID of wrapped.inputCharacterIDs">
                <wrapped-character-button :character-id="charID" :character="wrapped.characters.get(charID)"></wrapped-character-button>
            </template>
        </collapsible>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import Collapsible from "components/Collapsible.vue";
    import { PsCharacter } from "api/CharacterApi";

    const WrappedCharacterButton = Vue.extend({
        props: {
            CharacterId: { type: String, required: true },
            character: { type: Object as PropType<PsCharacter | null> }
        },

        template: `
            <button class="btn btn-primary mr-2">
                <span v-if="character == null">
                    &lt;missing {{CharacterId}}
                </span>

                <span v-else>
                    <span v-if="character.outfitID != null">
                        [{{character.outfitTag}}]
                    </span>

                    {{character.name}}
                </span>
            </button>
        `
    });

    export const WrappedViewHeader = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        components: {
            Collapsible,
            WrappedCharacterButton
        }

    });
    export default WrappedViewHeader;
</script>