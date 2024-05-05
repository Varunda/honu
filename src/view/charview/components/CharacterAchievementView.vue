
<template>
    <div>

        <div class="mb-2">
            <toggle-button v-model="showDebug">
                show debug
            </toggle-button>

            <toggle-button v-model="showUnknown">
                show unknown
            </toggle-button>
        </div>

        <a-table v-if="showTable"
            :entries="data"
            :show-filters="true" display-type="table"
            default-sort-field="name" default-sort-order="asc">

            <a-col sort-field="name">
                <a-header>
                    <b>Achievement</b>
                </a-header>

                <a-filter method="input" type="string" field="name"
                    :conditions="[ 'contains', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <div style="height: 2rem; width: 4rem; display: inline-flex;" class="justify-content-center">
                        <census-image v-if="entry.imageID != -1" :image-id="entry.imageID" style="height: 2rem;"></census-image>
                    </div>

                    <span>
                        {{entry.name}}
                    </span>

                    <span v-if="showDebug">
                        ({{entry.achievementID}})
                    </span>
                    <info-hover :text="entry.description"></info-hover>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>How to earn</b>
                </a-header>

                <a-filter method="dropdown" field="objTypeID" type="number" :source="objTypeIDs" max-width="60ch"
                    :conditions="[ 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    <div v-if="entry.objective == null" class="text-muted">
                        missing objective
                    </div>
                    
                    <div v-else style="max-width: 50ch;">
                        <character-achievement-objective-data :block="block.data" :entry="entry" :show-debug="showDebug"></character-achievement-objective-data>
                    </div>
                </a-body>
            </a-col>

            <a-col>
                <a-header>
                    <b>Repeatable</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.achievement != null && entry.achievement.repeatable == false">
                        no
                    </span>
                    <span v-else-if="entry.achievement != null && entry.achievement.repeatable == true">
                        yes
                    </span>
                    <span v-else>
                        unknown
                    </span>
                </a-body>
            </a-col>

            <a-col sort-field="earnedCount">
                <a-header>
                    <b>Earned</b>
                </a-header>

                <a-filter method="input" type="number" field="earnedCount" max-width="20ch"
                    :conditions="[ 'greater_than', 'equals' ]">
                </a-filter>

                <a-body v-slot="entry">
                    {{entry.earnedCount}}
                </a-body>
            </a-col>

            <a-col sort-field="dateStarted">
                <a-header>
                    <b>Started</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.dateStarted | moment}}
                </a-body>
            </a-col>

            <a-col sort-field="dateUpdated">
                <a-header>
                    <b>Last updated</b>
                </a-header>

                <a-body v-slot="entry">
                    {{entry.dateUpdated | moment}}
                </a-body>
            </a-col>

            <a-col sort-field="dateFinished">
                <a-header>
                    <b>Finished</b>
                </a-header>

                <a-body v-slot="entry">
                    <span v-if="entry.dateFinished == null">
                        --
                    </span>
                    <span v-else>
                        {{entry.dateFinished | moment}}
                    </span>
                </a-body>
            </a-col>

        </a-table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType }  from "vue";
    import { Loading, Loadable } from "Loading";

    import InfoHover from "components/InfoHover.vue";
    import { ATable, ACol, AHeader, AFilter, ABody } from "components/ATable";
    import Busy from "components/Busy.vue";
    import CensusImage from "components/CensusImage";
    import ToggleButton from "components/ToggleButton";

    import CharacterAchievementObjectiveData from "./CharacterAchievementObjectiveData.vue";

    import { PsCharacter } from "api/CharacterApi";
    import { CharacterAchievementApi, CharacterAchievementBlock, CharacterAchievement } from "api/CharacterAchievementApi";
    import { PsItem } from "api/ItemApi";
    import { Achievement } from "api/AchievementApi";
    import { PsObjective } from "api/ObjectiveApi";
    import { ObjectiveType } from "api/ObjectiveTypeApi";
    import { PsVehicle } from "api/VehicleApi";

    import { FlatCharacterAchievement } from "./common";

    export const CharacterAchievementView = Vue.extend({
        props: {
            character: { type: Object as PropType<PsCharacter>, required: true }
        },

        data: function() {
            return {
                block: Loadable.idle() as Loading<CharacterAchievementBlock>,

                showTable: true as boolean,

                showDebug: false as boolean,
                showUnknown: false as boolean

            }
        },

        mounted: function(): void {
            this.loadData();
        },

        methods: {
            loadData: async function(): Promise<void> {
                this.block = Loadable.loading();
                this.block = await CharacterAchievementApi.getByCharacterID(this.character.id);

                if (this.block.state == "loaded") {
                    // this lil hack lets the objTypeIDs computed property update, which forces the <a-table>
                    // to be remade, which then updates the <a-filter:source> property
                    this.showTable = false;
                    this.$nextTick(() => {
                        this.showTable = true;
                    });
                }
            }
        },

        computed: {
            data: function(): Loading<FlatCharacterAchievement[]> {
                if (this.block.state != "loaded") {
                    return Loadable.rewrap(this.block);
                }

                const achs: Map<number, Achievement> = new Map(this.block.data.achievements.map(iter => [iter.id, iter]));
                const items: Map<number, PsItem> = new Map(this.block.data.items.map(iter => [iter.id, iter]));
                const objs: Map<number, PsObjective> = new Map(this.block.data.objectives.map(iter => [iter.id, iter]));
                const objGroup: Map<number, PsObjective> = new Map(this.block.data.objectives.map(iter => [iter.groupID, iter]));
                const objTypes: Map<number, ObjectiveType> = new Map(this.block.data.objectiveTypes.map(iter => [iter.id, iter]));
                const vehicles: Map<number, PsVehicle> = new Map(this.block.data.vehicles.map(iter => [iter.id, iter]));

                return Loadable.loaded(this.block.data.entries
                    .filter(iter => {
                        if (this.showUnknown == false && achs.get(iter.achievementID) == undefined) {
                            return false;
                        }
                        return true;
                    })
                    .map((iter: CharacterAchievement) => {
                        const ach: Achievement | undefined = achs.get(iter.achievementID);
                        const item: PsItem | undefined = (ach == undefined || ach.itemID == null) ? undefined : items.get(ach.itemID);
                        const obj: PsObjective | undefined = (ach == undefined) ? undefined : objGroup.get(ach.objectiveGroupID);
                        const objType: ObjectiveType | undefined = (obj == undefined) ? undefined : objTypes.get(obj.typeID);

                        const objTypeID: number = obj?.typeID ?? objType?.id ?? -1;

                        let vehicle: PsVehicle | undefined = undefined;
                        // 12 => param4
                        // 21 => param4
                        // 37 => param4
                        // 90 => param2
                        // 91 => param2
                        if (obj != null) {
                            if (obj.param4 != null && (objTypeID == 12 || objTypeID == 21 || objTypeID == 37)) {
                                vehicle = vehicles.get(Number.parseInt(obj.param4));
                            } else if (obj.param2 != null && (objTypeID == 90 || objTypeID == 91)) {
                                vehicle = vehicles.get(Number.parseInt(obj.param2));
                            }
                        }

                        return {
                            achievementID: iter.achievementID,
                            earnedCount: iter.earnedCount,
                            dateFinished: iter.finishDate,
                            dateStarted: iter.startDate,
                            dateUpdated: iter.lastSaveDate,

                            name: ach?.name ?? `<missing ${iter.achievementID}>`,
                            description: ach?.description ?? `<missing ${iter.achievementID}>`,
                            imageID: ach?.imageID ?? -1,

                            achievement: ach || null,
                            item: item || null,
                            objTypeID: objTypeID,
                            objective: obj || null,
                            objectiveType: objType || null,
                            vehicle: vehicle || null
                        };
                    }
                ));
            },

            objTypeIDs: function(): any[] {
                if (this.block.state != "loaded") {
                    return [];
                }

                console.log(`mapping obj types`);

                const entries: {key: string, value: number | null}[] = this.block.data.objectiveTypes.map(iter => {
                    return {
                        key: iter.description,
                        value: iter.id
                    }
                });
                entries.push({
                    key: "All",
                    value: null
                });

                console.log(`got ${entries.length} entries!`);
                return entries;
            }
        },

        components: {
            Busy,
            InfoHover, CensusImage, ToggleButton,
            ATable, ACol, AHeader, AFilter, ABody,
            CharacterAchievementObjectiveData
        }
    });
    export default CharacterAchievementView;

</script>