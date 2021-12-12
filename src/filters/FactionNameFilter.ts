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

Vue.filter("factionLong", (factionID: number): string => {
	if (factionID == 1) {
		return "Vanu Sovereignty";
	} else if (factionID == 2) {
		return "New Conglomerate";
	} else if (factionID == 3) {
		return "Terran Republic";
	} else if (factionID == 4) {
		return "Nanite System Operatives";
	}
	return `Unchecked faction ID ${factionID}`;
});
