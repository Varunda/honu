import Vue from "vue";

Vue.filter("fixed", (value: number | string, decimals: number = 2): string => {
	if (typeof (value) == "string") {
		return Number(value).toFixed(decimals || 2);
	}
	return value.toFixed(decimals || 2);
});
