
export class PsVehicle {
    public id: number = 0;
    public name: string = "";
    public description: string = "";
}

export class VehicleApi {

    public static parse(elem: any): PsVehicle {
        return {
            ...elem
        };
    }

}