import Vue from "vue";

import SessionViewer from "./SessionViewer.vue";

const vm = new Vue({
	el: "#app",

	created: function (): void {

	},

	data: {

	},

	methods: {

	},

	components: {
		SessionViewer
	}
});
(window as any).vm = vm;
