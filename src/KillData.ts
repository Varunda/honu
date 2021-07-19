import Vue from "vue";
import EventBus from "EventBus";

import FactionColors from "FactionColors";
import { StatModalData } from "StatModalData";
import { CharacterKillApi, CharacterWeaponKillEntry } from "api/CharacterKillApi";

Vue.component("player-kill-block", {
	props: {
		block: { required: true },
		title: { type: String, required: false, default: "Player" }
	},

	data: function () {
		return {

		}
	},

	methods: {
		getFactionColor: function(factionID: number): string {
			return FactionColors.getFactionColor(factionID);
		},

		openCharacterWeaponKills: async function(event: any, charID: string): Promise<void> {
			const modalData: StatModalData = new StatModalData();

			const target: any = event.target;

			const kills: CharacterWeaponKillEntry[] = await CharacterKillApi.getWeaponEntries(charID);

			const totalKills: number = kills.reduce((acc, iter) => acc + iter.kills, 0);

			console.log(`Total kills: ${totalKills}`);

			modalData.title = "Weapon usage";
			modalData.columnFields = [ "weaponName", "kills", "headshotRatio", "percent" ];
			modalData.columnNames = [ "Weapon", "Kills", "Headshots", "Usage" ];
			modalData.root = target;

			modalData.data = kills.map((iter: CharacterWeaponKillEntry) => {
				return {
					...iter,
					headshotRatio: `${(iter.headshotKills / iter.kills * 100).toFixed(2)}%`,
					percent: `${(iter.kills / totalKills * 100).toFixed(2)}%`
				}
			});

			EventBus.$emit("set-modal-data", modalData);
		}
	},

	template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th style="width: 30ch">Player</th>
					<th>Kills</th>
					<th title="Kills / Minutes Online">KPM</th>
					<th title="Revives remove deaths">Deaths</th>
					<th>Assists</th>
					<th title="Kills / Deaths">K/D</th>
					<th title="(Kills + Assists) / Deaths">KDA</th>
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

							<span style="flex-grow: 1; overflow: hidden; text-overflow: ellipsis;" :style="{ color: getFactionColor(entry.factionID) }">
								{{entry.name}}
							</span>

							<span style="flex-grow: 0;" title="hours:minutes">
								{{entry.secondsOnline | duration}}
							</span>
						</span>
					</td>
					<td>
						<a @click="openCharacterWeaponKills($event, entry.id)" href="#">{{entry.kills}}</a>
					</td>
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
