import Vue from "vue";

Vue.filter("world", (worldID: number): string => {
	console.log(worldID);
	if (worldID == 1) {
		return "Connery";
	} else if (worldID == 3) {
		return "Helios (probably)";
	} else if (worldID == 10) {
		return "Miller";
	} else if (worldID == 13) {
		return "Cobalt";
	} else if (worldID == 17) {
		return "Emerald";
	} else if (worldID == 19) {
		return "Jaeger";
	} else if (worldID == 24) {
		return "Apex";
	} else if (worldID == 25) {
		return "Briggs";
	} else if (worldID == 40) {
		return "SolTech";
	} else if (worldID == 1000) {
		return "Genudine";
	} else if (worldID == 2000) {
		return "Ceres";
    }

	return `Unknown world ID ${worldID}`;
});