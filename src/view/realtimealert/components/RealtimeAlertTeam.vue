<template>
    <div>
        <div>
            {{team.kills | locale(0)}}
        </div>
        <div>
            {{team.vehicleKills | locale(0)}}
        </div>
        <div>
            {{reviveCount | locale(0)}}
        </div>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    import { RealtimeAlertTeam } from "api/RealtimeAlertApi";
    import { Experience } from "api/ExpStatApi";

    import "filters/FactionNameFilter";
    import "filters/LocaleFilter";

    export const RealtimeAlertTeamView = Vue.extend({
        props: {
            team: { type: Object as PropType<RealtimeAlertTeam | null>, required: true },
            name: { type: String, required: false }
        },

        computed: {
            reviveCount: function(): number {
                if (this.team == null) {
                    return 0;
                }
                return (this.team.experience.get(Experience.REVIVE) || 0) + (this.team.experience.get(Experience.SQUAD_REVIVE) || 0);
            }
        }

    });

    export default RealtimeAlertTeamView;
</script>