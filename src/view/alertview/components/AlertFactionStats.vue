<template>
    <table class="table table-sm">
        <tr class="text-center" :style="{ 'background-color': factionColor }">
            <td colspan="2">
                <b>
                    {{data.factionID | factionLong}}
                </b>
            </td>
        </tr>

        <tr>
            <td><b>Players</b></td>
            <td>
                {{data.members | locale}}
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
            <td><b>Vehicle kills</b></td>
            <td>
                {{data.vehicleKills | locale}}
                ({{data.vehicleKills / data.secondsOnline * 60 | locale(2)}})
            </td>
        </tr>

        <tr class="text-center table-success">
            <td colspan="2">
                <b>Medic</b>
                <info-hover text="Only includes players who had at least one minute of playtime as a medic"></info-hover>
            </td>
        </tr>

        <tr>
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

        <tr class="text-center table-warning">
            <td colspan="2">
                <b>Engineer</b>
                <info-hover text="Only includes player who had at least one minute of playtime as an engineer"></info-hover>
            </td>
        </tr>

        <tr>
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
    import Vue from "vue";

    import InfoHover from "components/InfoHover.vue";

    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";

    import ColorUtils from "util/Color"

    export const AlertFactionStats = Vue.extend({
        props: {
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
                return ColorUtils.getFactionColor(this.data.factionID);
            }
        },

        components: {
            InfoHover
        }
    });
    export default AlertFactionStats;
</script>