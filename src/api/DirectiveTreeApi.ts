
export class DirectiveTree {
    public id: number = 0;
    public categoryID: number = 0;
    public name: string = "";
    public imageSetID: number = 0;
    public imageID: number = 0;
}

export class DirectiveTreeApi {

    public static parse(elem: any): DirectiveTree {
        return {
            ...elem
        };
    }

}