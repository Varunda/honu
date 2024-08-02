import Vue from "vue";

import OnlineViewer from "./OnlineViewer.vue";

const vm = new Vue({
	el: "#app",

	created: function (): void {

	},

	data: {

	},

	methods: {

	},

	components: {
		OnlineViewer
	}
});
(window as any).vm = vm;
