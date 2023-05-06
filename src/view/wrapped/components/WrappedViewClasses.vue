<template>
    <div>
        <collapsible header-text="Class stats">

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--blue)">
                    General
                </h3>

                <table class="table border-top-0">
                    <thead class="table-secondary">
                        <tr>
                            <th>Class</th>
                            <th>
                                Time as
                            </th>
                            <th>
                                Kills
                            </th>
                            <th>
                                Deaths
                                <info-hover text="Revives remove a death"></info-hover>
                            </th>
                            <th>
                                K/D
                                <info-hover text="Revives remove a death"></info-hover>
                            </th>
                            <th>
                                KPM
                            </th>
                            <th>
                                Score
                            </th>
                            <th>
                                SPM
                                <info-hover text="Score per minute"></info-hover>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="clazz in general">
                            <td>
                                {{clazz.name}}
                            </td>
                            <td>
                                {{clazz.timeAs / 1000 | mduration}}
                            </td>
                            <td>
                                {{clazz.kills | locale(0)}}
                            </td>
                            <td>
                                {{clazz.deaths | locale(0)}}
                            </td>
                            <td>
                                {{clazz.kills / Math.max(1, clazz.deaths) | locale(2)}}
                            </td>
                            <td>
                                {{clazz.kills / Math.max(1, clazz.timeAs / 1000) * 60 | locale(2)}}
                            </td>
                            <td :title="clazz.exp | locale(0)">
                                {{clazz.exp | compact}}
                            </td>
                            <td>
                                {{clazz.exp / Math.max(1, clazz.timeAs / 1000) * 60 | locale(2)}}
                            </td>
                        </tr>
                    </tbody>
                </table>

            </div>

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
    import "filters/CompactFilter";

    // utils
    import LoadoutUtils from "util/Loadout";

    // models
    import { WrappedClassStats } from "../common";

    export const WrappedViewClasses = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {
                general: [] as WrappedClassStats[]
            }
        },

        created: function(): void {
            this.makeAll();
        },

        methods: {
            makeAll: function(): void {
                this.makeClassGeneralStats();
            },

            makeClassGeneralStats: function(): void {
                this.general = this.wrapped.extra.classStats;
            }
        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewClasses;
</script>