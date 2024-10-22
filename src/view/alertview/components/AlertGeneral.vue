<template>
    <div>
        <h1 class="text-center">
            Alert {{alert.displayID}} // {{alert.type}}
        </h1>

        <h3 class="text-center">
            took place on
            <span v-if="alert.zoneID != 0">
                {{alert.zoneID | zone}}
            </span>
            <span v-else>
                all continents
            </span>

            on
            <span>
                {{alert.worldID | world}}
            </span>

            <span v-if="showPlayers">
                with {{alert.participants | locale}} players
            </span>
        </h3>

        <h4 class="text-center">
            From {{alert.timestamp | moment}} to {{alertEnd | moment}} ({{alert.duration | mduration}})
        </h4>

        <h4 v-if="alert.instanceID != 0 && alert.zoneID != 0" class="text-center">
            Other sites:

            <a :href="'https://ps2alerts.com/alert/' + alert.displayID" target="_blank">
                PS2Alerts
            </a>
            
            &middot; 

            <!-- 
                voidwell appends 18 to the instance IDs
                https://github.com/voidwell/Voidwell.DaybreakGames/blob/master/src/Voidwell.DaybreakGames.Live/CensusStream/EventProcessors/MetagameEventProcessor.cs#L48
            -->
            <a :href="'https://voidwell.com/ps2/alerts/' + alert.worldID + '/' + alert.instanceID + '18'" target="_blank">
                Voidwell
            </a>
        </h4>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { PsAlert } from "api/AlertApi";

    import "filters/ZoneNameFilter";
    import "filters/WorldNameFilter";

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
            },

            showPlayers: function(): boolean {
                return this.alertEnd.getTime() <= new Date().getTime();
            }
        },

        components: {

        }
    });

    export default AlertGeneral;
</script>