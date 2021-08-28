import Vue, { PropType } from "vue";

Vue.component("faction-focus", {
	props: {
		focus: { type: Object as PropType<any>, required: false }
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

	template: `
		<div v-if="focus != undefined" class="focus-parent">
			<span v-if="focus.vsKills > 0" :style="{ 'flex-grow': vsWidth, 'background-color': 'var(--color-bg-vs)' }" class="focus-entry" title="Percent of kills are VS">
				{{vsWidth}}%
			</span>
			<span v-if="focus.ncKills > 0" :style="{ 'flex-grow': ncWidth, 'background-color': 'var(--color-bg-nc)' }" class="focus-entry" title="Percent of kills are NC">
				{{ncWidth}}%
			</span>
			<span v-if="focus.trKills > 0" :style="{ 'flex-grow': trWidth, 'background-color': 'var(--color-bg-tr)' }" class="focus-entry" title="Percent of kills are TR">
				{{trWidth}}%
			</span>
			<span v-if="otherKillsTotal > 0" :style="{ 'flex-grow': otherKills, 'background-color': 'var(--color-bg-ns)' }" class="focus-entry" title="Percent of kills on unknown factions">
				{{otherKillsTotal}}%
			</span>
		</div>
	`
});
