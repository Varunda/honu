

export class Achievement {
    public id: number = 0;
    public itemID: number | null = null;
    public objectiveGroupID: number = 0;
    public rewardID: number | null = null;
    public repeatable: boolean = true;
    public name: string = "";
    public description: string = "";
    public imageSetID: number = 0;
    public imageID: number = 0;
}

export class AchievementApi {

    public static parse(elem: any): Achievement {
        return {
            ...elem
        };
    }

}