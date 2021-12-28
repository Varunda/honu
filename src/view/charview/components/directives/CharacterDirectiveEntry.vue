<template>
    <span class="border" style="flex-grow: 1; width: 30%; max-width: 30%; border-radius: 0.25rem; padding: 0.25rem 0.5rem; margin-right: 1rem;">
        <h5 :title="entry.directive.description">
            <!-- <img style="margin: -16px;" :src="'https://census.daybreakgames.com/files/ps2/images/static/' + entry.directive.imageID + '.png'"/> -->
            {{entry.directive.name}}
        </h5>

        <div v-if="objectiveGoal != null" class="progress" style="height: 2rem; font-size: 14pt;">
            <div v-if="objectiveGoal != null" class="progress-bar"
                :style="{ 'width': objectivePercent + '%' }">

                {{objectiveProgress || 0}} / {{objectiveGoal}}
            </div>

            <div v-if="entry.entry != null && entry.entry.completionDate != null" class="progress-bar bg-success w-100 text-dark">
                {{entry.entry.completionDate | moment}}

                {{objectiveGoal}} / {{objectiveGoal}}
            </div>
        </div>

    </span>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { ExpandedCharacterDirective } from "api/CharacterDirectiveApi";
    import { PsObjective } from "api/ObjectiveApi";
    import { ObjectiveType } from "api/ObjectiveTypeApi";

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
            objectiveGoal: function(): number | null {
                if (this.entry.objectiveType == null || this.entry.objective == null) {
                    return null;
                }

                const objType: ObjectiveType = this.entry.objectiveType;
                const obj: PsObjective = this.entry.objective;
                let param: string | null = null;

                if (objType.id == 12) { // Kill count
                    param = obj.param1;
                } else if (objType.id == 69) { // XP reward count
                    param = obj.param1;
                } else if (objType.id == 66) { // Achievement count

                } else {
                    console.warn(`Unchecked objective type id ${objType.id}`);
                }

                if (param == null) {
                    console.warn(`failed to get objective count from objective type id ${objType.id}`);
                    return null;
                }

                return Number.parseInt(param);
            },

            objectiveProgress: function(): number | null {
                if (this.entry.characterObjective == null) {
                    return null;
                }

                return this.entry.characterObjective.stateData;
            },

            objectivePercent: function(): number {
                const goal: number = this.objectiveGoal || 0;
                const progress: number = this.objectiveProgress || 0;

                return progress / goal * 100;
            }

        },

        components: {

        }
    });
    export default CharacterDirectiveEntry;

</script>