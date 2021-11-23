<template>
    <div>
        template
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import Report from "../Report";

    export const ReportZ = Vue.extend({
        props: {
            report: { type: Object as PropType<Report>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        }
    });

    export default ReportZ;
</script>