import Vue from "vue";

function locale(value: number | string): string {
	let val: number = 0;
	if (typeof (value) == "string") {
		val = Number.parseFloat(value);
	} else {
		val = value;
	}
	return val.toLocaleString(undefined, {
		minimumFractionDigits: (Number.isInteger(val)) ? 0 : 2,
		maximumFractionDigits: (Number.isInteger(val)) ? 0 : 2
	});
}

Vue.filter("locale", locale);
