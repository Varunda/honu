<template>
    <div>
        <hr class="border" />

        <div>
            <div v-if="set.state == 'idle'"></div>
            
            <div v-else-if="set.state == 'loading'">
                Loading... <busy></busy>
            </div>

            <div v-else-if="set.state == 'error'">
                Error loading directives: {{set.message}}
            </div>

            <div v-else-if="set.state == 'loaded'">
                <character-directive-category v-for="category in set.data.categories" 
                    :key="category.categoryID" :category="category">
                </character-directive-category>
            </div>

            <div v-else>
                Unchecked state of set: '{{set.state}}'
            </div>
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
    import Busy from "components/Busy.vue";
    import CharacterDirectiveCategory from "./directives/CharacterDirectiveCategory.vue";

    import { PsCharacter } from "api/CharacterApi";
    import { CharacterDirectiveSet, CharacterDirectiveApi } from "api/CharacterDirectiveApi";

    export const CharacterDirectives = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                set: Loadable.idle() as Loading<CharacterDirectiveSet>
            }
        },

        mounted: function(): void {
            this.load();
        },

        methods: {
            load: async function(): Promise<void> {
                this.set = Loadable.loading();
                this.set = await CharacterDirectiveApi.getByCharacterID(this.character.id);
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
            CharacterDirectiveCategory
        }

    });
    export default CharacterDirectives;
</script>
