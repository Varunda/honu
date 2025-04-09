<template>
    <div class="border" style="flex-grow: 1; flex-basis: 33.333333%; padding: 0.25rem 0.5rem 0.5rem 0.5rem; max-width: 33.333333%">
        <h5 :title="entry.directive.description">
            <!-- <img style="margin: -16px;" :src="'https://census.daybreakgames.com/files/ps2/images/static/' + entry.directive.imageID + '.png'"/> -->
            {{entry.name}}

            <span v-if="showDebug == true">
                directive id: {{entry.directive.id}}, obj set id: {{ entry.directive.objectiveSetID }}
            </span>

            <info-hover v-if="entry.description && entry.description.length > 0" :text="entry.description">
            </info-hover>
        </h5>

        <div class="progress" style="height: 2rem; font-size: 14pt;">
            <span v-if="objectiveGoal != null" class="" style="position: absolute; line-height: 2rem; padding-left: 0.5rem">
                <span v-if="entry.entry != null && entry.entry.completionDate != null">
                    {{objectiveGoal}}
                </span>
                <span v-else>
                    {{objectiveProgress || 0}}
                </span>

                / {{objectiveGoal}}
            </span>

            <span v-else-if="entry.entry == null || entry.entry.completionDate == null" style="position: absolute; line-height: 2rem; padding-left: 0.5rem; font-size: 10pt;" class="text-muted">
                Unknown objective, cannot load progress
            </span>

            <div v-if="entry.entry == null || (objectiveGoal != null && entry.entry.completionDate == null)" class="progress-bar"
                :style="{ 'width': objectivePercent + '%' }">
            </div>

            <div v-if="entry.entry != null && entry.entry.completionDate != null" class="progress-bar bg-success w-100">
                {{entry.entry.completionDate | moment}}
                ({{entry.entry.completionDate | timeAgo}})
            </div>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { ExpandedCharacterDirective } from "api/CharacterDirectiveApi";
    import { PsObjective } from "api/ObjectiveApi";
    import { ObjectiveType } from "api/ObjectiveTypeApi";

    import InfoHover from "components/InfoHover.vue";

    export const CharacterDirectiveEntry = Vue.extend({
        props: {
            entry: { type: Object as PropType<ExpandedCharacterDirective>, required: true },
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            showDebug: function(): boolean {
                return (this.$root as any).debug.directive;
            },

            objectiveGoal: function(): number | null {
                return (this.entry as any).goal;
            },

            objectiveProgress: function(): number | null {
                return (this.entry as any).progress;
            },

            objectivePercent: function(): number {
                const goal: number = this.objectiveGoal || 0;
                const progress: number = this.objectiveProgress || 0;

                return (progress / Math.max(1, goal)) * 100;
            }

        },

        components: {
            InfoHover
        }
    });
    export default CharacterDirectiveEntry;

</script>