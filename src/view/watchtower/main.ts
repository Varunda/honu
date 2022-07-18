import * as sR from "signalR";
import Vue from "vue";
import { createPopper, Instance } from "node_modules/@popperjs/core/lib/popper";

import { WorldData } from "./WorldData";
import { PopperModalData } from "popper/PopperModalData";
import { ExpStatApi } from "api/ExpStatApi";
import { WorldTagApi } from "api/WorldTagApi";
import FactionColors from "FactionColors";
import EventBus from "EventBus";

import BlockView from "./components/BlockView.vue";
import WeaponKillsView from "./components/WeaponKillsView.vue";
import KillData from "./components/KillData.vue";
import OutfitKillData from "./components/OutfitKillData.vue";
import OutfitsOnline from "./components/OutfitsOnline.vue";
import FactionFocus from "./components/FactionFocus.vue";
import WorldTag from "./components/WorldTag.vue";

import { HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage } from "components/HonuMenu";
import ContinentMetadata from "components/ContinentMetadata.vue";
import InfoHover from "components/InfoHover.vue";

import "MomentFilter";
import "filters/WorldNameFilter";
import "filters/TilFilter";

type StreamFailure = {
	streamType: "death" | "exp";
	secondsMissed: number;
}

const vm = new Vue({
	el: "#app",

	created: function(): void {
		document.title = `Honu / Realtime`;

		this.socketState = "unconnected";

		this.connection = new sR.HubConnectionBuilder()
			.withUrl("/ws/data")
			.withAutomaticReconnect([5000, 10000, 20000, 20000])
			.build();

		this.connection.on("UpdateData", (data: WorldData) => {
			data.tagEntries = data.tagEntries.map((iter: any) => WorldTagApi.readEntry(iter));
			data.realtimeHealth.forEach((iter) => {
				iter.lastEvent = new Date(iter.lastEvent);
				iter.firstEvent = (iter.firstEvent == null) ? null : new Date(iter.firstEvent);
			});
			data.reconnects.forEach((iter) => {
				iter.timestamp = new Date(iter.timestamp);
			});
			console.log(data);

			this.worldData = data;
			this.lastUpdate = new Date();
		});

		this.connection.start().then(() => {
			this.socketState = "opened";
			console.log(`connected`);
			this.subscribeBasedOnWorldPath();
		}).catch(err => {
			console.error(err);
		});

		this.connection.onreconnected(() => {
			console.log(`reconnected`);
			this.socketState = "opened";
			this.subscribeBasedOnWorldPath();
		});

		this.connection.onclose((err?: Error) => {
			this.socketState = "closed";
			if (err) {
				console.error("onclose: ", err);
			}
		});

		this.connection.onreconnecting((err?: Error) => {
			this.socketState = "reconnecting";
			if (err) {
				console.error("onreconnecting: ", err);
			}
		});

		EventBus.$on("set-modal-data", (modalData: PopperModalData) => {
			this.setModalData(modalData);
		});
	},

	data: {
		worldData: new WorldData() as WorldData,
		connection: null as sR.HubConnection | null,
		socketState: "" as string,
		lastUpdate: null as Date | null,
		trackingPeriodStart: null as Date | null,

		worldID: 0 as number,

		modalData: new PopperModalData() as PopperModalData,

		popperInstance: null as Instance | null,

		expSources: {
			charHeal: ExpStatApi.getCharacterHealEntries,
			charRevive: ExpStatApi.getCharacterReviveEntries,
			charResupply: ExpStatApi.getCharacterResupplyEntries,
			charSpawn: ExpStatApi.getCharacterSpawnEntries,
			charVKills: ExpStatApi.getCharacterVehicleKillEntries,
			charShield: ExpStatApi.getCharacterShieldEntries,

			outfitHeal: ExpStatApi.getOutfitHealEntries,
			outfitRevive: ExpStatApi.getOutfitReviveEntries,
			outfitResupply: ExpStatApi.getOutfitResupplyEntries,
			outfitSpawn: ExpStatApi.getOutfitSpawnEntries,
			outfitVKills: ExpStatApi.getOutfitVehicleKillEntries,
			outfitShield: ExpStatApi.getOutfitShieldEntries
		}

	},

	methods: {
		subscribeToWorld: function (worldID: number): void {
			if (this.connection == null) {
				console.warn(`Cannot subscribe to world ${worldID}, connection is null`);
				return;
			}

			this.worldID = worldID;

			this.connection.invoke("SubscribeToWorld", worldID).then(() => {
				console.log(`Successfully subscribed to ${worldID}`);
			}).catch((err: any) => {
				console.error(`Error subscribing to world ${worldID}: ${err}`);
			});
		},

		subscribeBasedOnWorldPath: function (): void {
			const path: string = location.pathname;
			const parts: string[] = path.split("/");

			console.log(`path: ${path}, parts: ${parts.join(", ")}`);

			if (parts.length == 3) {
				const world: string = parts[2].toLowerCase();

				if (world == "connery" || world == "1") {
					document.title = `Honu / Server / Connery`;
					this.subscribeToWorld(1);
				} else if (world == "miller" || world == "10") {
					document.title = `Honu / Server / Miller`;
					this.subscribeToWorld(10);
				} else if (world == "cobalt" || world == "13") {
					document.title = `Honu / Server / Cobalt`;
					this.subscribeToWorld(13);
				} else if (world == "emerald" || world == "17") {
					document.title = `Honu / Server / Emerald`;
					this.subscribeToWorld(17);
				} else if (world == "jaeger" || world == "jeager" || world == "19") { // common misspelling
					document.title = `Honu / Server / Jaeger`;
					this.subscribeToWorld(19);
				} else if (world == "soltech" || world == "40") {
					document.title = `Honu / Server / SolTech`;
					this.subscribeToWorld(40);
				} else {
					console.error(`Unknown world ${world}`);
				}
			}
		},

		getFactionColor: function (factionID: number): string {
			return FactionColors.getFactionColor(factionID);
		},

		setModalData: function (modalData: PopperModalData): void {
			this.modalData = modalData;

			if (this.modalData.root == null) {
				console.error(`Missing root element`);
				return;
			}

			const tooltip: HTMLElement | null = document.getElementById("stat-table");
			if (tooltip == null) {
				console.error(`Missing tooltip element '#stat-table'`);
				return;
			}

			tooltip.style.display = "block";

			if (this.popperInstance != null) {
				this.popperInstance.destroy();
				this.popperInstance = null;
			}

			const popper: Instance = createPopper(this.modalData.root, tooltip, {
				placement: "auto",
			});
			this.popperInstance = popper;
		},

		closeStatTooltip: function (): void {
			if (this.popperInstance != null) {
				this.popperInstance.destroy();

				const tooltip: HTMLElement | null = document.getElementById("stat-table");
				if (tooltip != null) {
					tooltip.style.display = "none";
				}
			}
		}

	},

	computed: {
		indarCount: function (): number {
			return this.worldData.continentCount.indar.vs
				+ this.worldData.continentCount.indar.nc
				+ this.worldData.continentCount.indar.tr
				+ this.worldData.continentCount.indar.ns;
		},

		hossinCount: function (): number {
			return this.worldData.continentCount.hossin.vs
				+ this.worldData.continentCount.hossin.nc
				+ this.worldData.continentCount.hossin.tr
				+ this.worldData.continentCount.hossin.ns;
		},

		amerishCount: function (): number {
			return this.worldData.continentCount.amerish.vs
				+ this.worldData.continentCount.amerish.nc
				+ this.worldData.continentCount.amerish.tr
				+ this.worldData.continentCount.amerish.ns;
		},

		esamirCount: function (): number {
			return this.worldData.continentCount.esamir.vs
				+ this.worldData.continentCount.esamir.nc
				+ this.worldData.continentCount.esamir.tr
				+ this.worldData.continentCount.esamir.ns;
		},

		oshurCount: function(): number {
			return this.worldData.continentCount.oshur.vs
				+ this.worldData.continentCount.oshur.nc
				+ this.worldData.continentCount.oshur.tr
				+ this.worldData.continentCount.oshur.ns;
        },

		otherCount: function (): number {
			return this.worldData.continentCount.other.vs
				+ this.worldData.continentCount.other.nc
				+ this.worldData.continentCount.other.tr
				+ this.worldData.continentCount.other.ns;
		},

		totalCount: function (): number {
			return this.worldData.onlineCount;
		},

		totalVSCount: function (): number {
			return this.worldData.continentCount.indar.vs
				+ this.worldData.continentCount.hossin.vs
				+ this.worldData.continentCount.amerish.vs
				+ this.worldData.continentCount.esamir.vs
				+ this.worldData.continentCount.oshur.vs
				+ this.worldData.continentCount.other.vs;
		},

		totalNCCount: function (): number {
			return this.worldData.continentCount.indar.nc
				+ this.worldData.continentCount.hossin.nc
				+ this.worldData.continentCount.amerish.nc
				+ this.worldData.continentCount.esamir.nc
				+ this.worldData.continentCount.oshur.nc
				+ this.worldData.continentCount.other.nc;
		},

		totalTRCount: function (): number {
			return this.worldData.continentCount.indar.tr
				+ this.worldData.continentCount.hossin.tr
				+ this.worldData.continentCount.amerish.tr
				+ this.worldData.continentCount.esamir.tr
				+ this.worldData.continentCount.oshur.tr
				+ this.worldData.continentCount.other.tr;
		},

		totalNSCount: function (): number {
			return this.worldData.continentCount.indar.ns
				+ this.worldData.continentCount.hossin.ns
				+ this.worldData.continentCount.amerish.ns
				+ this.worldData.continentCount.esamir.ns
				+ this.worldData.continentCount.oshur.ns
				+ this.worldData.continentCount.other.ns;
		},

		badStreams: function(): StreamFailure[] {
			const cutoff: Date = new Date(new Date().getTime() - (1000 * 60 * 60 * 2));

			// Because the timestamp represents the end of an outage, and the duration can extend into before the current interval
			//		it's possible that a 2 hour outage 1 hour ago will instead show at 2 hours. To prevent this, the duration of the
			//		reconnect is adjusted to only include the period the realtime is for
			for (const reconnect of this.worldData.reconnects) {
				const outageStart: Date = new Date(reconnect.timestamp.getTime() - (reconnect.duration * 1000));
				const startDiff: number = outageStart.getTime() - cutoff.getTime();
				const diff: number = -1 * Math.floor(startDiff / 1000);
				if (startDiff < 0) {
					//console.log(`outage at ${reconnect.timestamp.toISOString()} from a duration of ${reconnect.duration} - ${diff} = ${reconnect.duration - diff}`);
					reconnect.duration -= diff;
                }
            }

			const arr: StreamFailure[] = [];

			const deathCount: number = this.worldData.reconnects.filter(iter => iter.streamType == "death").reduce((acc, i) => acc += i.duration, 0);
			if (deathCount > 0) {
				arr.push({ streamType: "death", secondsMissed: deathCount });
            }

			const expCount: number = this.worldData.reconnects.filter(iter => iter.streamType == "exp").reduce((acc, i) => acc += i.duration, 0);
			if (expCount > 0) {
				arr.push({ streamType: "exp", secondsMissed: expCount });
            }

			return arr;
        },

		streamFailureCount: function(): number {
			return this.worldData.realtimeHealth.reduce((acc, i) => acc += i.failureCount, 0);
        },

		streamMostRecentEvent: function(): Date {
			let minDate: Date = new Date();

			for (const entry of this.worldData.realtimeHealth) {
				if (entry.lastEvent != null && entry.lastEvent <= minDate) {
					minDate = entry.lastEvent;
                }
			}

			return minDate;
        },

		hasFailedStream: function(): boolean {
			return this.worldData.realtimeHealth.find(iter => iter.failureCount > 0) != undefined;
        }
	},

	components: {
		ContinentMetadata,
		BlockView,
		FactionFocus,
		"PlayerKillBlock": KillData,
		"OutfitKillBlock": OutfitKillData,
		OutfitsOnline,
		"WeaponKills": WeaponKillsView,
		InfoHover,
		HonuMenu, MenuSep, MenuCharacters, MenuOutfits, MenuLedger, MenuRealtime, MenuDropdown, MenuImage,
		WorldTag
	}
});
(window as any).vm = vm;