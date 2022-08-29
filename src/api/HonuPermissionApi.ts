import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class HonuPermission {
    public id: string = "";
    public description: string = "";
}

export class HonuPermissionApi extends ApiWrapper<HonuPermission> {
    private static _instance: HonuPermissionApi = new HonuPermissionApi();
    public static get(): HonuPermissionApi { return HonuPermissionApi._instance; };

    public static parse(elem: any): HonuPermission {
        return {
            ...elem,
        };
    }

    public static async getAll(): Promise<Loading<HonuPermission[]>> {
        return HonuPermissionApi.get().readList(`/api/permission/`, HonuPermissionApi.parse);
    }

}
