import Vue from "vue";

import Mainpage from "./Mainpage.vue";

const vm = new Vue({
	el: "#app",

	created: function(): void {

	},

	data: {

	},

	methods: {

	},

	components: {
		Mainpage
	}
});
(window as any).vm = vm;
