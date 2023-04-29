
export class EntityFought {
    public id: string = "";
    public type: "outfit" | "character" | "unset" = "unset";
    public displayName: string = "";
    public factionID: number = 0;
    public worldID: number = 0;

    public kills: number = 0;
    public deaths: number = 0;
    public kd: number = 0;

    public headshotKills: number = 0;
    public headshotKillRatio: number = 0;
    public headshotDeaths: number = 0;
    public headshotDeathRatio: number = 0;

    public hipKills: number = 0;
    public hipKillRatio: number = 0;
    public adsKills: number = 0;
    public adsKillRatio: number = 0;

    public hipDeaths: number = 0;
    public hipDeathRatio: number = 0;
    public adsDeaths: number = 0;
    public adsDeathRatio: number = 0;

    /**
     * Mutate the passed parameter to update the ratio values in the instance
     * @param elem
     */
    public static updateRatios(elem: EntityFought): EntityFought {
        elem.kd = elem.kills / Math.max(1, elem.deaths);
        elem.headshotKillRatio = elem.headshotKills / Math.max(1, elem.kills);
        elem.headshotDeathRatio = elem.headshotDeaths / Math.max(1, elem.deaths);
        elem.hipKillRatio = elem.hipKills / Math.max(1, elem.kills);
        elem.adsKillRatio = elem.adsKills / Math.max(1, elem.kills);
        elem.hipDeathRatio = elem.hipDeaths / Math.max(1, elem.deaths);
        elem.adsDeathRatio = elem.adsDeaths / Math.max(1, elem.deaths);

        return elem;
    }

}

export class EntitySupported {
    public id: string = "";
    public type: "outfit" | "character" | "unset" = "unset";
    public displayName: string = "";

    public heals: number = 0;
    public healthHealed: number = 0;
    public revives: number = 0;
    public resupplies: number = 0;
    public maxRepairs: number = 0;
    public maxHealthRepairs: number = 0;
    public assists: number = 0;
    public shieldRepairs: number = 0;

}