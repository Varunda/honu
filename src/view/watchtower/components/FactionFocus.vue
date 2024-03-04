<template>
    <div v-if="focus != undefined" class="focus-parent">
        <span v-if="focus.vsKills > 0" :style="{ 'flex-grow': vsWidth, 'background-color': 'var(--color-bg-vs)' }" class="focus-entry" title="Percent of kills are VS">
            <span v-if="ShowCount">
                {{focus.vsKills}}
            </span>
            <span v-else>
                {{vsWidth}}%
            </span>
        </span>

        <span v-if="focus.ncKills > 0" :style="{ 'flex-grow': ncWidth, 'background-color': 'var(--color-bg-nc)' }" class="focus-entry" title="Percent of kills are NC">
            <span v-if="ShowCount">
                {{focus.ncKills}}
            </span>
            <span v-else>
                {{ncWidth}}%
            </span>
        </span>

        <span v-if="focus.trKills > 0" :style="{ 'flex-grow': trWidth, 'background-color': 'var(--color-bg-tr)' }" class="focus-entry" title="Percent of kills are TR">
            <span v-if="ShowCount">
                {{focus.trKills}}
            </span>
            <span v-else>
                {{trWidth}}%
            </span>
        </span>

        <span v-if="otherKillsTotal > 0" :style="{ 'flex-grow': otherKills, 'background-color': 'var(--color-bg-ns)' }" class="focus-entry" title="Percent of kills on unknown factions">
            {{otherKills}}%
        </span>
    </div>
</template>

<script lang="ts">
    import Vue, { PropType } from "vue";

    export const FactionFocus = Vue.extend({
        props: {
            focus: { type: Object as PropType<any>, required: false },
            ShowCount: { type: Boolean, required: false, default: false }
        },

        computed: {
            vsWidth: function (): string {
                return `${(this.focus.vsKills / this.focus.totalKills * 100).toFixed(2)}`;
            },

            ncWidth: function (): string {
                return `${(this.focus.ncKills / this.focus.totalKills * 100).toFixed(2)}`;
            },

            trWidth: function (): string {
                return `${(this.focus.trKills / this.focus.totalKills * 100).toFixed(2)}`;
            },

            otherKillsTotal: function (): number {
                return this.focus.totalKills - this.focus.vsKills - this.focus.ncKills - this.focus.trKills;
            },

            otherKills: function (): string {
                return `${(this.otherKillsTotal / this.focus.totalKills * 100).toFixed(2)}`;
            },

            totalPercent: function (): number {
                return Number.parseFloat(this.vsWidth) + Number.parseFloat(this.ncWidth) + Number.parseFloat(this.trWidth) + Number.parseFloat(this.otherKills);
            }
        },
    });
    export default FactionFocus;
</script>