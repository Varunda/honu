<template>

</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsCharacter } from "api/CharacterApi";

    import "MomentFilter";
    import "filters/CharacterName";
    import "filters/TimeAgoFilter";
    import "filters/FactionNameFilter";

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

    export const PsbAccountList = Vue.extend({
        props: {

        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        components: {

        }
    });
</script>