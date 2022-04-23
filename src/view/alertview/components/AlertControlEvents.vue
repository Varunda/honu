<template>
    <div style="height: 40vh; overflow: auto;">
        <table class="table w-100">
            <thead>
                <tr style="position: sticky; top: 0;">
                    <th>a</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="ev in control">
                    <td>
                        {{ev.event.id}}
                    </td>
                    <td>
                        <span v-if="ev.facility != null">
                            {{ev.facility.name}}
                        </span>
                    </td>
                </tr>
            </tbody>
        </table>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { ExpandedFacilityControlEvent } from "api/FacilityControlEventApi";

    export const AlertControlEvents = Vue.extend({
        props: {
            control: { type: Array as PropType<ExpandedFacilityControlEvent[]>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        }

    });
    export default AlertControlEvents;
</script>