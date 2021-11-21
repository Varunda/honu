import Vue from "vue";

Vue.filter("faction", (factionID: number): string => {
	if (factionID == 1) {
		return "VS";
	} else if (factionID == 2) {
		return "NC";
	} else if (factionID == 3) {
		return "TR";
	} else if (factionID == 4) {
		return "NS";
	}
	return `Unchecked faction ID ${factionID}`;
});
