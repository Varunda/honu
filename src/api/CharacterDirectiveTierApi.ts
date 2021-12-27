
export class CharacterDirectiveTier {
    public characterID: string = "";
    public treeID: number = 0;
    public tierID: number = 0;
    public completionDate: Date | null = null;
}

export class CharacterDirectiveTierApi {

    public static parse(elem: any): CharacterDirectiveTier {
        return {
            ...elem,
            completionDate: elem.completionDate == null ? null : new Date(elem.completionDate)
        };
    }

}