import { Loading, Loadable } from "Loading";
import ApiWrapper from "api/ApiWrapper";

export class HonuAccountPermission {
    public id: number = 0;
    public accountID: number = 0;
    public permission: string = "";
    public timestamp: Date = new Date();
    public grantedByID: number = 0;
}

export class HonuAccountPermissionApi extends ApiWrapper<HonuAccountPermission> {

    private static _instance: HonuAccountPermissionApi = new HonuAccountPermissionApi();
    public static get(): HonuAccountPermissionApi { return HonuAccountPermissionApi._instance; };

    public static parse(elem: any): HonuAccountPermission {
        return {
            ...elem,
            timestamp: new Date(elem.timestamp)
        };
    }

    public static async getByAccountID(accountID: number): Promise<Loading<HonuAccountPermission[]>> {
        return HonuAccountPermissionApi.get().readList(`/api/account-permission/${accountID}`, HonuAccountPermissionApi.parse);
    }

    public static async insert(accountID: number, perm: string): Promise<Loading<number>> {
        return HonuAccountPermissionApi.get().postReply(`/api/account-permission/${accountID}?permission=${perm}`, (iter: any) => iter);
    }

    public static async delete(accPermID: number): Promise<Loading<void>> {
        return HonuAccountPermissionApi.get().delete(`/api/account-permission/${accPermID}`);
    }

}
