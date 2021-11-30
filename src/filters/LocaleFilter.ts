import Vue from "vue";

function locale(value: number | string, digits?: number): string {
	let val: number = 0;
	if (typeof (value) == "string") {
		val = Number.parseFloat(value);
	} else {
		val = value;
	}
	return val.toLocaleString(undefined, {
		minimumFractionDigits: digits ? digits : (Number.isInteger(val)) ? 0 : 2,
		maximumFractionDigits: digits ? digits : (Number.isInteger(val)) ? 0 : 2
	});
}

Vue.filter("locale", locale);
