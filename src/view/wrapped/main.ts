import Vue from "vue";

import WrappedView from "./WrappedView.vue";

const vm = new Vue({
	el: "#app",

	created: function (): void {

	},

	data: {

	},

	methods: {

	},

	components: {
		WrappedView
	}
});
(window as any).vm = vm;
