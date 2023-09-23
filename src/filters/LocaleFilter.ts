import Vue from "vue";

function locale(value: number | string, digits?: number): string {
	let val: number = 0;
	if (typeof (value) == "string") {
		val = Number.parseFloat(value);
	} else {
		val = value;
	}

	let dig: number = digits ?? (Number.isInteger(val) ? 0 : 2);

	return val.toLocaleString(undefined, {
		minimumFractionDigits: dig,
		maximumFractionDigits: dig
	});
}

Vue.filter("locale", locale);
