import Vue from "vue";

import FactionColors from "FactionColors";

Vue.component("block-view", {
	props: {
		block: { required: true },
		title: { type: String, required: false, default: "Player" },
	},

	data: function () {
		return {

		}
	},

	methods: {
		getFactionColor: function (factionID: number): string {
			return FactionColors.getFactionColor(factionID);
		}

	},

	template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th>{{title}}</th>
					<th style="width: 12ch">Amount</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.entries">
					<td :title="entry.name" :style="{ color: getFactionColor(entry.factionID) }">{{entry.name}}</td>
					<td>
						{{entry.value}} / 
						{{(entry.value / block.total * 100).toFixed(2)}}%
					</td>
				</tr>
				<tr class="table-secondary">
					<th colspan="2">
						Total: {{block.total}}
					</th>
				</tr>
			</tbody>
		</table>
	`
});