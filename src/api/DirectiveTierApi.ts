
export class DirectiveTier {
    public tierID: number = 0;
    public treeID: number = 0;
    public rewardSetID: number | null = null;
    public directivePoints: number = 0;
    public completionCount: number = 0;
    public name: string = "";
    public imageSetID: number = 0;
    public imageID: number = 0;
}

export class DirectiveTierApi {

    public static parse(elem: any): DirectiveTier {
        return {
            ...elem
        };
    }

}