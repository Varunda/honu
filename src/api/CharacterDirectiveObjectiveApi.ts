
export class CharacterDirectiveObjective {
    public characterID: string = "";
    public directiveID: number = 0;
    public objectiveID: number = 0;
    public objectiveGroupID: number = 0;
    public status: number = 0;
    public stateData: number = 0;
}

export class CharacterDirectiveObjectiveApi {

    public static parse(elem: any): CharacterDirectiveObjective {
        return {
            ...elem
        };
    }

}