
export class PsMetagameEvent {
    public id: number = 0;
    public name: string = "";
    public description: string = "";
    public typeID: number = 0;
    public durationMinutes: number = 0;
}

export class MetagameEventApi {

    public static parse(elem: any): PsMetagameEvent {
        return {
            ...elem
        };
    }

}