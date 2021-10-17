import Vue from "vue";

import CharacterViewer from "./CharacterViewer.vue";

const vm = new Vue({
	el: "#app",

	created: function (): void {

	},

	data: {

	},

	methods: {

	},

	components: {
		CharacterViewer
	}
});
(window as any).vm = vm;
