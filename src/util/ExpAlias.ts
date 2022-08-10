import { Experience } from "api/ExpStatApi";

export default class ExpAlias {

    private static _alias: Map<number, number> = new Map([
        [Experience.SQUAD_HEAL, Experience.HEAL],
        [Experience.SQUAD_REVIVE, Experience.REVIVE],
        [Experience.SQUAD_RESUPPLY, Experience.RESUPPLY],
        [Experience.SQUAD_MAX_REPAIR, Experience.MAX_REPAIR],
        [Experience.SPAWN_ASSIST, Experience.ASSIST],
        [Experience.PRIORITY_ASSIST, Experience.ASSIST],
        [Experience.HIGH_PRIORITY_ASSIST, Experience.ASSIST],
        [Experience.PRIORITY_KILL, Experience.KILL],
        [Experience.HIGH_PRIORITY_KILL, Experience.KILL]
    ]);

    public static get(expID: number): number {
        return ExpAlias._alias.get(expID) || expID;
    }

}