import Vue from "vue";

import CharacterViewer from "./CharacterViewer.vue";

const vm = new Vue({
	el: "#app",

	created: function (): void {

	},

	data: {
		debug: {
			directive: false as boolean
        }
	},

	methods: {

	},

	components: {
		CharacterViewer
	}
});
(window as any).vm = vm;
