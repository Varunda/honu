
export class ItemCategory {
    public id: number = 0;
    public name: string = "";
}

export class ItemCategoryApi {

    public static parse(elem: any): ItemCategory {
        return {
            id: elem.id,
            name: elem.name
        };
    }

}
