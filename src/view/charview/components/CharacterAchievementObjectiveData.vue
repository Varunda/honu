
<template>
    <div>
        <span v-if="entry.objTypeID == 12">
            Get
            <span v-if="!entry.objective.param1" class="text-warning">
                missing param1
            </span>

            <cell :entry="entry" :param="1"></cell>
            kills

            <span v-if="entry.achievement && entry.achievement.itemID">
                using the
                <span v-if="entry.item" class="text-param">
                    {{entry.item.name}}
                </span>
                <span v-else class="text-param">
                    unknown weapon {{entry.achievement.itemID}}
                </span>
            </span>
        </span>

        <span v-else-if="entry.objTypeID == 14">
            Have squad members lower than BR
            <cell :entry="entry" :param="2"></cell>

            earn
            <cell :entry="entry" :param="1"></cell>

            times as a platoon leader
        </span>

        <span v-else-if="entry.objTypeID == 15">
            Earn

            <cell :entry="entry" :param="1"></cell>
            xp

            from a {{awardTypeString}} exp event
        </span>

        <span v-else-if="entry.objTypeID == 17">
            Have squad members earn
            <cell :entry="entry" :param="1"></cell>
            exp

            of type {{awardTypeString}}
        </span>

        <span v-else-if="entry.objTypeID == 19">
            Complete
            <cell :entry="entry" :param="2"></cell>

            achievements of type
            <cell :entry="entry" :param="1"></cell>

            as a platoon leader
        </span>

        <span v-else-if="entry.objTypeID == 20">
            <span v-if="entry.objective.param5 == '1'">
                Capture
            </span>
            <span v-else-if="entry.objective.param5 == '0'">
                Defend
            </span>
            <span v-else class="text-warning">
                Unchecked value of param2: {{entry.objective.param2}}
            </span>

            facilities 
            <cell :entry="entry" :param="1"></cell>
            times,

            with at least
            <cell :entry="entry" :param="3"></cell>
            enemies
        </span>

        <span v-else-if="entry.objTypeID == 37">
            Earn a killstreak of 
            <cell :entry="entry" :param="1"></cell>
            players 

            <span v-if="entry.objective.param9 == '0'">
                (without dying)
            </span>
        </span>

        <span v-else-if="entry.objTypeID == 69">
            Earn exp events classified as {{awardTypeString}} events
            
            <cell :entry="entry" :param="1"></cell> times
        </span>

        <span v-else-if="entry.objTypeID == 90">
            Have passengers kill
            <cell :entry="entry" :param="1"></cell>
            enemies while you drive
        </span>

        <span v-else-if="entry.objTypeID == 91">
            Repair
            <cell :entry="entry" :param="1"></cell>
            damage
        </span>

        <span v-else-if="entry.objTypeID == 92">
            Revive
            <cell :entry="entry" :param="1"></cell>
            players
        </span>

        <span v-else-if="entry.objTypeID == 93">
            <span v-if="entry.objective.param2 == '0'">
                Capture
            </span>
            <span v-else-if="entry.objective.param2 == '1'">
                Defend
            </span>
            <span v-else class="text-warning">
                Unchecked value of param2: {{entry.objective.param2}}
            </span>

            <span v-if="!entry.objective.param1" class="text-warning">
                missing param1
            </span>
            <cell :entry="entry" :param="1"></cell>
            
            <span v-if="!entry.objective.param3" class="text-warning">
                missing param2
            </span>
            {{entry.objective.param3 | facilityType}}

            facilities
        </span>

        <span v-else>
            unchecked objective type {{entry.objTypeID}}
        </span>

        <span v-if="ShowDebug" class="text-monospace">
            <br />
            objTypeID={{entry.objTypeID}}

            <span v-if="entry.objective.param1 != null || entry.objectiveType.param1 != null">
                param1={{entry.objective.param1}}/{{entry.objectiveType.param1}}<br />
            </span>
            <span v-if="entry.objective.param2 != null || entry.objectiveType.param2 != null">
                param2={{entry.objective.param2}}/{{entry.objectiveType.param2}}<br />
            </span>
            <span v-if="entry.objective.param3 != null || entry.objectiveType.param3 != null">
                param3={{entry.objective.param3}}/{{entry.objectiveType.param3}}<br />
            </span>
            <span v-if="entry.objective.param4 != null || entry.objectiveType.param4 != null">
                param4={{entry.objective.param4}}/{{entry.objectiveType.param4}}<br />
            </span>
            <span v-if="entry.objective.param5 != null || entry.objectiveType.param5 != null">
                param5={{entry.objective.param5}}/{{entry.objectiveType.param5}}<br />
            </span>
            <span v-if="entry.objective.param6 != null || entry.objectiveType.param6 != null">
                param6={{entry.objective.param6}}/{{entry.objectiveType.param6}}<br />
            </span>
            <span v-if="entry.objective.param7 != null || entry.objectiveType.param7 != null">
                param7={{entry.objective.param7}}/{{entry.objectiveType.param7}}<br />
            </span>
            <span v-if="entry.objective.param8 != null || entry.objectiveType.param8 != null">
                param8={{entry.objective.param8}}/{{entry.objectiveType.param8}}<br />
            </span>
            <span v-if="entry.objective.param9 != null || entry.objectiveType.param9 != null">
                param9={{entry.objective.param9}}/{{entry.objectiveType.param9}}<br />
            </span>
            <span v-if="entry.objective.param10 != null || entry.objectiveType.param10 != null">
                param10={{entry.objective.param10}}/{{entry.objectiveType.param10}}<br />
            </span>
            <span v-if="entry.objective.param11 != null || entry.objectiveType.param11 != null">
                param11={{entry.objective.param11}}/{{entry.objectiveType.param11}}<br />
            </span>
        </span>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import "filters/FacilityTypeFilter";

    import { FlatCharacterAchievement } from "./common";
    import { CharacterAchievementBlock } from "api/CharacterAchievementApi";
import { ExperienceAwardType } from "../../../api/ExpStatApi";

    const Cell = Vue.extend({
        props: {
            entry: { type: Object as PropType<FlatCharacterAchievement> },
            param: { type: Number, required: true }
        },

        computed: {
            paramText: function(): string {
                if (this.entry.objectiveType == null) {
                    return "";
                }

                if (this.param == 1) { return this.entry.objectiveType.param1 ?? ""; }
                if (this.param == 2) { return this.entry.objectiveType.param2 ?? ""; }
                if (this.param == 3) { return this.entry.objectiveType.param3 ?? ""; }
                if (this.param == 4) { return this.entry.objectiveType.param4 ?? ""; }
                if (this.param == 5) { return this.entry.objectiveType.param5 ?? ""; }
                if (this.param == 6) { return this.entry.objectiveType.param6 ?? ""; }
                if (this.param == 7) { return this.entry.objectiveType.param7 ?? ""; }
                if (this.param == 8) { return this.entry.objectiveType.param8 ?? ""; }
                if (this.param == 9) { return this.entry.objectiveType.param9 ?? ""; }
                if (this.param == 10) { return this.entry.objectiveType.param10 ?? ""; }

                return "";
            },

            paramValue: function(): string {
                if (this.entry.objective == null) {
                    return "";
                }

                if (this.param == 1) { return this.entry.objective.param1 ?? ""; }
                if (this.param == 2) { return this.entry.objective.param2 ?? ""; }
                if (this.param == 3) { return this.entry.objective.param3 ?? ""; }
                if (this.param == 4) { return this.entry.objective.param4 ?? ""; }
                if (this.param == 5) { return this.entry.objective.param5 ?? ""; }
                if (this.param == 6) { return this.entry.objective.param6 ?? ""; }
                if (this.param == 7) { return this.entry.objective.param7 ?? ""; }
                if (this.param == 8) { return this.entry.objective.param8 ?? ""; }
                if (this.param == 9) { return this.entry.objective.param9 ?? ""; }
                if (this.param == 10) { return this.entry.objective.param10 ?? ""; }

                return "";
            }
        },

        template: `<span class="text-param" :title="'param' + param + ': ' + paramText">{{paramValue}}</span>`
    });

    export const CharacterAchievementObjectiveData = Vue.extend({
        props: {
            block: { type: Object as PropType<CharacterAchievementBlock> },
            entry: { type: Object as PropType<FlatCharacterAchievement> },
            ShowDebug: { type: Boolean, required: false }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {

            awardType1: function(): ExperienceAwardType | null {
                if (this.entry.objective == null || this.entry.objective.param2 == null) {
                    return null;
                }

                return this.block.awardTypes.find(iter => iter.id.toString() == this.entry.objective!.param2) ?? null;
            },

            awardType2: function(): ExperienceAwardType | null {
                if (this.entry.objective == null || this.entry.objective.param3 == null) {
                    return null;
                }

                return this.block.awardTypes.find(iter => iter.id.toString() == this.entry.objective!.param3) ?? null;
            },

            awardType3: function(): ExperienceAwardType | null {
                if (this.entry.objective == null || this.entry.objective.param4 == null) {
                    return null;
                }

                return this.block.awardTypes.find(iter => iter.id.toString() == this.entry.objective!.param4) ?? null;
            },

            awardType4: function(): ExperienceAwardType | null {
                if (this.entry.objective == null || this.entry.objective.param5 == null) {
                    return null;
                }

                return this.block.awardTypes.find(iter => iter.id.toString() == this.entry.objective!.param5) ?? null;
            },

            awardTypeString: function(): string {
                const types: string[] = [
                    this.awardType1,
                    this.awardType2,
                    this.awardType3,
                    this.awardType4
                ].filter(iter => iter != null).map(iter => iter!.name);

                return types.join(", or ");
            }

        },

        components: {
            Cell
        }
    });
    export default CharacterAchievementObjectiveData;

</script>