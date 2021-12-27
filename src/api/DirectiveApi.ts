
export class PsDirective {
    public id: number = 0;
    public treeID: number = 0;
    public tierID: number = 0;
    public objectiveSetID: number = 0;
    public name: string = "";
    public description: string = "";
    public imageSetID: number = 0;
    public imageID: number = 0;
}

export class DirectiveApi {

    public static parse(elem: any): PsDirective {
        return {
            ...elem
        };
    }

}