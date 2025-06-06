import Vue from "vue";

import Pat from "./Pat.vue";

const vm = new Vue({
	el: "#app",

	created: function (): void {

	},

	data: {

	},

	methods: {

	},

	components: {
		Pat
	}
});
(window as any).vm = vm;