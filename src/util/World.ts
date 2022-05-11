export default class WorldUtils {

    public static readonly Connery: number = 1;
    public static readonly Miller: number = 10;
    public static readonly Cobalt: number = 13;
    public static readonly Emerald: number = 17;
    public static readonly Jaeger: number = 19;
    public static readonly SolTech: number = 40;

    public static getWorldID(worldID: number): string {
        if (worldID == WorldUtils.Connery) {
            return "Connery";
        } else if (worldID == WorldUtils.Miller) {
            return "Miller";
        } else if (worldID == WorldUtils.Cobalt) {
            return "Cobalt";
        } else if (worldID == WorldUtils.Emerald) {
            return "Emerald";
        } else if (worldID == WorldUtils.Jaeger) {
            return "Jaeger";
        } else if (worldID == WorldUtils.SolTech) {
            return "SolTech";
        }

        return `Unchecked world ID ${worldID}`;
    }

}
