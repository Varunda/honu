<template>
    <div>
        <!--
            <div class="row mb-3">
                <div class="col-4 text-center">
                    <div :style="{ 'background-color': colors.vs }">
                        {{1 | factionLong}}
                    </div>
                    {{alert.countVS}}
                </div>

                <div class="col-4 text-center">
                    <div :style="{ 'background-color': colors.nc }">
                        {{2 | factionLong}}
                    </div>
                    {{alert.countNC}}
                </div>

                <div class="col-4 text-center">
                    <div :style="{ 'background-color': colors.tr }">
                        {{3 | factionLong}}
                    </div>
                    {{alert.countTR}}
                </div>
            </div>
        -->

        <div class="progress" style="height: 3rem;">
            <div class="progress-bar" :style="{ width: vsWidth, 'background-color': colors.vs, 'font-size': '2.5rem', border: (alert.victorFactionID == 1) ? 'gold solid 3px' : '' }">
                {{1 | faction}}
                <!--
                -
                {{alert.countVS / alert.zoneFacilityCount * 100 | locale(0)}}%
                ({{alert.countVS}})
                -->
            </div>

            <div class="progress-bar" :style="{ width: ncWidth, 'background-color': colors.nc, 'font-size': '2.5rem', border: (alert.victorFactionID == 2) ? 'gold solid 3px' : '' }">
                {{2 | faction}}
                <!--
                -
                {{alert.countNC / alert.zoneFacilityCount * 100 | locale(0)}}%
                ({{alert.countNC}})
                -->
            </div>

            <div class="progress-bar" :style="{ width: trWidth, 'background-color': colors.tr, 'font-size': '2.5rem', border: (alert.victorFactionID == 3) ? 'gold solid 3px' : '' }">
                {{3 | faction}}
                <!--
                -
                {{alert.countTR / alert.zoneFacilityCount * 100 | locale(0)}}%
                ({{alert.countTR}})
                -->
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsAlert } from "api/AlertApi";

    import ColorUtils from "util/Color";

    import "filters/FactionNameFilter";

    export const AlertWinner = Vue.extend({
        props: {
            alert: { type: Object as PropType<PsAlert>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            vsWidth: function(): string {
                if (this.alert.countVS == null) {
                    return "";
                }

                return `${this.alert.countVS / this.alert.zoneFacilityCount * 100}%`;
            },

            ncWidth: function(): string {
                if (this.alert.countNC == null) {
                    return "";
                }

                return `${this.alert.countNC / this.alert.zoneFacilityCount * 100}%`;
            },

            trWidth: function(): string {
                if (this.alert.countTR == null) {
                    return "";
                }

                return `${this.alert.countTR / this.alert.zoneFacilityCount * 100}%`;
            },

            colors: function() {
                return {
                    vs: ColorUtils.VS,
                    nc: ColorUtils.NC,
                    tr: ColorUtils.TR,
                    ns: ColorUtils.NS,
                };
            }
        },

        components: {

        }
    });
    export default AlertWinner;
</script>