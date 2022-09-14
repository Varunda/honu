import Vue from "vue";
import RealtimeData from "./RealtimeData.vue";

const vm = new Vue({
	el: "#app",

	components: {
		RealtimeData
    }
});
(window as any).vm = vm;