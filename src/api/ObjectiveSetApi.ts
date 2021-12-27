
export class ObjectiveSet {
    public setID: number = 0;
    public groupID: number = 0;
}

export class ObjectiveSetApi {

    public static parse(elem: any): ObjectiveSet {
        return {
            ...elem
        };
    }

}