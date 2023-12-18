<template>
    <div>
        <collapsible header-text="Achievements earned">
            <h3 class="wt-header mb-0 border-0" style="background-color: var(--cyan)">
                Achievements
            </h3>

            <a-table :entries="data"
                     :paginate="true"
                     :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                     default-sort-field="count" default-sort-order="desc"
                     class="border-top-0"
            >

                <a-col>
                    <a-header>
                        Achievement
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.name}}
                        <info-hover v-if="entry.achievement != null" :text="entry.achievement.description"></info-hover>
                    </a-body>
                </a-col>

                <a-col sort-field="captures">
                    <a-header>
                        Count
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.count | locale}}
                    </a-body>
                </a-col>

            </a-table>

        </collapsible>
    </div>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";

    import { Achievement } from "api/AchievementApi";

    class WrappedAchievement {
        public achievementID: number = 0;
        public count: number = 0;

        public achievement: Achievement | null = null;
        public name: string = "";
    }

    export const WrappedViewAchievements = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {

            data: function(): Loading<WrappedAchievement[]> {
                const map: Map<number, WrappedAchievement> = new Map();
                for (const ach of this.wrapped.achievementEarned) {

                    let iter: WrappedAchievement | undefined = map.get(ach.achievementID);
                    if (iter == undefined) {
                        iter = new WrappedAchievement();
                        iter.achievementID = ach.achievementID;

                        iter.achievement = this.wrapped.achivements.get(iter.achievementID) ?? null;
                        if (iter.achievement != null) {
                            iter.name = iter.achievement.name;
                        } else {
                            iter.name = `<unknown ${iter.achievementID}>`;
                        }
                    }

                    iter.count += 1;

                    map.set(iter.achievementID, iter);
                }

                return Loadable.loaded(Array.from(map.values()));
            }

        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewAchievements;
</script>