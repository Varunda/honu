import Vue from "vue";

Vue.component("player-kill-block", {
	props: {
		block: { required: true },
		title: { type: String, required: false, default: "Player" },
		seconds: { type: Number, required: true },
	},

	data: function () {
		return {

		}
	},

	methods: {

	},

	template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th style="width: 30ch">Player</th>
					<th title="NSO not included">Kills</th>
					<th>KPM</th>
					<th title="Revives remove deaths">Deaths</th>
					<th>Assists</th>
					<th>K/D</th>
					<th>KDA</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.playerKills.entries">
					<td :title="entry.name">
						<span style="display: flex;">
							<span v-if="entry.online == true" style="color: green;" title="Online">
								●
							</span>
							<span v-else style="color: red;" title="Offline">
								●
							</span>

							<span style="flex-grow: 1; overflow: hidden; text-overflow: ellipsis;">
								{{entry.name}}
							</span>

							<span style="flex-grow: 1;" title="hours:minutes">
								{{entry.secondsOnline | duration}}
							</span>
						</span>
					</td>
					<td>{{entry.kills}}</td>
					<td>{{(entry.kills / (entry.secondsOnline / 60)).toFixed(2)}}</td>
					<td>{{entry.deaths}}</td>
					<td>{{entry.assists}}</td>
					<td>
						{{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
					</td>
					<td>
						{{((entry.kills + entry.assists) / (entry.deaths || 1)).toFixed(2)}}
					</td>
				</tr>
				<tr class="table-secondary">
					<td><b>Total</b></td>
					<td colspan="2">{{block.totalKills}}</td>
					<td>{{block.totalDeaths}}</td>
					<td>{{block.totalAssists}}</td>
					<td>
						{{(block.totalKills / (block.totalDeaths || 1)).toFixed(2)}}
					</td>
					<td>
						{{((block.totalKills + block.totalAssists) / (block.totalDeaths || 1)).toFixed(2)}}
					</td>
				</tr>
			</tbody>
		</table>
	`
});
