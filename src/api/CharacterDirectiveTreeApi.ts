
export class CharacterDirectiveTree {
    public characterID: string = "";
    public treeID: number = 0;
    public currentTier: number = 0;
    public currentLevel: number = 0;
    public completionDate: Date | null = null;
}

export class CharacterDirectiveTreeApi {

    public static parse(elem: any): CharacterDirectiveTree {
        return {
            ...elem,
            completionDate: elem.completionDate == null ? null : new Date(elem.completionDate)
        };
    }

}