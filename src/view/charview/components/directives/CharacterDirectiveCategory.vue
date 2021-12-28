<template>
    <div>
        <h2 class="wt-header" data-toggle="collapse" :data-target="'#' + collapseID">
            <span v-if="category.category != null">
                {{category.category.name}}
            </span>
            <span v-else>
                &lt;missing category {{category.categoryID}}&gt;
            </span>
        </h2>

        <div class="collapse" :id="collapseID">
            <character-directive-tree v-for="tree in category.trees"
                :key="'tree-' + category.categoryID + '-' + tree.entry.treeID" :tree="tree">
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
            }
        },

        components: {
            CharacterDirectiveTree
        }
    });
    export default CharacterDirectiveCategory;
</script>