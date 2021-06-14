import Vue from "vue";

Vue.component("player-kill-block", {
	props: {
		block: { required: true },
		title: { type: String, required: false, default: "Player" },
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
					<th style="width: 40ch">Player</th>
					<th>Kills</th>
					<th>Deaths</th>
					<th>Assists</th>
					<th>K/D</th>
					<th>KDA</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.playerKills.entries">
					<td :title="entry.name">{{entry.name}}</td>
					<td>{{entry.kills}}</td>
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
					<td>{{block.totalKills}}</td>
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
