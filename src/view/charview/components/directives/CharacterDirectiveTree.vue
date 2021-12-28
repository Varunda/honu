<template>
    <div>
        <h3 data-toggle="collapse" :data-target="'#' + collapseID">
            <span v-if="tree.tree != null">
                <img style="margin: -32px;" :src="'https://census.daybreakgames.com/files/ps2/images/static/' + tree.tree.imageID + '.png'"/>
                {{tree.tree.name}}
                <span v-if="tree.entry.completionDate != null" class="border-left ml-2 pl-2">
                    {{tree.entry.completionDate | moment}}
                </span>

                <span v-if="currentTier != null">
                    - 
                    <!-- <img :src="'https://census.daybreakgames.com/files/ps2/images/static/' + currentTier.tier.imageID + '.png'" /> -->
                    {{currentTier.tier.name}}
                </span>
            </span>
            <span v-else>
                &lt;missing tree&gt;
            </span>
        </h3>

        <div class="collapse" :id="collapseID">
            <character-directive-tier v-for="tier in tiers"
                :key="'tier-' + tree.tree.id + '-' + tier.tierID" :tier="tier" :tree="tree">
            </character-directive-tier>
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

            }
        },

        methods: {

        },

        computed: {
            collapseID: function(): string {
                return `character-directive-tree-` + this.tree.entry.treeID;
            },

            tiers: function(): ExpandedCharacterDirectiveTier[] {
                return [...this.tree.tiers].sort((a, b) => b.tierID - a.tierID);
            },

            currentTier: function(): ExpandedCharacterDirectiveTier | null {
                return this.tree.tiers.find(iter => iter.tierID == this.tree.entry.currentTier) || null;
            }
        },

        components: {
            CharacterDirectiveTier
        }
    });
    export default CharacterDirectiveTree;

</script>