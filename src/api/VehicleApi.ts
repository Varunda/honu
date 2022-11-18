
import { Loading, Loadable } from "Loading";
import ApiWrapper from "./ApiWrapper";

export class PsVehicle {
    public id: number = 0;
    public name: string = "";
    public description: string = "";
    public imageID: number = 0;
    public imageSetID: number = 0;
}

export class VehicleApi extends ApiWrapper<PsVehicle> {

    private static _instance: VehicleApi = new VehicleApi();
    public static get(): VehicleApi { return VehicleApi._instance; }

    public static parse(elem: any): PsVehicle {
        return {
            ...elem
        };
    }

    public static async getAll(): Promise<Loading<PsVehicle[]>> {
        return VehicleApi.get().readList(`/api/vehicle`, VehicleApi.parse);
    }

}