<template>
    <div style="background-color: var(--light)" class="text-dark px-2 py-1">
        <div v-if="team == null">

        </div>
        <div v-else>
            <h4 class="border-bottom text-center" style="border-color: var(--dark) !important; width: 200px;">
                <span v-if="name">
                    {{name}}
                </span>
                <span v-else>
                    {{team.teamID | faction}}
                </span>
            </h4>
            <span class="w-50 d-inline-block">
                K: {{team.kills | locale(0)}}
            </span>
            <span class="w-50">
                D: {{team.deaths | locale(0)}}
            </span>

            <div>
                Revives: {{reviveCount | locale(0)}}
            </div>

            <span class="w-50 d-inline-block">
                VK: {{team.vehicleKills | locale(0)}}
            </span>
            <span>
                VD: {{team.vehicleDeaths | locale(0)}}
            </span>
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