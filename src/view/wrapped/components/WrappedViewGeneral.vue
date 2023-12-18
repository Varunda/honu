<template>
    <div>
        <collapsible header-text="General">
            <div style="font-size: 16px;" class="d-flex">
                <div class="border p-2 m-3 rounded">
                    <div>
                        <strong>
                        Total playtime:
                        </strong>
                        {{totalPlaytime | mduration}}
                    </div>

                    <div>
                        <strong>
                        Kills:
                        </strong>
                        {{this.wrapped.kills.length | locale}}
                    </div>

                    <div>
                        <strong>
                        Deaths:
                        </strong>
                        <info-hover text="Revives remove a death"></info-hover>
                        {{this.wrapped.deaths.length | locale}}
                    </div>

                    <div>
                        <strong>
                            K/D:
                        </strong>
                        <info-hover text="Revive remove a death. This is revive K/D, where a revive removes a death"></info-hover>
                        {{totalKD | locale(2)}}
                    </div>

                    <div>
                        <strong>
                        KPM:
                        </strong>
                        {{totalKPM | locale(2)}}
                    </div>
                </div>

                <div class="border p-2 m-3 rounded">
                    <div>
                        <strong>
                        Teamkills:
                        </strong>
                        {{this.wrapped.teamkills.length | locale(0)}}
                    </div>

                    <div>
                        <strong>
                        Teamdeaths:
                        </strong>
                        {{this.wrapped.teamdeaths.length | locale(0)}}
                    </div>
                </div>

                <div class="border p-2 m-3 rounded">
                    <div>
                        <strong>
                        Experience earned:
                        </strong>
                        {{totalScore | locale(0)}}
                    </div>

                    <div>
                        <strong>
                        SPM:
                        </strong>
                        {{totalSPM | locale(2)}}
                    </div>
                </div>

                <div class="border p-2 m-3 rounded">
                    <div>
                        <strong>
                            Vehicles killed:
                        </strong>
                        {{this.wrapped.vehicleKill.length | locale}}
                    </div>

                    <div>
                        <strong>
                        Vehicles lost:
                        </strong>
                        {{this.wrapped.vehicleDeath.length | locale}}
                    </div>
                </div>

                <div class="border p-2 m-3 rounded">
                    <div>
                        <strong>
                        Facilities captures:
                        </strong>
                        {{facilityControlCaptureCount | locale}}
                    </div>

                    <div>
                        <strong>
                        Facilities defended:
                        </strong>
                        {{facilityControlDefendCount | locale}}
                    </div>
                </div>

            </div>
        </collapsible>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { WrappedEntry } from "api/WrappedApi";
    import Collapsible from "components/Collapsible.vue";
    import InfoHover from "components/InfoHover.vue";

    import "MomentFilter";
    import "filters/LocaleFilter";

    export const WrappedViewGeneral = Vue.extend({
        props: {
            wrapped: { type: Object as PropType<WrappedEntry>, required: true }
        },

        data: function() {
            return {

            }
        },

        methods: {

        },

        computed: {
            totalPlaytime: function(): number {
                let time: number = 0;

                for (const session of this.wrapped.sessions) {
                    if (session.end == null) {
                        continue;
                    }

                    time += (session.end.getTime() - session.start.getTime()) / 1000;
                }

                return time;
            },

            totalKD: function(): number {
                return this.wrapped.kills.length / Math.max(1, this.wrapped.deaths.length);
            },

            totalKPM: function(): number {
                return this.wrapped.kills.length / Math.max(1, this.totalPlaytime) * 60;
            },

            totalScore: function(): number {
                return this.wrapped.exp.reduce((acc, iter) => acc += iter.amount, 0);
            },

            totalSPM: function(): number {
                return this.totalScore / Math.max(1, this.totalPlaytime) * 60;
            },

            facilityControlCaptureCount: function(): number {
                return this.wrapped.controlEvents.filter(iter => iter.newFactionID != iter.oldFactionID).length;
            },

            facilityControlDefendCount: function(): number {
                return this.wrapped.controlEvents.filter(iter => iter.newFactionID == iter.oldFactionID).length;
            }
        },

        components: {
            Collapsible, InfoHover
        }

    });
    export default WrappedViewGeneral;
</script>