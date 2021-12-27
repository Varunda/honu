<template>
    <div>
        <hr class="border" />

        <div class="d-flex">
            <div v-if="set.state == 'idle'"></div>
            
            <div v-else-if="set.state == 'loading'">
                Loading... <busy></busy>
            </div>

            <div v-else-if="set.state == 'error'">
                Error loading directives: {{set.message}}
            </div>

            <div v-else-if="set.state == 'loaded'">
                <div v-for="category in set.data.categories" :key="category.categoryID" class="border-bottom">
                    <h2>
                        <span v-if="category.category != null">
                            {{category.category.name}}
                        </span>
                        <span v-else>
                            &lt;missing category {{category.categoryID}}&gt;
                        </span>
                    </h2>

                    <div v-for="tree in category.trees">
                        <h3>
                            <span v-if="tree.tree != null">
                                <img style="margin: -32px;" :src="'https://census.daybreakgames.com/files/ps2/images/static/' + tree.tree.imageID + '.png'"/>
                                {{tree.tree.name}}
                                {{tree.entry.currentTier}}
                                <span v-if="tree.entry.completionDate != null" class="border-left ml-2 pl-2">
                                    {{tree.entry.completionDate | moment}}
                                </span>
                            </span>
                            <span v-else>
                                &lt;missing tree&gt;
                            </span>
                        </h3>

                        <div v-for="tier in tree.tiers">
                            <div v-if="tier.tierID == tree.entry.currentTier">
                                <div v-for="directive in tier.directives">
                                    <div v-if="directive.directive != null">
                                        <div>
                                            <h5 class="d-inline-block">{{directive.directive.name}}</h5>
                                            <span v-if="directive.entry.completionDate != null" class="border-left ml-2 pl-2">
                                                Completed: {{directive.entry.completionDate | moment}}
                                            </span>
                                        </div>
                                        <h6>{{directive.directive.description}}</h6>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
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

    import { PsCharacter } from "api/CharacterApi";
    import { CharacterDirectiveSet, CharacterDirectiveApi } from "api/CharacterDirectiveApi";

    import Busy from "components/Busy.vue";

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
            ABody
        }

    });
    export default CharacterDirectives;
</script>
