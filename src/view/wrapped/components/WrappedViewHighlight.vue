<template>
    <div>
        <collapsible header-text="Highlights">

            <div>
                Total time played
                total kills
                total deaths
                total KD
                total KPM
                total exp
                total SPM
            </div>

            <div>
                most killed and most died to
            </div>

            <div>
                best and worst KD
                character and outfit
            </div>

            <div>
                most healed, most resupplied, most repaired
            </div>

            <div>
                Vehicles destroyed
            </div>

            <div>
                facilities captured
                facilities defended
            </div>

            <div>
                squad shit
                how many unique people you ran in your squads
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

    export const WrappedViewHighlight = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        components: {
            Collapsible,
            InfoHover,
            ATable, AFilter, AHeader, ABody, ACol, ARank,
        }

    });
    export default WrappedViewHighlight;
</script>