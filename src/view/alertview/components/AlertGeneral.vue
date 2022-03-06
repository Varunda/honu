<template>
    <div>
        <h1 class="text-center">
            Alert {{alert.worldID}}-{{alert.instanceID}} on {{alert.zoneID | zone}} on {{alert.worldID | world}} with {{alert.participants | locale}} players
        </h1>

        <h3 class="text-center">
            From {{alert.timestamp | moment}} to {{alertEnd | moment}} ({{alert.duration | mduration}})
        </h3>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsAlert } from "api/AlertApi";

    import "filters/ZoneNameFilter";

    export const AlertGeneral = Vue.extend({
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
            alertEnd: function(): Date {
                const ms: number = this.alert.timestamp.getTime();
                const endms: number = ms + (this.alert.duration * 1000);

                return new Date(endms);
            }
        },

        components: {

        }
    });

    export default AlertGeneral;
</script>