import Vue from "vue";
Vue.component("player-kill-block", {
    props: {
        block: { required: true },
        title: { type: String, required: false, default: "Player" },
    },
    data: function () {
        return {};
    },
    methods: {},
    template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th style="width: 40ch">Player</th>
					<th>Kills</th>
					<th>Deaths</th>
					<th>K/D</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.entries">
					<td :title="entry.name">{{entry.name}}</td>
					<td>{{entry.kills}}</td>
					<td>{{entry.deaths}}</td>
					<td>
						{{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
					</td>
				</tr>
				<tr class="table-secondary">
				</tr>
			</tbody>
		</table>
	`
});
//# sourceMappingURL=KillData.js.map