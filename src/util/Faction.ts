
export default class FactionUtils {

    public static VS: number = 1;

    public static NC: number = 2;

    public static TR: number = 3;

    public static NS: number = 4;

    public static getName(factionID: number): string {
        if (factionID == FactionUtils.VS) {
            return "VS";
        } else if (factionID == FactionUtils.NC) {
            return "NC";
        } else if (factionID == FactionUtils.TR) {
            return "TR";
        } else if (factionID == FactionUtils.NS) {
            return "NS";
        } else {
            return `Unchecked ${factionID}`;
        }
    }

}