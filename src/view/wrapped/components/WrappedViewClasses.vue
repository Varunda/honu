<template>
    <div>
        <collapsible header-text="Class stats">

            <div>
                <h3 class="wt-header mb-0 border-0" style="background-color: var(--blue)">
                    General
                </h3>

                <a-table :entries="classData"
                         :paginate="true"
                         :page-sizes="[10, 20, 50, 100]" :default-page-size="10"
                         default-sort-field="timeAs" default-sort-order="desc"
                         class="border-top-0"
                >

                    <a-col sort-field="name">
                        <a-header>
                            Class
                        </a-header>

                        <a-body v-slot="entry">
                            <img :src="'/img/classes/icon_' + entry.icon + '.png'" style="width: 24px;" />

                            {{entry.name}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="timeAs">
                        <a-header>
                            Time as
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.timeAs / 1000 | mduration}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="kills">
                        <a-header>
                            Kills
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kills | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="deaths">
                        <a-header>
                            Deaths
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.deaths | locale}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="kd">
                        <a-header>
                            K/D
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kd | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="kpm">
                        <a-header>
                            KPM
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.kpm | locale(2)}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="exp">
                        <a-header>
                            Score
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.exp | compact}}
                        </a-body>
                    </a-col>

                    <a-col sort-field="spm">
                        <a-header>
                            SPM
                        </a-header>

                        <a-body v-slot="entry">
                            {{entry.spm | locale(2)}}
                        </a-body>
                    </a-col>

                </a-table>

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

        computed: {
            classData: function(): Loading<WrappedClassStats[]> {
                return Loadable.loaded(this.wrapped.extra.classStats);
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