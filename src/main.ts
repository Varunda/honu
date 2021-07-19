import * as sR from "signalR";
import Vue from "vue";
import { createPopper, Instance } from "../node_modules/@popperjs/core/lib/popper";

import { WorldData } from "./WorldData";
import FactionColors from "./FactionColors";
import EventBus from "EventBus";

import "./BlockView";
import "./KillData";
import "./OutfitKillData";
import "./InfoHover";
import "./MomentFilter";
import { StatModalData } from "StatModalData";

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
		socketState: "" as string,
		lastUpdate: null as Date | null,
		trackingPeriodStart: null as Date | null,
		connection: null as sR.HubConnection | null,

		modalData: new StatModalData() as StatModalData,

		popperInstance: null as Instance | null,
	},

	methods: {
		subscribeToWorld: function (worldID: number): void {
			if (this.connection == null) {
				console.warn(`Cannot subscribe to world ${worldID}, connection is null`);
				return;
			}

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

		setModalData: function(modalData: StatModalData): void {
			this.modalData = modalData;

			this.$nextTick(() => {
				if (modalData.root == null) {
					console.error(`Missing root element`);
					return;
				}

				const tooltip: HTMLElement | null = document.getElementById("stat-table");
				if (tooltip == null) {
					console.error(`Missing tooltip element '#stat-table'`);
					return;
				}

				const popper: Instance = createPopper(modalData.root, tooltip, {
					placement: "auto",
				});
				this.popperInstance = popper;
			});
		},

		closeStatTooltip: function(): void {
			if (this.popperInstance != null) {
				this.popperInstance.destroy();
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

	}

});
(window as any).vm = vm;