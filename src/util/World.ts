export default class WorldUtils {

    public static readonly Connery: number = 1;
    public static readonly Osprey: number = 1;
    public static readonly Miller: number = 10;
    public static readonly Wainwright: number = 10;
    public static readonly Cobalt: number = 13;
    public static readonly Emerald: number = 17;
    public static readonly Jaeger: number = 19;
    public static readonly SolTech: number = 40;

    public static readonly Genudine: number = 1000;
    public static readonly Ceres: number = 2000;

    public static getWorldID(worldID: number): string {
        if (worldID == WorldUtils.Osprey) {
            return "Osprey (US)";
        } else if (worldID == WorldUtils.Wainwright) {
            return "Wainwright (EU)";
        } else if (worldID == WorldUtils.Cobalt) {
            return "Cobalt";
        } else if (worldID == WorldUtils.Emerald) {
            return "Emerald";
        } else if (worldID == WorldUtils.Jaeger) {
            return "Jaeger";
        } else if (worldID == WorldUtils.SolTech) {
            return "SolTech";
        } else if (worldID == WorldUtils.Genudine) {
            return "Genudine";
        } else if (worldID == WorldUtils.Ceres) {
            return "Ceres";
        }

        return `Unchecked world ID ${worldID}`;
    }

}
