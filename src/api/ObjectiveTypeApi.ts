
export class ObjectiveType {
    public id: number = 0;
    public description: string = "";

    public param1: string | null = null;
    public param2: string | null = null;
    public param3: string | null = null;
    public param4: string | null = null;
    public param5: string | null = null;
    public param6: string | null = null;
    public param7: string | null = null;
    public param8: string | null = null;
    public param9: string | null = null;
    public param10: string | null = null;
}

export class ObjectiveTypeApi {

    public static parse(elem: any): ObjectiveType {
        return {
            ...elem
        };
    }

}