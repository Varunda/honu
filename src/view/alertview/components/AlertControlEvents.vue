<template>
    <div style="height: 40vh; overflow: auto;">
        <table class="table w-100 table-sticky-header" >
            <thead>
                <tr>
                    <th>Timestamp</th>
                    <th>Facility</th>
                    <th>Action</th>
                    <th>Outfit</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="ev in events">
                    <td>
                        {{ev.event.timestamp | moment}}
                    </td>
                    <td>
                        <span v-if="ev.facility != null">
                            {{ev.facility.name}}
                        </span>
                    </td>
                    <td>
                        {{ev.event.newFactionID | faction}}
                        <span v-if="ev.event.newFactionID != ev.event.oldFactionID">
                            captured from {{ev.event.oldFactionID | faction}}
                        </span>
                        <span v-else>
                            defended
                        </span>
                        with
                        {{ev.event.players}}
                        players
                    </td>
                    <td>
                        <span v-if="ev.event.oldFactionID != ev.event.newFactionID">
                            <span v-if="ev.outfit == null">
                                &lt;no outfit&gt;
                            </span>
                            <span v-else>
                                <a :href="'/o/' + ev.event.outfitID">
                                    <span v-if="ev.outfit.tag != null">
                                        [{{ev.outfit.tag}}]
                                    </span>
                                    {{ev.outfit.name}}
                                </a>
                            </span>
                        </span>
                        <span v-else>
                            --
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

    import ATable, { ACol, ABody, AFilter, AHeader } from "components/ATable";

    import "MomentFilter";
    import "filters/FactionNameFilter";

    export const AlertControlEvents = Vue.extend({
        props: {
            control: { type: Array as PropType<ExpandedFacilityControlEvent[]>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            events: function(): ExpandedFacilityControlEvent[] {
                return this.control.filter(iter => iter.event.oldFactionID != iter.event.newFactionID);
            }
        },

        components: {
            ATable, ACol, ABody, AFilter, AHeader,
        }
    });
    export default AlertControlEvents;
</script>