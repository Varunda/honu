import * as sR from "signalR";
import Vue from "vue";
import { createPopper, Instance } from "node_modules/@popperjs/core/lib/popper";

import { WorldData } from "./WorldData";
import { StatModalData } from "./StatModalData";
import { ExpStatApi } from "api/ExpStatApi";
import FactionColors from "FactionColors";
import EventBus from "EventBus";

import BlockView from "./components/BlockView.vue";
import WeaponKillsView from "./components/WeaponKillsView.vue";
import KillData from "./components/KillData.vue";
import OutfitKillData from "./components/OutfitKillData.vue";
import OutfitsOnline from "./components/OutfitsOnline.vue";
import FactionFocus from "./components/FactionFocus.vue";

import ContinentMetadata from "components/ContinentMetadata.vue";
import InfoHover from "components/InfoHover.vue";
import "MomentFilter";

const vm = new Vue({
	el: "#app",

	created: function (): void {
		this.socketState = "unconnected";

		this.connection = new sR.HubConnectionBuilder()
			.withUrl("/ws/data")
			.withAutomaticReconnect([5000, 10000, 20000, 20000])
			.build();

		this.connection.on("UpdateData", (data: any) => {
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

		EventBus.$on("set-modal-data", (modalData: StatModalData) => {
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

		modalData: new StatModalData() as StatModalData,

		popperInstance: null as Instance | null,

		expSources: {
			charHeal: ExpStatApi.getCharacterHealEntries,
			charRevive: ExpStatApi.getCharacterReviveEntries,
			charResupply: ExpStatApi.getCharacterResupplyEntries,
			charSpawn: ExpStatApi.getCharacterSpawnEntries,
			charVKills: ExpStatApi.getCharacterVehicleKillEntries,

			outfitHeal: ExpStatApi.getOutfitHealEntries,
			outfitRevive: ExpStatApi.getOutfitReviveEntries,
			outfitResupply: ExpStatApi.getOutfitResupplyEntries,
			outfitSpawn: ExpStatApi.getOutfitSpawnEntries,
			outfitVKills: ExpStatApi.getOutfitVehicleKillEntries
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
					this.subscribeToWorld(1);
				} else if (world == "miller" || world == "10") {
					this.subscribeToWorld(10);
				} else if (world == "cobalt" || world == "13") {
					this.subscribeToWorld(13);
				} else if (world == "emerald" || world == "17") {
					this.subscribeToWorld(17);
				} else if (world == "jaeger" || world == "jeager" || world == "19") { // common misspelling
					this.subscribeToWorld(19);
				} else if (world == "soltech" || world == "40") {
					this.subscribeToWorld(40);
				} else {
					console.error(`Unknown world ${world}`);
				}
			}
		},

		getFactionColor: function (factionID: number): string {
			return FactionColors.getFactionColor(factionID);
		},

		setModalData: function (modalData: StatModalData): void {
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
				+ this.worldData.continentCount.other.vs;
		},

		totalNCCount: function (): number {
			return this.worldData.continentCount.indar.nc
				+ this.worldData.continentCount.hossin.nc
				+ this.worldData.continentCount.amerish.nc
				+ this.worldData.continentCount.esamir.nc
				+ this.worldData.continentCount.other.nc;
		},

		totalTRCount: function (): number {
			return this.worldData.continentCount.indar.tr
				+ this.worldData.continentCount.hossin.tr
				+ this.worldData.continentCount.amerish.tr
				+ this.worldData.continentCount.esamir.tr
				+ this.worldData.continentCount.other.tr;
		},

		totalNSCount: function (): number {
			return this.worldData.continentCount.indar.ns
				+ this.worldData.continentCount.hossin.ns
				+ this.worldData.continentCount.amerish.ns
				+ this.worldData.continentCount.esamir.ns
				+ this.worldData.continentCount.other.ns;
		},
	},

	components: {
		ContinentMetadata,
		BlockView,
		FactionFocus,
		"PlayerKillBlock": KillData,
		"OutfitKillBlock": OutfitKillData,
		OutfitsOnline,
		"WeaponKills": WeaponKillsView,
		InfoHover
	}
});
(window as any).vm = vm;