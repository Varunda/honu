<template>
    <collapsible header-text="Experience earned">

        <div>
            <h3 class="wt-header mb-0 border-0" style="background-color: var(--purple)">
                Experience events
            </h3>

            <a-table :entries="expData"
                     :paginate="true"
                     :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                     default-sort-field="count" default-sort-order="desc"
                     class="border-top-0"
            >

                <a-col>
                    <a-header>
                        Type
                    </a-header>

                    <a-body v-slot="entry">
                        <span v-if="entry.expType != null">
                            {{entry.expType.name}}
                        </span>
                        <span v-else>
                            &lt;missing {{entry.expID}}&gt;
                        </span>
                    </a-body>
                </a-col>

                <a-col sort-field="count">
                    <a-header>
                        Count earned
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.count | locale}}
                        ({{entry.count / wrapped.exp.length * 100 | locale(2)}}%)
                    </a-body>
                </a-col>

                <a-col sort-field="countPerMin">
                    <a-header>
                        Count earned per minute
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.countPerMin | locale}}
                    </a-body>
                </a-col>

                <a-col sort-field="expEarned">
                    <a-header>
                        Exp earned
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.expEarned | locale}}
                        ({{entry.expEarned / totalExpAmount * 100 | locale(2)}}%)
                    </a-body>
                </a-col>

                <a-col sort-field="expEarnedPerMin">
                    <a-header>
                        Exp earned per min
                    </a-header>

                    <a-body v-slot="entry">
                        {{entry.expEarnedPerMin | locale(2)}}
                    </a-body>
                </a-col>

            </a-table>

        </div>

    </collapsible>
</template>

<script lang="ts">
    // general
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import { Loadable, Loading } from "Loading";

    // models
    import { ExperienceType } from "api/ExpStatApi";

    // components
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";
    import { ATable, AFilter, AHeader, ABody, ACol, ARank } from "components/ATable";

    // filters
    import "MomentFilter";
    import "filters/LocaleFilter";

    type ExpData = {
        expID: number;
        expType: ExperienceType | null;
        count: number;
        countPerMin: number;
        expEarned: number;
        expEarnedPerMin: number;
    }

    export const WrappedViewExp = Vue.extend({
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
            expData: function(): Loading<ExpData[]> {
                const map: Map<number, ExpData> = new Map();

                for (const exp of this.wrapped.exp) {
                    if (map.has(exp.experienceID) == false) {
                        map.set(exp.experienceID, {
                            expID: exp.experienceID,
                            expType: this.wrapped.expTypes.get(exp.experienceID) ?? null,
                            count: 0,
                            countPerMin: 0,
                            expEarned: 0,
                            expEarnedPerMin: 0
                        });
                    }

                    const elem: ExpData = map.get(exp.experienceID)!;

                    elem.count += 1;
                    elem.expEarned += exp.amount;
                }

                let arr: ExpData[] = Array.from(map.values());
                arr = arr.map((iter: ExpData) => {
                    iter.countPerMin = iter.count / Math.max(1, this.wrapped.extra.totalPlaytime / 60);
                    iter.expEarnedPerMin = iter.expEarned / Math.max(1, this.wrapped.extra.totalPlaytime / 60);
                    return iter;
                });

                return Loadable.loaded(arr);
            },

            totalExpAmount: function(): number {
                return this.wrapped.exp.reduce((acc, iter) => acc += iter.amount, 0);
            }
        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewExp;
</script>