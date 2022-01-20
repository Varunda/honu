
export default class FactionColors {

	public static VS: string = "#cf17cf";

	public static NC: string = "#3f7fff";

	public static TR: string = "#ea5e5e";

	public static NS: string = "#cbcbcb";

	public static getFactionColor(factionID: number): string {
		if (factionID == 1) {
			return FactionColors.VS;
		} else if (factionID == 2) {
			return FactionColors.NC;
		} else if (factionID == 3) {
			return FactionColors.TR;
		} else if (factionID == 4) {
			return FactionColors.NS;
		} 

		return "";
	}

}