import Vue from "vue";

Vue.filter("world", (worldID: number): string => {
	if (worldID == 1) {
		return "Connery";
	} else if (worldID == 10) {
		return "Miller";
	} else if (worldID == 13) {
		return "Cobalt";
	} else if (worldID == 17) {
		return "Emerald";
	} else if (worldID == 19) {
		return "Jaeger";
	} else if (worldID == 40) {
		return "SolTech";
	}

	return `Unknown world ID ${worldID}`;
});