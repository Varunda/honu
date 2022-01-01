<template>
    <div class="border-bottom border-left border-right mb-3">
        <h2 class="wt-header mb-0 border-bottom-0" data-toggle="collapse" :data-target="'#' + collapseID">
            <span v-if="category.category != null">
                {{category.category.name}}
            </span>
            <span v-else>
                &lt;missing category {{category.categoryID}}&gt;
            </span>

            <span v-if="showDebug == true">
                {{category.categoryID}}
            </span>
        </h2>

        <div class="collapse" :id="collapseID">
            <character-directive-tree v-for="tree in category.trees"
                :key="'tree-' + category.categoryID + '-' + tree.entry.treeID" :tree="tree"
                class="mx-2">
            </character-directive-tree>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import CharacterDirectiveTree from "./CharacterDirectiveTree.vue";

    import { ExpandedCharacterDirectiveCategory } from "api/CharacterDirectiveApi";

    export const CharacterDirectiveCategory = Vue.extend({
        props: {
            category: { type: Object as PropType<ExpandedCharacterDirectiveCategory>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            collapseID: function(): string {
                return `character-directive-category-` + this.category.categoryID;
            },

            showDebug: function(): boolean {
                return (this.$root as any).debug.directive;
            }
        },

        components: {
            CharacterDirectiveTree
        }
    });
    export default CharacterDirectiveCategory;
</script>