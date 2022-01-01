<template>
    <div>
        <div v-if="show" class="d-flex" style="flex-wrap: wrap;">
            <character-directive-entry v-for="directive in tier.directives"
                :key="'dir-' + directive.directive.id" :entry="directive">
            </character-directive-entry>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { ExpandedCharacterDirectiveTier, ExpandedCharacterDirectiveTree } from "api/CharacterDirectiveApi";

    import CharacterDirectiveEntry from "./CharacterDirectiveEntry.vue";

    export const CharacterDirectiveTier = Vue.extend({
        props: {
            tier: { type: Object as PropType<ExpandedCharacterDirectiveTier>, required: true },
            show: { type: Boolean, required: true }
        },

        data: function() {
            return {

            }
        },

        mounted: function(): void {

        },

        methods: {

        },

        computed: {
            collapseID: function(): string {
                return `character-directive-tier-` + this.tier.tierID;
            },

            showDebug: function(): boolean {
                return (this.$root as any).debug.directive;
            },

        },

        components: {
            CharacterDirectiveEntry
        }
    });
    export default CharacterDirectiveTier;

</script>