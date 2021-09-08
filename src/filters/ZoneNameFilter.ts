import Vue from "vue";

Vue.filter("zone", (zoneID: number): string => {
	if (zoneID == 2) {
		return "Indar";
	} else if (zoneID == 4) {
		return "Hossin";
	} else if (zoneID == 6) {
		return "Amerish";
	} else if (zoneID == 8) {
		return "Esamir";
	}

	return `Unknown ${zoneID}`;
});