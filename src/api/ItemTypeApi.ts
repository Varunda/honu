
export class ItemType {
    public id: number = 0;
    public name: string = "";
    public code: string = "";
}

export class ItemTypeApi {

    public static parse(elem: any): ItemType {
        return {
            id: elem.id,
            name: elem.name,
            code: elem.code
        };
    }

}