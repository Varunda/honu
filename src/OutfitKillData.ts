import Vue from "vue";

Vue.component("outfit-kill-block", {
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
					<th style="width: 30ch">Outfit</th>
					<th>Kills</th>
					<th>Deaths</th>
					<th>K/D</th>
					<th>Online</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.entries">
					<td :title="entry.name">[{{entry.tag}}] {{entry.name}}</td>
					<td>
						{{entry.kills}}
						({{(entry.kills / (entry.members || 1)).toFixed(2)}})
					</td>
					<td>
						{{entry.deaths}}
						({{(entry.deaths / (entry.members || 1)).toFixed(2)}})
					</td>
					<td>
						{{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
					</td>
					<td>{{entry.members}}</td>
				</tr>
			</tbody>
		</table>
	`
});
