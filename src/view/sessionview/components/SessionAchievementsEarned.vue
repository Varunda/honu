<template>
    <div>
        <div class="d-flex flex-wrap mw-100">
            <div class="flex-grow-1 flex-basis-0">
                <chart-entry-list :data="blockEarned" left-column-title="Achievement" middle-column-title="Count"></chart-entry-list>
            </div>
            <div class="flex-grow-1 flex-basis-0">
                <chart-entry-pie :data="blockEarned"></chart-entry-pie>
            </div>

            <div class="flex-grow-1 flex-basis-0"></div>
            <div class="flex-grow-1 flex-basis-0"></div>
        </div>

        <a @click="showTable = !showTable" href="javascript:void(0);">(debug) show all achievement earned events</a>

        <table v-show="showTable" class="table">
            <thead class="table-secondary">
                <tr>
                    <th colspan="2">Achievement</th>
                    <th>Continent</th>
                    <th>Timestamp</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="a in achievements" :key="a.event.id">
                    <td style="width: 128px;">
                        <div v-if="a.achievement != null">
                            <census-image :image-id="a.achievement.imageID" style="height: 64px;"></census-image>
                        </div>
                        <div v-else style="height: 64px;"></div>
                    </td>
                    <td class="align-middle">
                        <div v-if="a.achievement != null">
                            {{a.achievement.name}}: {{a.achievement.description}}
                        </div>
                        <div v-else style="height: 64px;">
                            &lt;unknown achievement {{a.event.achievementID}}&gt;
                        </div>
                    </td>

                    <td class="align-middle">
                        {{a.event.zoneID | zone}}
                    </td>

                    <td class="align-middle">
                        {{a.event.timestamp | moment}}
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { AchievementEarned, AchievementEarnedBlock } from "api/AchievementEarnedApi";
    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";
    import { Achievement } from "api/AchievementApi";

    import CensusImage from "components/CensusImage";

    import ChartEntryList from "./ChartEntryList.vue";
    import ChartEntryPie from "./ChartEntryPie.vue";

    import "MomentFilter";
    import "filters/ZoneNameFilter";

    interface Entry {
        display: string;
        count: number;
        link: string | null;
    }

    type ExpandedAchievementEarned = {
        event: AchievementEarned;
        achievement: Achievement | null;
    }

    export const SessionAchievementsEarned = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            earned: { type: Object as PropType<AchievementEarnedBlock>, required: true },
        },

        data: function() {
            return {
                blockEarned: [] as Entry[],

                showTable: false as boolean
            }
        },

        created: function(): void {
            this.makeBlock();
        },

        methods: {
            makeBlock: function(): void {
                const map: Map<number, Entry> = new Map();

                for (const ev of this.achievements) {
                    let entry: Entry | undefined = map.get(ev.event.achievementID);

                    if (entry == undefined) {
                        entry = {
                            display: "",
                            count: 0,
                            link: null
                        };

                        const name: string = (ev.achievement == null) ? "missing achievement" : ev.achievement.name;
                        entry.display = name;
                    }

                    ++entry.count;

                    map.set(ev.event.achievementID, entry);
                }

                this.blockEarned = Array.from(map.values()).sort((a, b) => b.count - a.count);
            }
        },

        computed: {
            achievements: function(): ExpandedAchievementEarned[] {
                return this.earned.events.map((iter: AchievementEarned): ExpandedAchievementEarned => {
                    return {
                        event: iter,
                        achievement: this.earned.achievements.get(iter.achievementID) || null
                    };
                });
            }
        },

        components: {
            CensusImage,
            ChartEntryList, ChartEntryPie
        }
    });
    export default SessionAchievementsEarned;

</script>