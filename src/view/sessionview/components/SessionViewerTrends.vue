﻿<template>
    <div>
        <div>
            <h5>What is trend?</h5>
            <p>
                Trend is the overall average over the entire session. This will change at a higher rate near the start of sessions, and lower further into a session
            </p>

            <h5>What is interval?</h5>
            <p>
                Interval is a rolling average over a smaller period of time. If there is a period where no actions are taking place, only that period will be low
            </p>
        </div>

        <div>
            <h3>Kills per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="killData"></chart-timestamp>
        </div>

        <div>
            <h3>Kill/Death ratio</h3>
            <chart-kill-death-ratio-trend :session="session" :kills="kills" :deaths="deaths"></chart-kill-death-ratio-trend>
        </div>

        <div>
            <h3>Assists per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="assistData"></chart-timestamp>
        </div>

        <div v-if="reviveData.length > 10">
            <h3>Revives per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="reviveData"></chart-timestamp>
        </div>

        <div v-if="healData.length > 10">
            <h3>Heals per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="healData"></chart-timestamp>
        </div>

        <div v-if="shieldRepairData.length > 10">
            <h3>Shield repairs per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="shieldRepairData"></chart-timestamp>
        </div>

        <div v-if="resupplyData.length > 10">
            <h3>Resupplies per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="resupplyData"></chart-timestamp>
        </div>

        <div v-if="repairData.length > 10">
            <h3>Repairs per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="repairData"></chart-timestamp>
        </div>

        <div v-if="vehicleKillData.length > 10">
            <h3>Vehicle kills per minute</h3>
            <chart-timestamp :start="session.start" :end="session.end || new Date()" :data="vehicleKillData"></chart-timestamp>
        </div>

    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";
    import { Loading, Loadable } from "Loading";

    import "MomentFilter";
    import "filters/FixedFilter";
    import "filters/LocaleFilter";

    import Chart from "chart.js/auto/auto.esm";

    import ChartTimestamp from "./ChartTimestamp.vue";
    import ChartKillDeathRatioTrend from "./ChartKillDeathRatioTrend.vue";
    import InfoHover from "components/InfoHover.vue";
    import Busy from "components/Busy.vue";

    import { ExpandedKillEvent, KillEvent, KillStatApi } from "api/KillStatApi";
    import { Experience, ExpandedExpEvent, ExpStatApi, ExperienceBlock } from "api/ExpStatApi";
    import { Session, SessionApi } from "api/SessionApi";
    import { PsCharacter, CharacterApi } from "api/CharacterApi";

    export const SessionViewerTrends = Vue.extend({
        props: {
            session: { type: Object as PropType<Session>, required: true },
            kills: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            deaths: { type: Array as PropType<ExpandedKillEvent[]>, required: true },
            exp: { type: Object as PropType<ExperienceBlock>, required: true }
        },

        data: function() {
            return {
                killData: [] as Date[],
                healData: [] as Date[],
                reviveData: [] as Date[],
                resupplyData: [] as Date[],
                repairData: [] as Date[],
                shieldRepairData: [] as Date[],
                vehicleKillData: [] as Date[],
                assistData: [] as Date[],
            }
        },

        beforeMount: function(): void {
            this.bindExp();
            this.bindKills();
        },

        methods: {
            bindExp: function(): void {
                this.healData = this.exp.events.filter(iter => Experience.isHeal(iter.experienceID)).map(iter => iter.timestamp);
                this.reviveData = this.exp.events.filter(iter => Experience.isRevive(iter.experienceID)).map(iter => iter.timestamp);
                this.resupplyData = this.exp.events.filter(iter => Experience.isResupply(iter.experienceID)).map(iter => iter.timestamp);
                this.repairData = this.exp.events.filter(iter => Experience.isMaxRepair(iter.experienceID)).map(iter => iter.timestamp);
                this.shieldRepairData = this.exp.events.filter(iter => Experience.isShieldRepair(iter.experienceID)).map(iter => iter.timestamp);
                this.vehicleKillData = this.exp.events.filter(iter => Experience.isVehicleKill(iter.experienceID)).map(iter => iter.timestamp);
                this.assistData = this.exp.events.filter(iter => Experience.isAssist(iter.experienceID)).map(iter => iter.timestamp);
            },

            bindKills: function(): void {
                this.killData = this.kills.map(iter => iter.event.timestamp);
            },

        },

        computed: {
            durationInSeconds: function(): number {
                return ((this.session.end || new Date()).getTime() - this.session.start.getTime()) / 1000;
            },
        },

        components: {
            ChartTimestamp,
            ChartKillDeathRatioTrend,
            InfoHover,
            Busy,
        }

    });
    export default SessionViewerTrends;
</script>