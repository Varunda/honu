import Vue from "vue";
import EventBus from "EventBus";

import FactionColors from "FactionColors";
import { StatModalData } from "StatModalData";
import { CharacterExpSupportEntry } from "api/ExpStatApi";

Vue.component("block-view", {
	props: {
		block: { required: true },
		title: { type: String, required: false, default: "Player" },

		source: { type: Function, required: false, default: null },

		sourceLimit: { type: Number, required: false, default: 6 },
		sourceWorldId: { type: Number, required: false },
		sourceTeamId: { type: Number, required: false },
		sourceTitle: { type: String, required: false, default: "Supported" }
	},

	data: function () {
		return {

		}
	},

	methods: {
		getFactionColor: function (factionID: number): string {
			return FactionColors.getFactionColor(factionID);
		},

		clickHandler: async function(event: any, charID: string): Promise<void> {
			if (this.source) {
				const modalData: StatModalData = new StatModalData();
				modalData.root = event.target;
				modalData.title = this.sourceTitle;
				modalData.columnFields = [ "characterName", "amount", "percent" ];
				modalData.columnNames = [ "Character", "Amount", "Percent" ];
				modalData.loading = true;

				EventBus.$emit("set-modal-data", modalData);

				let data: CharacterExpSupportEntry[] = await this.source(charID, this.sourceWorldId, this.sourceTeamId);
				const total: number = data.reduce((acc, iter) => acc + iter.amount, 0);

				// Trim to only show the top 6 killers
				if (data.length > (this.sourceLimit + 1)) {
					const hidden: CharacterExpSupportEntry[] = data.slice(this.sourceLimit);
					data = data.slice(0, this.sourceLimit);

					data.push({
						characterID: "",
						characterName:  `${hidden.length} others`,
						amount: hidden.reduce((acc, iter) => acc + iter.amount, 0)
					});
				}

				modalData.data = data.map((iter: CharacterExpSupportEntry) => {
					return {
						...iter,
						percent: `${(iter.amount / total * 100).toFixed(2)}%`
					}
				});
				modalData.loading = false;

				EventBus.$emit("set-modal-data", modalData);
			}
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
						<a v-if="source" @click="clickHandler($event, entry.id)">
							{{entry.value}} 
						</a>

						<span v-else>
							{{entry.value}}
						</span>

						/ {{(entry.value / block.total * 100).toFixed(2)}}%
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