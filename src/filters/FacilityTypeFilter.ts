import Vue from "vue";

Vue.filter("facilityType", (typeID: number): string => {

	if (typeID == 1) {
		return "default";
	} else if (typeID == 2) {
		return "Amp station";
	} else if (typeID == 3) {
		return "Biolab";
	} else if (typeID == 4) {
		return "Tech plant";
	} else if (typeID == 5) {
		return "Large outpost";
	} else if (typeID == 6) {
		return "Small outpost";
	} else if (typeID == 7) {
		return "Warpgate";
	} else if (typeID == 8) {
		return "Interlink";
	} else if (typeID == 9) {
		return "Construction outpost";
	} else if (typeID == 10) {
		return "Relic";
	} else if (typeID == 11) {
		return "Containment site";
	} else if (typeID == 12) {
		return "Trident relay";
	} else if (typeID == 13) {
		return "Seapost";
	} else if (typeID == 14) {
		return "Large CTF outpost";
	} else if (typeID == 15) {
		return "Small CTF outpost";
	} else if (typeID == 16) {
		return "Amp station CTF";
	} else if (typeID == 17) {
		return "Construction outpost CTF";
	}

	return `unchecked facility type ${typeID}`;
});
