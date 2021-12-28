<template>
    <div>
        <div v-if="show" class="d-flex" style="flex-wrap: wrap;">
            <character-directive-entry v-for="directive in tier.directives"
                :key="'dir' + directive.directive.id" :entry="directive">
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
            tree: { type: Object as PropType<ExpandedCharacterDirectiveTree>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            collapseID: function(): string {
                return `character-directive-tier-` + this.tier.tierID;
            },

            show: function(): boolean {
                if (this.tier.tierID == this.tree.entry.currentTier) {
                    return true;
                }

                const highestTierID: number = Math.max(...this.tree.tiers.map(iter => iter.tierID));

                return this.tier.tierID == highestTierID;
            }
        },

        components: {
            CharacterDirectiveEntry
        }
    });
    export default CharacterDirectiveTier;

</script>