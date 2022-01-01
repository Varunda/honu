<template>
    <div class="border-top">
        <h3 data-toggle="collapse" :data-target="'#' + collapseID" style="cursor: pointer;" class="mt-2">
            <span v-if="tree.tree != null" class="d-flex">
                <span class="h-100">
                    <span v-for="count in tree.tiers.length">
                        <span v-if="count < tree.entry.currentTier" class="text-success h-100">
                            &block;
                        </span>
                        <span v-else-if="count == tree.entry.currentTier" class="text-info h-100">
                            &block;
                        </span>
                        <span v-else class="text-muted h-100">
                            &block;
                        </span>
                    </span>
                </span>

                <span class="flex-grow-1">
                    <img style="margin: -32px; margin-top: -48px;" :src="'https://census.daybreakgames.com/files/ps2/images/static/' + tree.tree.imageID + '.png'"/>
                    {{tree.tree.name}}

                    <span v-if="showDebug">
                        {{tree.tree.id}}
                    </span>
                </span>

                <span v-if="tree.entry.completionDate != null" class="mr-2">
                    {{tree.entry.completionDate | moment("YYYY-MM-DD")}}
                    ({{tree.entry.completionDate | timeAgo}})
                </span>

                <span>
                    <span v-for="count in highestTierCompletionCount">
                        <span v-if="count - 1 < highestTierCompletedCount" class="text-success">
                            &block;
                        </span>
                        <span v-else-if="count - 1 < (highestTierCompletedCount + highestTierInProgressCount)" class="text-warning">
                            &block;
                        </span>
                        <span v-else class="text-muted">
                            &block;
                        </span>
                    </span>
                </span>
            </span>
            <span v-else>
                &lt;missing tree&gt;
            </span>
        </h3>

        <div class="collapse" :id="collapseID">
            <div class="btn-group w-100 border-bottom pb-2">
                <button v-for="tier in tree.tiers" @click="setSelectedTier(tier.tierID)" class="btn border" :class="[ selectedTier == tier.tierID ? 'btn-primary' : 'btn-secondary' ]">
                    {{tier.tier.name}}

                    <span v-if="showDebug">
                        {{tier.tier.id}}
                    </span>
                </button>
            </div>

            <character-directive-tier v-for="tier in tiers"
                :key="'tier-' + tree.tree.id + '-' + tier.tierID" :tier="tier" :show="tier.tierID == selectedTier">
            </character-directive-tier>

            <hr />
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { ExpandedCharacterDirectiveTier, ExpandedCharacterDirectiveTree } from "api/CharacterDirectiveApi";

    import CharacterDirectiveTier from "./CharacterDirectiveTier.vue";

    export const CharacterDirectiveTree = Vue.extend({
        props: {
            tree: { type: Object as PropType<ExpandedCharacterDirectiveTree>, required: true }
        },

        data: function() {
            return {
                selectedTier: 0 as number
            }
        },

        mounted: function() {
            this.selectedTier = this.tree.entry.currentTier;

            if (this.selectedTier > this.tree.tiers.length) {
                this.selectedTier = this.tree.tiers.length;
            }
        },

        methods: {
            setSelectedTier: function(tierID: number): void {
                this.selectedTier = tierID;
            }
        },

        computed: {
            collapseID: function(): string {
                return `character-directive-tree-` + this.tree.entry.treeID;
            },

            showDebug: function(): boolean {
                return (this.$root as any).debug.directive;
            },

            tiers: function(): ExpandedCharacterDirectiveTier[] {
                return [...this.tree.tiers].sort((a, b) => b.tierID - a.tierID);
            },

            highestTier: function(): ExpandedCharacterDirectiveTier | null {
                if (this.tree.entry.currentTier > this.tiers.length) {
                    return this.tiers[0];
                }
                return this.tree.tiers.find(iter => iter.tierID == this.tree.entry.currentTier) || null;
            },

            highestTierDirectivePoints: function(): number {
                return this.highestTier?.tier?.directivePoints ?? 0;
            },

            highestTierCompletionCount: function(): number {
                return this.highestTier?.tier?.completionCount ?? 0;
            },

            highestTierCompletedCount: function(): number {
                if (this.highestTier == null) {
                    return 0;
                }

                return this.highestTier.directives.filter(iter => iter.entry != null && iter.entry.completionDate != null).length;
            },

            highestTierInProgressCount: function(): number {
                if (this.highestTier == null) {
                    return 0;
                }

                return this.highestTier.directives.filter(iter => iter.entry != null && iter.entry.completionDate == null).length;
            }
        },

        components: {
            CharacterDirectiveTier
        }
    });
    export default CharacterDirectiveTree;

</script>