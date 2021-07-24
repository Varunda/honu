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
					<th style="width: 12ch">Kills</th>
                    <th style="width: 20ch">Percent</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in weaponKills.entries">
					<td :title="entry.name">{{entry.itemName}}</td>
					<td>
                        {{entry.kills}}
					</td>
                    <td>
                        {{(entry.kills / total * 100).toFixed(2)}}%
                    </td>
				</tr>

                <tr class="table-secondary">
                    <td colspan="2">Total</td>
                    <td>{{total}}</td>
                </tr>
			</tbody>
		</table>
	`
});