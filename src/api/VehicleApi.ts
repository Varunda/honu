
export class PsVehicle {
    public id: number = 0;
    public name: string = "";
    public description: string = "";
    public imageID: number = 0;
    public imageSetID: number = 0;
}

export class VehicleApi {

    public static parse(elem: any): PsVehicle {
        return {
            ...elem
        };
    }

}