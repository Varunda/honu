
export class DirectiveTreeCategory {
    public id: number = 0;
    public name: string = "";
}

export class DirectiveTreeCategoryApi {

    public static parse(elem: any): DirectiveTreeCategory {
        return {
            ...elem
        };
    }

}