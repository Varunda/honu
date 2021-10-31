import Vue from "vue";

Vue.filter("fixed", (value: number | string, decimals: number = 2): string => {
	const val: number = typeof (value) == "string" ? Number(value) : value;
	return val.toFixed(decimals || 2);
});
