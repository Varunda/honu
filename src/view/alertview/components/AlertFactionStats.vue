<template>
    <table class="table table-sm">
        <tr class="text-center" :style="{ 'background-color': factionColor }">
            <td colspan="2">
                <b>
                    {{data.factionID | factionLong}}
                </b>
            </td>
        </tr>

        <tr v-if="alert.zoneID != 0 && data.facilityCount > 0">
            <td><b>Facilities</b></td>
            <td>
                {{data.facilityCount}}
                ({{data.facilityCount / alert.zoneFacilityCount * 100 | locale(0)}}%)
            </td>
        </tr>

        <tr>
            <td><b>Players</b></td>
            <td>
                {{data.members | locale}}
                <info-hover text="This is not an overpop indicator. This is how many players played over the course of the alert"></info-hover>
            </td>
        </tr>

        <tr>
            <td><b>Kills (per minute)</b></td>
            <td>
                {{data.kills | locale}}
                ({{data.kills / data.secondsOnline * 60 | locale(2)}})
            </td>
        </tr>

        <tr>
            <td><b>Deaths</b></td>
            <td>{{data.deaths | locale}}</td>
        </tr>

        <tr>
            <td><b>K/D</b></td>
            <td>{{data.kills / data.deaths | locale(2)}}</td>
        </tr>

        <tr>
            <td><b>Vehicle kills (per hour)</b></td>
            <td>
                {{data.vehicleKills | locale}}
                ({{data.vehicleKills / data.secondsOnline * 60 * 60 | locale(2)}})
            </td>
        </tr>

        <tr class="text-center rounded" style="background-color: #009970;">
            <td colspan="2" class="border-top-0" style="border-radius: 0.5rem; border: 0px solid transparent;">
                <b>Medic</b>
                <info-hover text="Only includes players who had at least one minute of playtime as a medic"></info-hover>
            </td>
        </tr>

        <tr class="th-border-top-0">
            <td><b>Players</b></td>
            <td>{{data.medicPlayers | locale}}</td>
        </tr>

        <tr>
            <td><b>Revives</b></td>
            <td>
                {{data.medicRevives | locale}}
                ({{data.medicRevives / data.medicTimeAs * 60 | locale(2)}})
            </td>
        </tr>

        <tr>
            <td><b>Heals</b></td>
            <td>
                {{data.medicHeals | locale}}
                ({{data.medicHeals / data.medicTimeAs * 60 | locale(2)}})
            </td>
        </tr>

        <tr class="text-center rounded" style="background-color: #9f660c;">
            <td colspan="2" class="border-top-0" style="border-radius: 0.5rem; border: 0px solid transparent;">
                <b>Engineer</b>
                <info-hover text="Only includes player who had at least one minute of playtime as an engineer"></info-hover>
            </td>
        </tr>

        <tr class="th-border-top-0">
            <td><b>Players</b></td>
            <td>{{data.engPlayers | locale}}</td>
        </tr>

        <tr>
            <td><b>Resupplies</b></td>
            <td>
                {{data.engResupplies | locale}}
                ({{data.engResupplies / data.engTimeAs * 60 | locale(2)}})
            </td>
        </tr>

        <tr>
            <td><b>Repairs</b></td>
            <td>
                {{data.engRepairs | locale}}
                ({{data.engRepairs / data.engTimeAs * 60 | locale(2)}})
            </td>
        </tr>

    </table>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { PsAlert } from "api/AlertApi";

    import InfoHover from "components/InfoHover.vue";

    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";

    import ColorUtils from "util/Color"

    export const AlertFactionStats = Vue.extend({
        props: {
            alert: { type: Object as PropType<PsAlert>, required: true },
            data: { type: Object, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {
            
        },

        computed: {
            factionColor: function(): string {
                if (this.data.factionID == 1) {
                    return ColorUtils.BG_VS;
                } else if (this.data.factionID == 2) {
                    return ColorUtils.BG_NC;
                } else if (this.data.factionID == 3) {
                    return ColorUtils.BG_TR;
                }
                throw `Unchecked factionID '${this.data.factionID}'`;
            }
        },

        components: {
            InfoHover
        }
    });
    export default AlertFactionStats;
</script>