import Vue from "vue";

Vue.component("weapon-kills", {
	props: {
		weaponKills: { required: true },
        total: { type: Number, required: true }
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
					<th>Weapon</th>
					<th>Users</th>
					<th>Kills</th>
                    <th>Percent</th>
					<th>HSR</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in weaponKills.entries">
					<td :title="entry.name">{{entry.itemName}}</td>
					<td>
						{{entry.users}} 
					</td>
					<td>
                        {{entry.kills}}
					</td>
                    <td>
                        {{(entry.kills / total * 100).toFixed(2)}}%
                    </td>
					<td>
						{{(entry.headshotKills / entry.kills * 100).toFixed(2)}}%
					</td>
				</tr>

                <tr class="table-secondary">
                    <td colspan="4">Total</td>
                    <td>{{total}}</td>
                </tr>
			</tbody>
		</table>
	`
});