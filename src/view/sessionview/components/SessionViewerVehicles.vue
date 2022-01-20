<template>
    <div>
        vehicle stuff
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { Session } from "api/SessionApi";
    import { PsCharacter } from "api/CharacterApi";
    import { ExpandedKillEvent, KillEvent } from "api/KillStatApi";
    import { Experience, ExpandedExpEvent, ExpEvent, ExpStatApi } from "api/ExpStatApi";

    export const SessionViewerVehicles = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            exp: { type: Array as PropType<ExpandedExpEvent[]>, required: true },
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        components: {

        }
    });

    export default SessionViewerVehicles;
</script>